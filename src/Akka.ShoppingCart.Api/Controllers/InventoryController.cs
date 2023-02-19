using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Akka.ShoppingCart.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IActorRef _inventoryRegion;
        protected readonly ActorRegistry ActorRegistry;

        public InventoryController(ActorRegistry actorRegistry)
        {
            _inventoryRegion = actorRegistry.Get<RegistryKey.InventoryRegion>();
        }
        [HttpGet]
        public async ValueTask<JsonResult> GetAllProductsAsync()
        {
            var getAllProductsTasks = Enum.GetValues<ProductCategory>()
                .Select(category => _inventoryRegion.Ask<HashSet<ProductDetails>>(new Inventory.GetAllProducts(category)));
            var allProducts = await Task.WhenAll(getAllProductsTasks);
            var products = new HashSet<ProductDetails>(allProducts.SelectMany(products => products));
            return Result(products);
        }
        private JsonResult Result(object rt)
        {
            return new JsonResult(rt) { StatusCode = (int)HttpStatusCode.OK };
        }
    }
}
