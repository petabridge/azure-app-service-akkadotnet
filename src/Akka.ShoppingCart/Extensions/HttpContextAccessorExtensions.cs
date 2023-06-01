// -----------------------------------------------------------------------
//  <copyright file="HttpContextAccessorExtensions.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Extensions;

internal static class HttpContextAccessorExtensions
{
    internal static string? TryGetUserId(
        this IHttpContextAccessor? httpContextAccessor) =>
        httpContextAccessor
            ?.HttpContext
            ?.User
            .FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "demo-shared-user";
}
