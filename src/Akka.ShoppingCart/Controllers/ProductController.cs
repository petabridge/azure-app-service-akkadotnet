// -----------------------------------------------------------------------
// <copyright file="ProductController.cs" company="Petabridge, LLC">
//     Copyright (C) 2015-2022 Petabridge, LLC <https://petabridge.com>
//     Copyright (c) Microsoft. All rights reserved.
//     Licensed under the MIT License.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using System.Net;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Akka.ShoppingCart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IActorRef _productRegion;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly ActorRegistry ActorRegistry;

        protected string? UserId => _httpContextAccessor.TryGetUserId();
        public ProductController(IHttpContextAccessor httpContextAccessor, ActorRegistry actorRegistry)
        {
            _httpContextAccessor = httpContextAccessor; 
            _productRegion = actorRegistry.Get<RegistryKey.ProductRegion>();
        }
        [HttpGet]
        [Route("/id")]
        public async ValueTask<JsonResult> TryTakeProductAsync(string productId, int quantity)
        {
            var userId = UserId;
            // (userId is null)
             //   return Result((false, null)); 

            var (isAvailable, details) = await _productRegion.Ask<Product.TakeResult>(new Product.TryTake(productId, quantity));
            
            return Result((isAvailable, details));
        }
        private JsonResult Result(object rt)
        {           
            return new JsonResult(rt) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}
