// -----------------------------------------------------------------------
// <copyright file="BlazorService.cs" company="Petabridge, LLC">
//     Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//     Copyright (c) Microsoft. All rights reserved.
//     Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------

namespace Akka.ShoppingCart.Services
{
    public sealed class BlazorService : IHostedService
    {
        private ActorSystem _clusterSystem;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private IActorRef _requestDistributor;

        private readonly IHostApplicationLifetime _applicationLifetime;

        public BlazorService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _applicationLifetime = appLifetime;
            _configuration = configuration;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
