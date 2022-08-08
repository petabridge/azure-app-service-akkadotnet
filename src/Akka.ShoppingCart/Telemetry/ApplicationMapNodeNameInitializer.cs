// -----------------------------------------------------------------------
//  <copyright file="ApplicationMapNodeNameInitializer.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Telemetry;

internal class ApplicationMapNodeNameInitializer : ITelemetryInitializer
{
    private readonly string _name;

    internal ApplicationMapNodeNameInitializer(string name) => _name = name;

    public void Initialize(ITelemetry telemetry) =>
        telemetry.Context.Cloud.RoleName = _name;
}
