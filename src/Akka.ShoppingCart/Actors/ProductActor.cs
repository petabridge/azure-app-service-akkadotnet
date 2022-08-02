// -----------------------------------------------------------------------
//  <copyright file="ProductActor.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
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
            Persist(message, msg =>
            {
                UpdateState(_product with
                {
                    Quantity = _product.Quantity + msg.Quantity
                });
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
                });
                sender.Tell(new Product.TakeResult(true, _product));
            });
        });
        Command<Product.CreateOrUpdate>(message =>
        {
            Persist(message, msg =>
            {
                UpdateState(msg.ProductDetails);
            });
        });

        #endregion
    }

    protected override bool Receive(object message)
    {
        if (message is SaveSnapshotSuccess)
            return true;
        
        return base.Receive(message);
    }

    private void UpdateState(ProductDetails product)
    {
        var oldCategory = _product.Category;
        
        _product = product;
        SaveSnapshot(_product);
        
        _inventory.Tell(new Inventory.AddOrUpdateProduct(product.Category, product));

        if (oldCategory != product.Category)
        {
            // If category changed, remove the product from the old inventory actor.
            _inventory.Tell(new Inventory.RemoveProduct(oldCategory, product.Id));
        }
    }
}