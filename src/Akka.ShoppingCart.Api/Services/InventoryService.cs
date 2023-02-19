﻿// -----------------------------------------------------------------------
//  <copyright file="InventoryService.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Api.Services;

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