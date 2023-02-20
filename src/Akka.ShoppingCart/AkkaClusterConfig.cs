// -----------------------------------------------------------------------
// <copyright file="AkkaClusterConfig.cs" company="Petabridge, LLC">
//     Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//     Copyright (c) Microsoft. All rights reserved.
//     Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart
{
    public class AkkaClusterConfig
    {
        public string ActorSystemName { get; set; } = "ActorSystem";
        public string? Hostname { get; set; }
        public int? Port { get; set; }

        public string[]? Roles { get; set; } = Array.Empty<string>();

        public string[]? SeedNodes { get; set; } = Array.Empty<string>();
    }
}
