// -----------------------------------------------------------------------
//  <copyright file="ToastService.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Hosting;

namespace Akka.ShoppingCart.Services;

public class ToastService
{
    public event Func<(string Title, string Message), Task>? OnToastedRequested;
    
    public async Task ShowToastAsync(string title, string message)
    {
        if (OnToastedRequested is not null)
        {
            await OnToastedRequested.Invoke((title, message));
        }
    }
}