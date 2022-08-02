// -----------------------------------------------------------------------
//  <copyright file="ShoppingCartMessages.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Abstraction.Messages;

public static class ShoppingCart
{
    public static Option<(string, object)> ExtractEntityId(object message)
        => (((ShoppingCartBase)message).UserId, message);

    public static string ExtractShardId(object message)
        => ((ShoppingCartBase)message).UserId;
    
    public abstract class ShoppingCartBase
    {
        protected ShoppingCartBase(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
    
    public sealed class AddOrUpdateItem: ShoppingCartBase
    {
        public AddOrUpdateItem(string userId, int quantity, ProductDetails product) : base(userId)
        {
            Quantity = quantity;
            Product = product;
        }

        public int Quantity { get; }
        public ProductDetails Product { get; }
    }

    public sealed class RemoveItem: ShoppingCartBase
    {
        public RemoveItem(string userId, ProductDetails product) : base(userId)
        {
            Product = product;
        }

        public ProductDetails Product { get; }
    }
    
    public sealed class GetAllItems: ShoppingCartBase
    {
        public GetAllItems(string userId) : base(userId)
        {
        }
    }
    
    public sealed class GetTotalItemsInCart: ShoppingCartBase
    {
        public GetTotalItemsInCart(string userId) : base(userId)
        {
        }
    }
    
    public sealed class EmptyCart: ShoppingCartBase
    {
        public EmptyCart(string userId) : base(userId)
        {
        }
    }
}