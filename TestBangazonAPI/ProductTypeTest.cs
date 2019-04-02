using BangazonAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestBangazonAPI.Test;
using Xunit;

namespace TestBangazonAPI
{
    public class ProductTypeTest
    {
        //GET
        [Fact]
        public async Task Get_All_Product_Type()
        {
            //Using APIClientProvider.Client we can carry out the rest of our test 
            using (var client = new APIClientProvider().Client)
            {
                //going to api/payment types
                var productTypeResponse = await client.GetAsync("api/ProductType");
                //Line 25 will wait until paymentTypeResponse is complete then read that response as a string
                string responseBody = await productTypeResponse.Content.ReadAsStringAsync();
                //Then we convert the json into something that C# can read and create a list of all of the payment types
                var productTypeList = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);

                //Asserting/promising that that the status code should be 200 
                Assert.Equal(HttpStatusCode.OK, productTypeResponse.StatusCode);
                //Asserting that the list will have more items than 0 in it.
                Assert.True(productTypeList.Count > 0);
            }
        }
        //GET by ID
        [Fact]
        public async Task Get_Specific_Product_Type()
        {
            //Usiing APIClientProvider.Client we can carry out the rest of our test 
            using (var client = new APIClientProvider().Client)
            {
                //going to api/ProductType/1 - targeting a specific ProductType Obj in the DB
                var specificProductType = await client.GetAsync("api/ProductType/3");
                //wait until specific_PT is complete then read that response as a string
                string responseBody = await specificProductType.Content.ReadAsStringAsync();
                //Then we convert the json into something that C# can read and create a list of all of the product types
                var specific_ProductType_res = JsonConvert.DeserializeObject<ProductType>(responseBody);


                Assert.Equal(HttpStatusCode.OK, specificProductType.StatusCode);
                Assert.Equal("Clothing", specific_ProductType_res.Name);
            }
        }
        //POST
        [Fact]
        public async Task Post_New_Product_Type()
        {
            using (var client = new APIClientProvider().Client)
            {
                //Creating a new Payment Type in C# later to be serialized to JSON
                ProductType shoes = new ProductType
                {
                    Name = "Nike"
                };

                //Serialize the C# object into JSON object 
                var shoesAsJson = JsonConvert.SerializeObject(shoes);

                //need to use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/ProductType",
                    new StringContent(shoesAsJson, Encoding.UTF8, "application/json")
                );
                //If post is not successful the code will crash around line 87

                //preforming the GET after posting to the DB. Will only execute if the inital posting is successful. 
                string responseBody = await response.Content.ReadAsStringAsync();

                var newProductType = JsonConvert.DeserializeObject<ProductType>(responseBody);

                //Asserting that the created Staus code 201 is obtained
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                //Asserting that if the new PaymentType's name is MasterCard then the post was successful.
                Assert.Equal("Nike", newProductType.Name);
            }
        }
        [Fact]
        public async Task Update_Existing_Product_Type()
        {
            using (var client = new APIClientProvider().Client)
            {
                //==================================GET a list of all the payment types ===================================//
                //going to api/payment types
                var productTypeResponse = await client.GetAsync("api/ProdcutType");

                string responseBody = await productTypeResponse.Content.ReadAsStringAsync();
                //Then we convert the json into something that C# can read and create a list of all of the payment types
                var productTypeList = JsonConvert.DeserializeObject<List<ProductType>>(responseBody);
                //Asserting/promising that that the status code should be 200 
                Assert.Equal(HttpStatusCode.OK, productTypeResponse.StatusCode);
                //Asserting that the list will have more items than 0 in it.

                //selecting the first PaymentType object in the list using [0]
                var productTypeObj = productTypeList[0];

                //assigned default value of the account number for later
                var defaultName = productTypeObj.Name;

                //modify the Acctnumber of the selected object
                productTypeObj.Name = "Fishing Rod";

                //Serialize the C# object into JSON object 
                var productTypeJson = JsonConvert.SerializeObject(productTypeObj);

                //need to use the client to send the request and store the response
                var response = await client.PutAsync(
                    $"/api/ProductType/{productTypeObj.id}",
                    new StringContent(productTypeJson, Encoding.UTF8, "application/json")
                );
                string newModifiedObjBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                //going to api/PaymentTypes/1 - targeting a specific PaymentType Obj in the DB
                var specific_PT = await client.GetAsync($"api/ProductType/{productTypeObj.id}");
                //wait until specific_PT is complete then read that response as a string
                string modPaymentTypeRespinseBody = await specific_PT.Content.ReadAsStringAsync();
                //Then we convert the json into something that C# can read and create a list of all of the payment types
                var specific_ProductType_res = JsonConvert.DeserializeObject<PaymentType>(modPaymentTypeRespinseBody);

                Assert.Equal(HttpStatusCode.OK, specific_PT.StatusCode);
                Assert.Equal("Fishing Rod", specific_ProductType_res.Name);

                //set obj back to original value by doing another put
                specific_ProductType_res.Name = defaultName;
                var originalProductTypeJson = JsonConvert.SerializeObject(productTypeObj);

                //need to use the client to send the request and store the response
                var originalResponse = await client.PutAsync(
                    $"/api/ProductType/{productTypeObj.id}",
                    new StringContent(originalProductTypeJson, Encoding.UTF8, "application/json")
                );
                string originalProductTypeObject = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
    }
}
