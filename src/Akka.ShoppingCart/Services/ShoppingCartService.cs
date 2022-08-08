// -----------------------------------------------------------------------
//  <copyright file="ShoppingCartService.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Services;

public class ShoppingCartService: BaseClusterService
{
    private readonly IActorRef _cartRegion;
    
    public ShoppingCartService(IHttpContextAccessor httpContextAccessor, ActorRegistry actorRegistry) : base(httpContextAccessor, actorRegistry)
    {
        _cartRegion = actorRegistry.Get<RegistryKey.ShoppingCartRegion>();
    }

    public async Task<HashSet<CartItem>> GetAllItemsAsync()
    {
        var userId = UserId;
        if (userId is null)
            return new HashSet<CartItem>();

        return await _cartRegion.Ask<HashSet<CartItem>>(new Abstraction.Messages.ShoppingCart.GetAllItems(userId));
    }

    public async Task<int> GetCartCountAsync()
    {
        var userId = UserId;
        if (userId is null)
            return 0;
        return await _cartRegion.Ask<int>(new Abstraction.Messages.ShoppingCart.GetTotalItemsInCart(userId));
    }

    public void EmptyCart()
    {
        var userId = UserId;
        if (userId is null)
            return;
        _cartRegion.Tell(new Abstraction.Messages.ShoppingCart.EmptyCart(userId));
    }

    public async Task<bool> AddOrUpdateItemAsync(int quantity, ProductDetails product)
    {
        var userId = UserId;
        if (userId is null)
            return false;

        return await _cartRegion.Ask<bool>(
            new Abstraction.Messages.ShoppingCart.AddOrUpdateItem(userId, quantity, product));
    }

    public void RemoveItem(ProductDetails product)
    {
        var userId = UserId;
        if (userId is null)
            return;
        
        _cartRegion.Tell(new Abstraction.Messages.ShoppingCart.RemoveItem(userId, product));
    }
}