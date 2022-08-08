// -----------------------------------------------------------------------
//  <copyright file="CartItem.cs" company="Akka.NET Project">
//      Copyright (C) 2013-2022 .NET Foundation <https://github.com/akkadotnet/akka.net>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Abstraction;

public sealed record CartItem(
    string UserId,
    int Quantity,
    ProductDetails Product)
{
    [JsonIgnore]
    public decimal TotalPrice =>
        Math.Round(Quantity * Product.UnitPrice, 2);
}