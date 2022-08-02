// -----------------------------------------------------------------------
//  <copyright file="BaseClusterService.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Pages;

public sealed partial class Cart
{
    private HashSet<CartItem>? _cartItems;

    [Inject]
    public ShoppingCartService ShoppingCart { get; set; } = null!;

    [Inject]
    public ComponentStateChangedObserver Observer { get; set; } = null!;

    protected override Task OnInitializedAsync() => GetCartItemsAsync();

    private Task GetCartItemsAsync() =>
        InvokeAsync(async () =>
        {
            _cartItems = await ShoppingCart.GetAllItemsAsync();
            StateHasChanged();
        });

    private async Task OnItemRemovedAsync(ProductDetails product)
    {
        ShoppingCart.RemoveItem(product);
        await Observer.NotifyStateChangedAsync();

        _ = _cartItems?.RemoveWhere(item => item.Product == product);
    }

    private async Task OnItemUpdatedAsync((int Quantity, ProductDetails Product) tuple)
    {
        await ShoppingCart.AddOrUpdateItemAsync(tuple.Quantity, tuple.Product);
        await GetCartItemsAsync();
    }

    private async Task EmptyCartAsync()
    {
        ShoppingCart.EmptyCart();
        await Observer.NotifyStateChangedAsync();

        _cartItems?.Clear();
    }
}
