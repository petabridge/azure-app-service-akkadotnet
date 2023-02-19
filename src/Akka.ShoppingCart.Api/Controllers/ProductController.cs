using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Akka.ShoppingCart.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IActorRef _productRegion;
        protected readonly ActorRegistry ActorRegistry;

        public ProductController(ActorRegistry actorRegistry)
        {
            _productRegion = actorRegistry.Get<RegistryKey.ProductRegion>();
        }
        [HttpGet]
        [Route("/id")]
        public async ValueTask<JsonResult> TryTakeProductAsync(string productId, int quantity)
        {
            var (isAvailable, details) = await _productRegion.Ask<Product.TakeResult>(new Product.TryTake(productId, quantity));

            return Result((isAvailable, details));
        }
        private JsonResult Result(object rt)
        {
            return new JsonResult(rt) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}
