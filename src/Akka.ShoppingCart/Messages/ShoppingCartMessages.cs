// -----------------------------------------------------------------------
//  <copyright file="ShoppingCartMessages.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Messages;

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