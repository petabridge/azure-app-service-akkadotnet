// -----------------------------------------------------------------------
//  <copyright file="ApplicationMapNodeNameInitializer.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
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
