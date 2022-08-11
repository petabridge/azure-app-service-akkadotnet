# Akka.NET: Shopping Cart App

A canonical shopping cart sample application, built using Akka.NET. This app shows the following features:

- **Shopping cart**: A simple shopping cart application that uses Akka.NET for its cross-platform framework support, and its scalable distributed applications capabilities.

    - **Inventory management**: Edit and/or create product inventory.
    - **Shop inventory**: Explore purchasable products and add them to your cart.
    - **Cart**: View a summary of all the items in your cart, and manage these items; either removing or changing the quantity of each item.

![Shopping Cart sample app running.](media/shopping-cart.png)

## Features

- [.NET 6](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-6)
- [ASP.NET Core Blazor](https://docs.microsoft.com/aspnet/core/blazor/?view=aspnetcore-6.0)
- [Akka.NET: Akka.Persistence](https://getakka.net/articles/persistence/architecture.html)
    - [Azure Storage persistence](https://github.com/petabridge/Akka.Persistence.Azure/)
- [Akka.NET: Akka.Management](https://github.com/akkadotnet/Akka.Management/)
    - [Akka.Management.Cluster.Bootstrap](https://github.com/akkadotnet/Akka.Management/tree/dev/src/cluster.bootstrap/Akka.Management.Cluster.Bootstrap)
    - [Akka.Discovery.Azure](https://github.com/akkadotnet/Akka.Management/tree/dev/src/discovery/azure/Akka.Discovery.Azure)
- [Azure Bicep](https://docs.microsoft.com/azure/azure-resource-manager/bicep)
- [Azure App Service](https://docs.microsoft.com/azure/app-service/overview)

The app is architected as follows:

![Shopping Cart sample app architecture.](media/shopping-cart-arch.png)

## Get Started

### Prerequisites

- A [GitHub account](https://github.com/join)
- The [.NET 6 SDK or later](https://dotnet.microsoft.com/download/dotnet)
- The [Azure CLI](/cli/azure/install-azure-cli)
- A .NET integrated development environment (IDE)
    - Feel free to use the [Visual Studio IDE](https://visualstudio.microsoft.com) or the [Visual Studio Code](https://code.visualstudio.com)

### Quickstart

1. `git clone git@github.com:petabridge/azure-app-service-akkadotnet.git akka-on-app-service`
2. `cd akka-on-app-service`
3. `dotnet run --project src\Akka.ShoppingCart\Akka.ShoppingCart.csproj`

### Acknowledgements

The Akka.ShoppingCart project uses the following 3rd party open-source projects:

- [MudBlazor](https://github.com/MudBlazor/MudBlazor): Blazor Component Library based on Material design.
- [Bogus](https://github.com/bchavez/Bogus): A simple fake data generator for C#, F#, and VB.NET.
- [Blazorators](https://github.com/IEvangelist/blazorators): Source-generated packages for Blazor JavaScript interop.

Derived from: 
- [Azure-Samples/Orleans-Cluster-on-Azure-App-Service](https://github.com/Azure-Samples/Orleans-Cluster-on-Azure-App-Service).
- [IEvangelist/orleans-shopping-cart](https://github.com/IEvangelist/orleans-shopping-cart).
