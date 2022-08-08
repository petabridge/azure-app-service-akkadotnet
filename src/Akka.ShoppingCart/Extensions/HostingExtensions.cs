// -----------------------------------------------------------------------
//  <copyright file="HostingExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Extensions;

public static class HostingExtensions
{
    public static AkkaConfigurationBuilder AddShoppingCartRegions(this AkkaConfigurationBuilder builder)
    {
        return builder
            .WithShardRegion<RegistryKey.ProductRegion>(
                typeName: "product",
                entityPropsFactory: ProductActor.Props,
                extractEntityId: Product.ExtractEntityId,
                extractShardId: Product.ExtractShardId,
                shardOptions: new ShardOptions())
            .WithShardRegion<RegistryKey.InventoryRegion>(
                typeName: "inventory",
                entityPropsFactory: InventoryActor.Props,
                extractEntityId: Inventory.ExtractEntityId,
                extractShardId: Inventory.ExtractShardId,
                shardOptions: new ShardOptions())
            .WithShardRegion<RegistryKey.ShoppingCartRegion>(
                typeName: "shoppingCart",
                entityPropsFactory: ShoppingCartActor.Props,
                extractEntityId: Abstraction.Messages.ShoppingCart.ExtractEntityId,
                extractShardId: Abstraction.Messages.ShoppingCart.ExtractShardId,
                shardOptions: new ShardOptions());
    }
}