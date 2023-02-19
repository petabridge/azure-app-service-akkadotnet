// -----------------------------------------------------------------------
//  <copyright file="GlobalUsings.cs" company="Petabridge, LLC">
//      Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//      Copyright (c) Microsoft. All rights reserved.
//      Licensed under the MIT License.
//  </copyright>
// -----------------------------------------------------------------------

global using System.Security.Claims;
global using System.Text;
global using Bogus;
global using Microsoft.ApplicationInsights.Channel;
global using Microsoft.ApplicationInsights.Extensibility;

global using Akka.ShoppingCart.Abstraction;
global using Akka.ShoppingCart.Api.Actors;
global using Akka.ShoppingCart.Api.Extensions;
global using Akka.ShoppingCart.Api.Messages;
global using Akka.ShoppingCart.Api.Services;

global using Akka.Actor;
global using Akka.Cluster.Hosting;
global using Akka.Configuration;
global using Akka.DependencyInjection;
global using Akka.Discovery;
global using Akka.Discovery.Azure;
global using Akka.Hosting;
global using Akka.Management;
global using Akka.Management.Cluster.Bootstrap;
global using Akka.Persistence;
global using Akka.Persistence.Azure.Hosting;
global using Akka.Persistence.Hosting;
global using Akka.Remote.Hosting;
global using Akka.Util;
global using Akka.Util.Internal;
