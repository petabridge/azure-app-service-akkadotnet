// -----------------------------------------------------------------------
//  <copyright file="ComponentStateChangedObserver.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
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