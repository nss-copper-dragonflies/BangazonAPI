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

//Author: Brittany Ramos-Janeway
//Function: Performs the tests for the Get list, Get individual, Post, and Put methods in the customer resource controller

namespace TestBangazonAPI
{
    public class CustomerTest
    {

        //Get all customers test
        [Fact]
        public async Task Test_Get_All_Customers()
        {
            using (var client = new APIClientProvider().Client)
            {
                
                var response = await client.GetAsync("/api/customer");

                string responseBody = await response.Content.ReadAsStringAsync();
                var customerList = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customerList.Count > 0);
            }
        }

        //Get single customer test
        [Fact]
        public async Task Test_Get_Specific_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/api/customer/1");
                string responseBody = await response.Content.ReadAsStringAsync();
                var Customer = JsonConvert.DeserializeObject<Customer>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(Customer.Id == 1);
            }
        }

        //Put method test
        [Fact]
        public async Task Test_Modify_Customer()
        {
            string newFirstName = "Not Allison";

            using (var client = new APIClientProvider().Client)
            {
                Customer modifiedAaron = new Customer
                {
                    FirstName = newFirstName,
                    LastName = "Carter"
                };
                var modifiedAaronAsJSON = JsonConvert.SerializeObject(modifiedAaron);

                var response = await client.PutAsync(
                    "/api/customer/1",
                    new StringContent(modifiedAaronAsJSON, Encoding.UTF8, "application/json"));

                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                var getAaron = await client.GetAsync("/api/customer/1");
                getAaron.EnsureSuccessStatusCode();

                string getAaronBody = await getAaron.Content.ReadAsStringAsync();
                Customer newAaron = JsonConvert.DeserializeObject<Customer>(getAaronBody);

                Assert.Equal(HttpStatusCode.OK, getAaron.StatusCode);
                Assert.Equal(newFirstName, newAaron.FirstName);

            }
        }

        //Post method test
        [Fact]
        public async Task Test_Create_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {
                Customer dizzee = new Customer
                {
                    FirstName = "Dizzee",
                    LastName = "Rascal"
                };

                var dizzeeAsJSON = JsonConvert.SerializeObject(dizzee);

                var response = await client.PostAsync(
                    "/api/customer",
                    new StringContent(dizzeeAsJSON, Encoding.UTF8, "application/json"));

                string responseBody = await response.Content.ReadAsStringAsync();

                var newDizzee = JsonConvert.DeserializeObject<Customer>(responseBody);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Dizzee", newDizzee.FirstName);
                Assert.Equal("Rascal", newDizzee.LastName);
            }
        }
    }
}
