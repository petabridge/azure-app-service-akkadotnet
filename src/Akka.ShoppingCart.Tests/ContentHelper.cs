using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Akka.ShoppingCart.Tests
{
    public static class ContentHelper
    {
        public static StringContent StringContent(this object obj)
            => new StringContent( JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true}), Encoding.Default, "application/json");
    }
}
