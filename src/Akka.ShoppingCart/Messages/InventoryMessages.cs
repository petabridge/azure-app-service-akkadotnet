// -----------------------------------------------------------------------
//  <copyright file="BaseClusterService.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Messages;

public static class Inventory
{
    public static Option<(string, object)> ExtractEntityId(object message)
        => (((InventoryBase)message).CategoryId.ToString(), message);

    public static string ExtractShardId(object message)
        => ((InventoryBase)message).CategoryId.ToString();

    public abstract class InventoryBase
    {
        protected InventoryBase(ProductCategory categoryId)
        {
            CategoryId = categoryId;
        }

        public ProductCategory CategoryId { get; }
    }
    
    public sealed class GetAllProducts: InventoryBase
    {
        public GetAllProducts(ProductCategory categoryId) : base(categoryId)
        {
        }
    }
    
    public sealed class AddOrUpdateProduct: InventoryBase
    {
        public AddOrUpdateProduct(ProductCategory categoryId, ProductDetails productDetails) : base(categoryId)
        {
            ProductDetails = productDetails;
        }

        public ProductDetails ProductDetails { get; }
    }

    public sealed class RemoveProduct: InventoryBase
    {
        public RemoveProduct(ProductCategory categoryId, string productId) : base(categoryId)
        {
            ProductId = productId;
        }

        public string ProductId { get; }
    }
}