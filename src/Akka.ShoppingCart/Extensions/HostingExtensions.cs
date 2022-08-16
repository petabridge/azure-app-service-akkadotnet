// -----------------------------------------------------------------------
//  <copyright file="HostingExtensions.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
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
                extractEntityId: Messages.ShoppingCart.ExtractEntityId,
                extractShardId: Messages.ShoppingCart.ExtractShardId,
                shardOptions: new ShardOptions());
    }

    public static AkkaConfigurationBuilder WithConfigDiscovery(
        this AkkaConfigurationBuilder builder,
        Dictionary<string, List<string>> services)
    {
        var sb = new StringBuilder();
        foreach (var service in services)
        {
            sb.AppendLine($@"
{service.Key} {{
    endpoints = [ {string.Join(", ", service.Value.Select(s => $"\"{s}\""))} ]
}}
");
        }
        var config = ConfigurationFactory.ParseString($@"
akka.discovery{{
    method = config
    config {{
        services {{
            {sb}
        }}
    }}
}}").WithFallback(DiscoveryProvider.DefaultConfiguration());
        
        builder.AddHocon(config, HoconAddMode.Prepend);
        return builder;
    }
}