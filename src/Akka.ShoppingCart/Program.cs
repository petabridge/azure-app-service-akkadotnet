// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
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