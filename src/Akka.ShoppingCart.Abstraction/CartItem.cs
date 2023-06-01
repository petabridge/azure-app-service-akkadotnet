// -----------------------------------------------------------------------
//  <copyright file="CartItem.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2023 Petabridge, LLC <https://petabridge.com>
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