// -----------------------------------------------------------------------
//  <copyright file="ProductService.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Services;

public class ProductService: BaseClusterService
{
    private readonly IActorRef _productRegion;
    
    public ProductService(IHttpContextAccessor httpContextAccessor, ActorRegistry actorRegistry) 
        : base(httpContextAccessor, actorRegistry)
    {
        _productRegion = actorRegistry.Get<RegistryKey.ProductRegion>();
    }

    public async Task CreateOrUpdateProductAsync(ProductDetails product)
        => await _productRegion.Ask<Done>(new Product.CreateOrUpdate(product));

    public async Task<(bool, ProductDetails?)> TryTakeProductAsync(string productId, int quantity)
    {
        var userId = UserId;
        if (userId is null)
            return(false, null);

        var (isAvailable, details) = await _productRegion.Ask<Product.TakeResult>(new Product.TryTake(productId, quantity));
        return (isAvailable, details);
    }

    public async Task ReturnProductAsync(string productId, int quantity)
        => await _productRegion.Ask<Done>(new Product.Return(productId, quantity));

    public async Task<int> GetProductAvailability(string productId)
        => await _productRegion.Ask<int>(new Product.GetAvailability(productId));
}