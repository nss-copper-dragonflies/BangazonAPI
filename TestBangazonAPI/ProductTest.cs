//author: Allison Collins. Integration tests for GET all, GET single, POST, PUT, and DELETE products.


using Newtonsoft.Json;
using BangazonAPI.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TestBangazonAPI.Test;

namespace TestBangazonAPI
{
    public class ProductTest
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
        public async Task Test_Get_Single_Product()
        {
            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("api/product/3");
                string responseBody = await response.Content.ReadAsStringAsync();

                var specificProduct = JsonConvert.DeserializeObject<Product>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(specificProduct.Title == "Chia Pet");
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
                var response = await client.PostAsync(
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
                Assert.Equal("Electric Slingshot", newSlingshot.Title);
                Assert.Equal("Defeat the giants in your life without breaking a sweat", newSlingshot.Description);
                Assert.Equal(50, newSlingshot.Quantity);
            }
        }

        //PUT test - update product
        [Fact]
        public async Task Test_Modify_Product()
        {
            // New title to change to and test
            int newQuantity = 20;

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                */
                Product modifiedJeans = new Product
                {
                    Id = 5,
                    ProductTypeId = 3,
                    CustomerId = 5,
                    Price = 150,
                    Title = "Apple Bottom Jeans",
                    Description = "Pair with fur boots",
                    Quantity = newQuantity
                };
                var modifiedJeansAsJSON = JsonConvert.SerializeObject(modifiedJeans);

                var response = await client.PutAsync(
                    "/api/product/5",
                    new StringContent(modifiedJeansAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                    Verify that the PUT operation was successful
                */

                var getJeans = await client.GetAsync("/api/product/5");
                getJeans.EnsureSuccessStatusCode();

                string getJeansBody = await getJeans.Content.ReadAsStringAsync();
                Product newJeans = JsonConvert.DeserializeObject<Product>(getJeansBody);

                Assert.Equal(HttpStatusCode.OK, getJeans.StatusCode);
                Assert.Equal(20, newJeans.Quantity);
            }
        }

        //DELETE test
        [Fact]
        public async Task Test_Delete_Product()
        {
            using (var client = new APIClientProvider().Client)
            {

                var response = await client.DeleteAsync("/api/Product/6");


                string responseBody = await response.Content.ReadAsStringAsync();
                var Product = JsonConvert.DeserializeObject<Product>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
