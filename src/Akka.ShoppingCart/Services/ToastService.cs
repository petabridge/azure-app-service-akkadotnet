// -----------------------------------------------------------------------
//  <copyright file="ToastService.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
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