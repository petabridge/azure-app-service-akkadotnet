// -----------------------------------------------------------------------
//  <copyright file="ShoppingCartService.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
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

        return await _cartRegion.Ask<HashSet<CartItem>>(new Messages.ShoppingCart.GetAllItems(userId));
    }

    public async Task<int> GetCartCountAsync()
    {
        var userId = UserId;
        if (userId is null)
            return 0;
        return await _cartRegion.Ask<int>(new Messages.ShoppingCart.GetTotalItemsInCart(userId));
    }

    public async Task EmptyCartAsync()
    {
        var userId = UserId;
        if (userId is null)
            return;
        await _cartRegion.Ask<Done>(new Messages.ShoppingCart.EmptyCart(userId));
    }

    public async Task<bool> AddOrUpdateItemAsync(int quantity, ProductDetails product)
    {
        var userId = UserId;
        if (userId is null)
            return false;

        return await _cartRegion.Ask<bool>(
            new Messages.ShoppingCart.AddOrUpdateItem(userId, quantity, product));
    }

    public async Task RemoveItemAsync(ProductDetails product)
    {
        var userId = UserId;
        if (userId is null)
            return;
        
        await _cartRegion.Ask<Done>(new Messages.ShoppingCart.RemoveItem(userId, product));
    }
}