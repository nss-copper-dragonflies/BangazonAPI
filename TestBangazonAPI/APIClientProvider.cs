using Microsoft.AspNetCore.Mvc.Testing;
<<<<<<< HEAD
using System.Net.Http;
using Xunit;
using BangazonAPI;

namespace TestBangazonAPI
=======
using BangazonAPI;
using System.Net.Http;
using Xunit;

namespace TestBangazonAPI.Test
>>>>>>> master
{
    class APIClientProvider : IClassFixture<WebApplicationFactory<Startup>>
    {
        public HttpClient Client { get; private set; }
        private readonly WebApplicationFactory<Startup> _factory = new WebApplicationFactory<Startup>();

        public APIClientProvider()
        {
            Client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _factory?.Dispose();
            Client?.Dispose();
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> master
