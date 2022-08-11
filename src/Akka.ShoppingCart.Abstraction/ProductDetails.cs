// -----------------------------------------------------------------------
//  <copyright file="ProductDetails.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Abstraction;

public sealed record ProductDetails
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ProductCategory Category { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string DetailsUrl { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;

    [JsonIgnore]
    public decimal TotalPrice =>
        Math.Round(Quantity * UnitPrice, 2);
}
