// -----------------------------------------------------------------------
//  <copyright file="BaseClusterService.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
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