// -----------------------------------------------------------------------
//  <copyright file="SeedProductStoreTask.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.StartupTask;

public sealed class SeedProductStoreTask
{
    private readonly IActorRef _productRegion;

    public SeedProductStoreTask(ActorRegistry registry) =>
        _productRegion = registry.Get<RegistryKey.ProductRegion>();

    public void Execute()
    {
        var faker = new ProductDetails().GetBogusFaker();

        foreach (var product in faker.GenerateLazy(50))
        {
            _productRegion.Tell(new Product.CreateOrUpdate(product));
        }
    }
}
