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
    public class DepartmentTest
    {
        [Fact]
        public async Task GetAllDepartments()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*============================
                 * Arrange
                 ========================*/
                var response = await client.GetAsync("api/Department");

                string responseBody = await response.Content.ReadAsStringAsync();
                var DepartmentList = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(DepartmentList.Count > 0);
            }
        }
        [Fact]
        public async Task GetSpecificDepartment()
        {
            
            using (var client = new APIClientProvider().Client)
            {
                
                var specific_Department = await client.GetAsync("api/Department/1");
                
                string responseBody = await specific_Department.Content.ReadAsStringAsync();
                
                var specific_Dept_res = JsonConvert.DeserializeObject<Department>(responseBody);


                Assert.Equal(HttpStatusCode.OK, specific_Department.StatusCode);
                Assert.Equal("Gardening", specific_Dept_res.Name);
            }
        }
        [Fact]
        public async Task PostNewDepartment()
        {
            using (var client = new APIClientProvider().Client)
            {
                //Creating a new Payment Type in C# later to be serialized to JSON
                Department WaterBottles = new Department
                {
                    Name = "WaterBottle",
                    Budget = 9518234,
                };

                //Serialize the C# object into JSON object 
                var waterBottleAsJson = JsonConvert.SerializeObject(WaterBottles);

                //need to use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/Department",
                    new StringContent(waterBottleAsJson, Encoding.UTF8, "application/json")
                );
                //If post is not successful the code will crash around line 87

                //preforming the GET after posting to the DB. Will only execute if the inital posting is successful. 
                string responseBody = await response.Content.ReadAsStringAsync();

                var newDepartmentInDB = JsonConvert.DeserializeObject<Department>(responseBody);

                //Asserting that the created Staus code 201 is obtained
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                //Asserting that if the new PaymentType's name is MasterCard then the post was successful.
                Assert.Equal("WaterBottle", newDepartmentInDB.Name);
            }
        }
        [Fact]
        public async Task PutDepartment()
        {
            using (var client = new APIClientProvider().Client)
            {
                //need to get to api/ProductType to get all available product types.
                var departmentResponse = await client.GetAsync("api/Department");

                string responseBody = await departmentResponse.Content.ReadAsStringAsync();

                //Convert the response into something that C# can understand
                var departmentList = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, departmentResponse.StatusCode);
                Assert.True(departmentList.Count > 0);

                //Pick an object from the list of the ProductTypes and assign the name of that object to a variable to use for later
                var departmentObject = departmentList[0];
                var defaultDepartmentName = departmentObject.Name;

                //Modify the selected object
                departmentObject.Name = "Wax";

                //Serialize modified object and perform a PUT
                var departmentJson = JsonConvert.SerializeObject(departmentObject);

                //Utilize the client to perform a PUT
                var response = await client.PutAsync(
                        $"/api/Department/{departmentObject.id}",
                        new StringContent(departmentJson, Encoding.UTF8, "application/json")
                    );
                string newModifiedObjBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                //Get by Id and verify that the Name was indeed changed.

                var specificDepartment= await client.GetAsync($"api/Department/{departmentObject.id}");
                //wait until specific_PT is complete then read that response as a string
                string modDepartmentResponseBody = await specificDepartment.Content.ReadAsStringAsync();
                //Then we convert the json into something that C# can read and create a list of all of the payment types
                var departmentRes = JsonConvert.DeserializeObject<Department>(modDepartmentResponseBody);

                Assert.Equal(HttpStatusCode.OK, specificDepartment.StatusCode);
                Assert.Equal("Wax", departmentRes.Name);

                //set obj back to original value by doing another put
                departmentRes.Name = defaultDepartmentName;
                var originalProductTypeJson = JsonConvert.SerializeObject(departmentObject);

                //need to use the client to send the request and store the response
                var originalResponse = await client.PutAsync(
                    $"/api/Department/{departmentObject.id}",
                    new StringContent(departmentObject, Encoding.UTF8, "application/json")
                );
                string originalProductTypeObject = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
    }
}
