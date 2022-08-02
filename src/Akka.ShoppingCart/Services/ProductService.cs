// -----------------------------------------------------------------------
//  <copyright file="ProductService.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
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

    public void CreateOrUpdateProductAsync(ProductDetails product)
        => _productRegion.Tell(new Product.CreateOrUpdate(product));

    public async Task<(bool, ProductDetails?)> TryTakeProductAsync(string productId, int quantity)
    {
        var userId = UserId;
        if (userId is null)
            return(false, null);

        var (isAvailable, details) = await _productRegion.Ask<Product.TakeResult>(new Product.TryTake(productId, quantity));
        return (isAvailable, details);
    }

    public void ReturnProductAsync(string productId, int quantity)
        => _productRegion.Tell(new Product.Return(productId, quantity));

    public async Task<int> GetProductAvailability(string productId)
        => await _productRegion.Ask<int>(new Product.GetAvailability(productId));
}