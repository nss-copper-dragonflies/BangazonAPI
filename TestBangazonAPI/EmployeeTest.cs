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
    public class EmployeeTest
    {
        //Get all employees test
        [Fact]
        public async Task Test_Get_All_Employees()
        {
            using (var client = new APIClientProvider().Client)
            {

                var response = await client.GetAsync("/api/employee");

                string responseBody = await response.Content.ReadAsStringAsync();
                var employeeList = JsonConvert.DeserializeObject<List<Employee>>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(employeeList.Count > 0);
            }
        }

        //Get single employee test
        [Fact]
        public async Task Test_Get_Specific_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/api/employee/1");
                string responseBody = await response.Content.ReadAsStringAsync();
                var employee = JsonConvert.DeserializeObject<Employee>(responseBody);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(employee.Id == 1);
            }
        }

        //Put employee test
        [Fact]
        public async Task Test_Modify_Employee()
        {
            string newFirstName = "Real Allison";

            using (var client = new APIClientProvider().Client)
            {
                Employee modifiedAllison = new Employee
                {
                    FirstName = newFirstName,
                    LastName = "Collins",
                    IsSupervisor = true,
                    DepartmentId = 4

                };
                var modifiedAllisonAsJSON = JsonConvert.SerializeObject(modifiedAllison);

                var response = await client.PutAsync(
                    "/api/employee/1",
                    new StringContent(modifiedAllisonAsJSON, Encoding.UTF8, "application/json"));

                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                var getAllison = await client.GetAsync("/api/employee/1");
                getAllison.EnsureSuccessStatusCode();

                string getAllisonBody = await getAllison.Content.ReadAsStringAsync();
                Employee newAllison = JsonConvert.DeserializeObject<Employee>(getAllisonBody);

                Assert.Equal(HttpStatusCode.OK, getAllison.StatusCode);
                Assert.Equal("Real Allison", newAllison.FirstName);

            }
        }

        //Post employee test
        [Fact]
        public async Task Test_Create_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {
                Employee austin = new Employee
                {
                    FirstName = "Austin",
                    LastName = "Blade",
                    IsSupervisor = false,
                    DepartmentId = 4
                };

                var austinAsJSON = JsonConvert.SerializeObject(austin);

                var response = await client.PostAsync(
                    "/api/employee",
                    new StringContent(austinAsJSON, Encoding.UTF8, "application/json"));

                string responseBody = await response.Content.ReadAsStringAsync();

                var newAustin = JsonConvert.DeserializeObject<Employee>(responseBody);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Austin", newAustin.FirstName);
            }
        }
    }
}
