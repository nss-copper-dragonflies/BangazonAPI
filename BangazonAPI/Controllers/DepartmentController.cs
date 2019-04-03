using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration configuration)
        {
            _config = configuration;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: api/Department
        [HttpGet]
        public IEnumerable<Department> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "employees")
                    {
                        cmd.CommandText = @"select  department.id as Departmentid, 
                                                    department.[Name] as departmentName, 
                                                    department.budget, 
                                                    employee.Id as employeeId, 
                                                    employee.FirstName, 
                                                    employee.LastName,
                                                    employee.isSupervisor,
                                                    employee.DepartmentId 
                                            from Department 
                                            left join employee on Department.Id = employee.DepartmentId
                                            Where 1=1";
                    }
                    else
                    {
                        cmd.CommandText = @"select department.id as Departmentid, department.[name] as departmentName, department.budget
                                            From department
                                            Where 1=1";
                    }
                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        cmd.CommandText += @" AND
                                                (department.[name] LIKE @q)";
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Department> departmentDictionary = new Dictionary<int, Department>();

                    while (reader.Read())
                    {
                        int Departmentid = reader.GetInt32(reader.GetOrdinal("Departmentid"));
                        if (!departmentDictionary.ContainsKey(Departmentid))
                        {
                            Department newDepartment = new Department
                            {
                                id = Departmentid,
                                Name = reader.GetString(reader.GetOrdinal("departmentName")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            };
                            departmentDictionary.Add(Departmentid, newDepartment);
                        }

                        if (include == "employees")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("employeeId")))
                            {
                                Department currentDepartment = departmentDictionary[Departmentid];
                                currentDepartment.employeeList.Add(
                                   new Employee
                                   {
                                       id = reader.GetInt32(reader.GetOrdinal("employeeId")),
                                       FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                       LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                       isSupervisor = reader.GetBoolean(reader.GetOrdinal("isSupervisor")),
                                       DepartmentId = reader.GetInt32(reader.GetOrdinal("departmentId"))

                                   }
                                );
                            }
                        }
                    }
                    reader.Close();
                    return departmentDictionary.Values.ToList();
                }
            }
        }

        // GET: api/Department/5
        [HttpGet("{id}", Name = "GetSpecific")]
        public void  Get(int id)
        {

        }

        // POST: api/Department
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Department/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
