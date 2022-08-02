// -----------------------------------------------------------------------
//  <copyright file="SeedProductStoreTask.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
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
