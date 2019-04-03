using BangazonAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
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
    }
}
