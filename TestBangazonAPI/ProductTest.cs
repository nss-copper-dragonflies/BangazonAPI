//author: Allison Collins

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using Xunit;
using BangazonAPI.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Net;

namespace TestBangazonAPI
{
    //GET ALL PRODUCTS test
    //Perform a GetAsync() request to a resource URL, convert the response to C# and write a corresponding assertion
    [Fact]
    public async Task Test_Get_All_Products()
    {
        using (var client = new APIClientProvider().Client)
        {
            /*============================
             * Arrange
             ========================*/
            var response = await client.GetAsync("api/product");

            string responseBody = await response.Content.ReadAsStringAsync();
            var productList = JsonConvert.DeserializeObject<List<Product>>(responseBody);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(productList.Count > 0);
        }
    }

    //GET SINGLE PRODUCT test
    [Fact]
    public async Task Get_Single_Product()
    {
        using (var client = new APIClientProvider().Client)
        {
            var response = await client.GetAsync("api/product/1");
            string responseBody = await response.Content.ReadAsStringAsync();
            var specificProduct = JsonConvert.DeserializeObject<Product>(responseBody);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Chia Pet", specificProduct.Title);
        }
    }

    //POST test - add new product
    [Fact]
    public async Task Test_Create_Product()
    {
        /*
            Generate a new instance of an HttpClient that you can
            use to generate HTTP requests to your API controllers.
            The `using` keyword will automatically dispose of this
            instance of HttpClient once your code is done executing.
        */
        using (var client = new APIClientProvider().Client)
        {
            /*
                ARRANGE
            */

            // Construct a new product object to be sent to the API
            Product slingshot = new Product
            {
                ProductTypeId = 1,
                CustomerId = 2,
                Price = 40,
                Title = "Electric Slingshot",
                Description = "Defeat the giants in your life without breaking a sweat",
                Quantity = 50
            };

            // Serialize the C# object into a JSON string
            var slingshotAsJSON = JsonConvert.SerializeObject(slingshot);


            /*
                ACT
            */

            // Use the client to send the request and store the response
            var response = await client.PostAsync
                "/api/product",
                new StringContent(slingshotAsJSON, Encoding.UTF8, "application/json")
            );

            // Store the JSON body of the response
            string responseBody = await response.Content.ReadAsStringAsync();

            // Deserialize the JSON into an instance of Product
            var newSlingshot = JsonConvert.DeserializeObject<Product>(responseBody);


            /*
                ASSERT
            */

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(1, newSlingshot.ProductTypeId);
            Assert.Equal(2, newSlingshot.CustomerId);
            Assert.Equal(40, newSlingshot.Price);
            Assert.Equal("Electric Slingshot", newSlingshot.Title);
            Assert.Equal("Defeat the giants in your life without breaking a sweat", newSlingshot.Description);
            Assert.Equal(50, newSlingshot.Quantity);
        }
    }

    //PUT test - update product
    [Fact]
    public async Task Test_Modify_Product()
    {
        // New last name to change to and test
        string newTitle = "Plasma Slingshot";

        using (var client = new APIClientProvider().Client)
        {
            /*
                PUT section
            */
            Product modifiedSlingshot = new Product
            {
                ProductTypeId = 1,
                CustomerId = 2,
                Price = 40,
                Title = "Plasma Slingshot",
                Description = "Defeat the giants in your life without breaking a sweat",
                Quantity = 50
            };
            var modifiedSlingshotAsJSON = JsonConvert.SerializeObject(modifiedSlingshot);

            var response = await client.PutAsync(
                "/api/product/1",
                new StringContent(modifiedSlingshotAsJSON, Encoding.UTF8, "application/json")
            );
            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            /*
                GET section
                Verify that the PUT operation was successful
            */

            var getSlingshot = await client.GetAsync("/api/product/1");
            getSlingshot.EnsureSuccessStatusCode();

            string getSlingshotBody = await getSlingshot.Content.ReadAsStringAsync();
            Product newSlingshot = JsonConvert.DeserializeObject<Product>(getSlingshotBody);

            Assert.Equal(HttpStatusCode.OK, getSlingshot.StatusCode);
            Assert.Equal(newTitle, newSlingshot.Title);
        }
    }
}
