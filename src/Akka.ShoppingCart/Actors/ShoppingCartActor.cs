// -----------------------------------------------------------------------
//  <copyright file="ShoppingCartActor.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Actors;

public class ShoppingCartActor: ReceivePersistentActor
{
    public static Props Props(string userId)
        => Actor.Props.Create(() => new ShoppingCartActor(userId));

    private Dictionary<string, CartItem> _cart = new ();
    private readonly IActorRef _productRegion;

    public override string PersistenceId { get; }

    public ShoppingCartActor(string userId)
    {
        PersistenceId = userId;

        var resolver = DependencyResolver.For(Context.System);
        var registry = resolver.Resolver.GetService<ActorRegistry>();
        _productRegion = registry.Get<RegistryKey.ProductRegion>();

        #region Recovery

        Recover<SnapshotOffer>(msg =>
        {
            _cart = (Dictionary<string, CartItem>)msg.Snapshot;
        });
        
        Recover<Messages.ShoppingCart.AddOrUpdateItem>(msg =>
        {
            _cart.AddOrSet(msg.Product.Id, ToCartItem(msg.Quantity, msg.Product));
        });

        Recover<Messages.ShoppingCart.EmptyCart>(_ =>
        {
            _cart.Clear();
        });

        Recover<Messages.ShoppingCart.RemoveItem>(msg =>
        {
            _cart.Remove(msg.Product.Id);
        });
        #endregion
        
        CommandAsync<Messages.ShoppingCart.AddOrUpdateItem>(async message =>
        {
            var sender = Sender;
            var product = message.Product;
            var quantity = message.Quantity;
            
            int? adjustedQuantity = null;

            if (_cart.TryGetValue(product.Id, out var existingItem))
            {
                adjustedQuantity = quantity - existingItem.Quantity;
            }

            var (isAvailable, claimedProduct) = await _productRegion.Ask<Product.TakeResult>(
                new Product.TryTake(product.Id, adjustedQuantity ?? quantity));

            if (isAvailable && claimedProduct is not null)
            {
                var persisted =
                    new Messages.ShoppingCart.AddOrUpdateItem(PersistenceId, quantity, claimedProduct);
                _cart.AddOrSet(claimedProduct.Id, ToCartItem(persisted.Quantity, persisted.Product));
                SaveSnapshot(_cart);
                sender.Tell(true);
            }
            else
            {
                sender.Tell(false);
            }
        });
        
        CommandAsync<Messages.ShoppingCart.EmptyCart>(async _ =>
        {
            var sender = Sender;
            foreach (var item in _cart.Values)
            {
                await _productRegion.Ask<Done>(new Product.Return(item.Product.Id, item.Quantity));
            }
            _cart.Clear();
            SaveSnapshot(_cart);
            sender.Tell(Done.Instance);
        });

        Command<Messages.ShoppingCart.GetAllItems>(_ =>
        {
            Sender.Tell(_cart.Values.ToHashSet());
        });
        
        Command<Messages.ShoppingCart.GetTotalItemsInCart>(_ =>
        {
            Sender.Tell(_cart.Count);
        });
        
        CommandAsync<Messages.ShoppingCart.RemoveItem>(async message =>
        {
            var sender = Sender;
            var product = message.Product;
            if (!_cart.TryGetValue(product.Id, out var cartItem))
                return;
            
            await _productRegion.Ask<Done>(new Product.Return(product.Id, cartItem.Quantity));

            if (_cart.ContainsKey(product.Id))
            {
                _cart.Remove(product.Id);
                SaveSnapshot(_cart);
            }
            sender.Tell(Done.Instance);
        });
    }
    
    protected override bool Receive(object message)
    {
        if (message is SaveSnapshotSuccess)
            return true;
        
        return base.Receive(message);
    }

    private CartItem ToCartItem(int quantity, ProductDetails product) =>
        new(PersistenceId, quantity, product);
}