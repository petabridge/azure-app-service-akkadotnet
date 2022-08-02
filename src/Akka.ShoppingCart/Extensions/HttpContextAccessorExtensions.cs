// -----------------------------------------------------------------------
//  <copyright file="HttpContextAccessorExtensions.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
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
