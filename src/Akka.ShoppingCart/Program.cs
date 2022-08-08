// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

using Akka.ShoppingCart;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(config =>
    {
        config.UseStartup(context => new Startup(context));
    })
    .RunConsoleAsync();