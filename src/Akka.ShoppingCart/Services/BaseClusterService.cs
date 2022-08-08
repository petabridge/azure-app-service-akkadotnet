// -----------------------------------------------------------------------
//  <copyright file="BaseClusterService.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Services;

public class BaseClusterService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ActorRegistry ActorRegistry;

    protected string? UserId => _httpContextAccessor.TryGetUserId();
    
    public BaseClusterService(IHttpContextAccessor httpContextAccessor, ActorRegistry actorRegistry)
    {
        _httpContextAccessor = httpContextAccessor;
        ActorRegistry = actorRegistry;
    }
    
}