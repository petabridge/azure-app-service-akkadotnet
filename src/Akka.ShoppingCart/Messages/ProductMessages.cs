// -----------------------------------------------------------------------
//  <copyright file="ProductMessages.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Messages;

public static class Product
{
    public static Option<(string, object)> ExtractEntityId(object message)
        => (((ProductBase)message).ProductId, message);

    public static string ExtractShardId(object message)
        => ((ProductBase)message).ProductId;

    public abstract class ProductBase
    {
        protected ProductBase(string productId)
        {
            ProductId = productId;
        }

        public string ProductId { get; }
    }
    
    public sealed class TryTake: ProductBase
    {
        public TryTake(string productId, int quantity) : base(productId)
        {
            Quantity = quantity;
        }

        public int Quantity { get; }
    }
    
    public sealed class TakeResult
    {
        public TakeResult(bool isAvailable, ProductDetails? productDetails = null)
        {
            IsAvailable = isAvailable;
            ProductDetails = productDetails;
        }

        public bool IsAvailable { get; }
        public ProductDetails? ProductDetails { get; }

        public void Deconstruct(out bool isAvailable, out ProductDetails? productDetails)
        {
            isAvailable = IsAvailable;
            productDetails = ProductDetails;
        }
    }

    public sealed class Return: ProductBase
    {
        public Return(string productId, int quantity) : base(productId)
        {
            Quantity = quantity;
        }

        public int Quantity { get; }
    }
    
    public sealed class GetAvailability: ProductBase
    {
        public GetAvailability(string productId) : base(productId)
        {
        }
    }
    
    public sealed class CreateOrUpdate: ProductBase
    {
        public CreateOrUpdate(ProductDetails productDetails) : base(productDetails.Id)
        {
            ProductDetails = productDetails;
        }

        public ProductDetails ProductDetails { get; }
    }
    
    public sealed class GetDetails: ProductBase
    {
        public GetDetails(string productId) : base(productId)
        {
        }
    }
}