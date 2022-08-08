// -----------------------------------------------------------------------
//  <copyright file="ComponentStateChangedObserver.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Services;

public sealed class ComponentStateChangedObserver
{
    public event Func<Task>? OnStateChanged;

    public Task NotifyStateChangedAsync() =>
        OnStateChanged?.Invoke() ?? Task.CompletedTask;
}