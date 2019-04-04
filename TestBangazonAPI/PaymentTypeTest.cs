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

//JORDAN ROSAS : Integration Tests for the payment types of the bangazon project. Testing Get, Put, Post, and Delete functionality

    /*============
     * REF KEY
     ===========*/
     //PT = PaymentType
     //DB = Databse
namespace TestBangazonAPI
{
    public class PaymentTypeTest
    {
        //This test will be doing the get of all payment types in the database

        //[Fact] to tell the Xunit framework that we want to test this block of code. 

        //GET
        [Fact]
        public async Task Get_All_Payment_Types()
        {
            //Usiing APIClientProvider.Client we can carry out the rest of our test 
            using (var client = new APIClientProvider().Client)
            {
                //going to api/payment types
                var paymentTypeResponse = await client.GetAsync("api/PaymentType");
                //Line 25 will wait until paymentTypeResponse is complete then read that response as a string
                string responseBody = await paymentTypeResponse.Content.ReadAsStringAsync();
                //Then we convert the json into something that C# can read and create a list of all of the payment types
                var paymentTypeList = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);

                //Asserting/promising that that the status code should be 200 
                Assert.Equal(HttpStatusCode.OK, paymentTypeResponse.StatusCode);
                //Asserting that the list will have more items than 0 in it.
                Assert.True(paymentTypeList.Count > 0);
            }
        }
        //Asserting to Xunit that we want to test the following method. With this method we are going to be testing a GET of a specific item 

        //GET by ID
        [Fact]
        public async Task Get_Specific_Payment_Type()
        {
            //Usiing APIClientProvider.Client we can carry out the rest of our test 
            using (var client = new APIClientProvider().Client)
            {
                //going to api/PaymentTypes/1 - targeting a specific PaymentType Obj in the DB
                var specific_PT = await client.GetAsync("api/PaymentType/3");
                //wait until specific_PT is complete then read that response as a string
                string responseBody = await specific_PT.Content.ReadAsStringAsync();
                //Then we convert the json into something that C# can read and create a list of all of the payment types
                var specific_PT_res = JsonConvert.DeserializeObject<PaymentType>(responseBody);
                

                Assert.Equal(HttpStatusCode.OK, specific_PT.StatusCode);
                Assert.Equal("Mastercard", specific_PT_res.Name);
            }
        }
        //Testing a Post/Creating a new Payment type 
        //POST
        [Fact]
        public async Task Post_New_Payment_Type()
        {
            using (var client = new APIClientProvider().Client)
            {
                //Creating a new Payment Type in C# later to be serialized to JSON
                PaymentType masterCard = new PaymentType
                {
                    Name = "MasterCard",
                    AcctNumber = 9518234,
                    CustomerId = 1
                };

                //Serialize the C# object into JSON object 
                var masterCardAsJson = JsonConvert.SerializeObject(masterCard);

                //need to use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/PaymentType",
                    new StringContent(masterCardAsJson, Encoding.UTF8, "application/json")
                );
                //If post is not successful the code will crash around line 87

                //preforming the GET after posting to the DB. Will only execute if the inital posting is successful. 
                string responseBody = await response.Content.ReadAsStringAsync();

                var newMasterCardInDB = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                //Asserting that the created Staus code 201 is obtained
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                //Asserting that if the new PaymentType's name is MasterCard then the post was successful.
                Assert.Equal("MasterCard", newMasterCardInDB.Name);
            }
        }
        //PUT
        //============================= MY FAVORITE TEST ===============================//
        [Fact]
        public async Task Update_Existing_Payment_Type()
        {
            using (var client = new APIClientProvider().Client)
            {
                //==================================GET a list of all the payment types ===================================//
                //going to api/payment types
                var paymentTypeResponse = await client.GetAsync("api/PaymentType");
                
                string responseBody = await paymentTypeResponse.Content.ReadAsStringAsync();
                //Then we convert the json into something that C# can read and create a list of all of the payment types
                var paymentTypeList = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);
                //Asserting/promising that that the status code should be 200 
                Assert.Equal(HttpStatusCode.OK, paymentTypeResponse.StatusCode);
                //Asserting that the list will have more items than 0 in it.

                //selecting the first PaymentType object in the list using [0]
                var paymentTypeObj = paymentTypeList[0];

                //assigned default value of the account number for later
                var defaultAcctNumber = paymentTypeObj.AcctNumber;

                //modify the Acctnumber of the selected object
                paymentTypeObj.AcctNumber = 1111;

                //Serialize the C# object into JSON object 
                var paymentTypeJson = JsonConvert.SerializeObject(paymentTypeObj);

                //need to use the client to send the request and store the response
                var response = await client.PutAsync(
                    $"/api/PaymentType/{paymentTypeObj.id}",
                    new StringContent(paymentTypeJson, Encoding.UTF8, "application/json")
                );
                string newModifiedObjBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                
                //going to api/PaymentTypes/1 - targeting a specific PaymentType Obj in the DB
                var specific_PT = await client.GetAsync($"api/PaymentType/{paymentTypeObj.id}");
                //wait until specific_PT is complete then read that response as a string
                string modPaymentTypeRespinseBody = await specific_PT.Content.ReadAsStringAsync();
                //Then we convert the json into something that C# can read and create a list of all of the payment types
                var specific_PT_res = JsonConvert.DeserializeObject<PaymentType>(modPaymentTypeRespinseBody);

                Assert.Equal(HttpStatusCode.OK, specific_PT.StatusCode);
                Assert.Equal(1111, specific_PT_res.AcctNumber);

                //set obj back to original value by doing another put
                specific_PT_res.AcctNumber = defaultAcctNumber;
                var originalPaymentTypeJson = JsonConvert.SerializeObject(paymentTypeObj);

                //need to use the client to send the request and store the response
                var originalResponse = await client.PutAsync(
                    $"/api/PaymentType/{paymentTypeObj.id}",
                    new StringContent(paymentTypeJson, Encoding.UTF8, "application/json")
                );
                string originalPaymentTypeObject = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
        [Fact]
        public async Task Forbidden_Delete_payment_type()
        {
            using (var client = new APIClientProvider().Client)
            {

                var response = await client.DeleteAsync("/api/PaymentType/6");


                string responseBody = await response.Content.ReadAsStringAsync();
                var PaymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            }
        }
        [Fact]
        public async Task Can_Be_Deleted()
        {
            using (var client = new APIClientProvider().Client)
            {
                //Creating a new Payment Type in C# later to be serialized to JSON
                PaymentType CardToDelete = new PaymentType
                {
                    Name = "AndyHasChallenges",
                    AcctNumber = 9518234,
                    CustomerId = 1
                };

                //Serialize the C# object into JSON object 
                var CardToDeleteAsJson = JsonConvert.SerializeObject(CardToDelete);

                //need to use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/PaymentType",
                    new StringContent(CardToDeleteAsJson, Encoding.UTF8, "application/json")
                );
                
                var deleteResponse = await client.DeleteAsync($"/api/PaymentType/31");


                string responseBody = await response.Content.ReadAsStringAsync();
                var PaymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            }
        }
    }
}
