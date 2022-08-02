// -----------------------------------------------------------------------
//  <copyright file="InventoryService.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Services;

public class InventoryService: BaseClusterService
{
    private readonly IActorRef _inventoryRegion;
    
    public InventoryService(IHttpContextAccessor httpContextAccessor, ActorRegistry actorRegistry) 
        : base(httpContextAccessor, actorRegistry)
    {
        _inventoryRegion = ActorRegistry.Get<RegistryKey.InventoryRegion>();
    }

    public async Task<HashSet<ProductDetails>> GetAllProductsAsync()
    {
        var getAllProductsTasks = Enum.GetValues<ProductCategory>()
            .Select(category => _inventoryRegion.Ask<HashSet<ProductDetails>>(new Inventory.GetAllProducts(category)));
        var allProducts = await Task.WhenAll(getAllProductsTasks);
        return new HashSet<ProductDetails>(allProducts.SelectMany(products => products));
    }
}