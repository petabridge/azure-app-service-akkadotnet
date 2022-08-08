// -----------------------------------------------------------------------
//  <copyright file="ProductActor.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Actors;

public class ProductActor: ReceivePersistentActor
{
    public static Props Props(string productId)
        => Actor.Props.Create(() => new ProductActor(productId));
    
    private ProductDetails _product;
    private readonly IActorRef _inventory;

    public override string PersistenceId => _product.Id;

    public ProductActor(string persistenceId)
    {
        _product = new ProductDetails{Id = persistenceId};
        
        var resolver = DependencyResolver.For(Context.System);
        var registry = resolver.Resolver.GetService<ActorRegistry>();
        _inventory = registry.Get<RegistryKey.InventoryRegion>();

        #region Recovery

        Recover<SnapshotOffer>(offer =>
        {
            _product = (ProductDetails)offer.Snapshot;
        });
        Recover<Product.Return>(msg =>
        {
            _product = _product with
            {
                Quantity = _product.Quantity + msg.Quantity
            };
        });
        Recover<Product.TryTake>(msg =>
        {
            _product = _product with
            {
                Quantity = _product.Quantity - msg.Quantity
            };
        });
        Recover<Product.CreateOrUpdate>(msg =>
        {
            _product = msg.ProductDetails;
        });

        #endregion
        
        #region Commands
        
        Command<Product.GetAvailability>(_ => Sender.Tell(_product.Quantity, Self));
        Command<Product.GetDetails>(_ => Sender.Tell(_product, Self));
        Command<Product.Return>(message =>
        {
            var sender = Sender;
            Persist(message, msg =>
            {
                UpdateState(_product with
                {
                    Quantity = _product.Quantity + msg.Quantity
                }).PipeTo(Self, success: () => new Product.CommandCompleted(message, sender));
            });
        });
        Command<Product.TryTake>(message =>
        {
            if (_product.Quantity < message.Quantity)
            {
                Sender.Tell(new Product.TakeResult(false));
                return;
            }

            var sender = Sender;
            Persist(message, msg =>
            {
                UpdateState(_product with
                {
                    Quantity = _product.Quantity - msg.Quantity
                }).PipeTo(Self, success: () => new Product.CommandCompleted(message, sender));
            });
        });
        Command<Product.CreateOrUpdate>(message =>
        {
            var sender = Sender;
            Persist(message, msg =>
            {
                UpdateState(msg.ProductDetails).PipeTo(Self, success: () => new Product.CommandCompleted(message, sender));
            });
        });

        #endregion
    }

    protected override bool Receive(object message)
    {
        switch (message)
        {
            case SaveSnapshotSuccess :
                return true;
            case Product.CommandCompleted evt:
                evt.ReplyTo.Tell(Done.Instance);
                return true;
            default:
                return base.Receive(message);
        }
    }

    private async Task UpdateState(ProductDetails product)
    {
        var oldCategory = _product.Category;
        
        _product = product;
        SaveSnapshot(_product);
        
        await _inventory.Ask<Done>(new Inventory.AddOrUpdateProduct(product.Category, product));

        if (oldCategory != product.Category)
        {
            // If category changed, remove the product from the old inventory actor.
            await _inventory.Ask<Done>(new Inventory.RemoveProduct(oldCategory, product.Id));
        }
    }
}