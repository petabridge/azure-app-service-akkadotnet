// -----------------------------------------------------------------------
//  <copyright file="ServiceCollectionExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static void AddApplicationInsights(
        this IServiceCollection services, string applicationName)
    {
        services.AddApplicationInsightsTelemetry();
        services.AddSingleton<ITelemetryInitializer>(
            _ => new ApplicationMapNodeNameInitializer(applicationName));
    }
}
