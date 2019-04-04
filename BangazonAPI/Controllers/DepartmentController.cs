using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
//JORDAN ROSAS: Controller for the Get, Get single, put and post working with query strings and q='s
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
                                       Id = reader.GetInt32(reader.GetOrdinal("employeeId")),
                                       FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                       LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                       IsSupervisor = reader.GetBoolean(reader.GetOrdinal("isSupervisor")),
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
        //GET by Id with query strings!

        [HttpGet("{id}", Name = "GetSpecific")]
        public IActionResult Get(int id, string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //this if statement catches the search parameters for the words "product" and "payment"
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
                                            Where department.id = @id";
                    }
                    else
                    {
                        cmd.CommandText = @"select department.id as Departmentid, department.[name] as departmentName, department.budget
                                            From department Where department.id = @id";
                                            
                    }
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Department department = null;
                    while (reader.Read())
                    {
                        if (department == null)
                        {
                            department = new Department
                            {
                                id = reader.GetInt32(reader.GetOrdinal("departmentId")),
                                Name = reader.GetString(reader.GetOrdinal("departmentName")),
                                Budget = reader.GetInt32(reader.GetOrdinal("budget"))
                            };
                        }

                        //this "if" statement includes the details of a product with its corresponding customer in the case that product is included as a search parameter
                        if (include == "employees")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("employeeId")))
                            {
                                department.employeeList.Add(
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
                    return Ok(department);
                }
            }
        }


        // Posts a new customer to the database
        [HttpPost]
        public IActionResult Post([FromBody] Department newDepartment)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department ([name], budget)
                                            OUTPUT INSERTED.Id
                                            VALUES (@name, @budget)";
                    cmd.Parameters.Add(new SqlParameter("@name", newDepartment.Name));
                    cmd.Parameters.Add(new SqlParameter("@budget", newDepartment.Budget));

                    int newId = (int)cmd.ExecuteScalar();

                    newDepartment.id = newId;
                    return CreatedAtRoute( new { id = newId }, newDepartment);
                }
            }
        }

        // Put an existing customer based on customer Id
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Department updatedDepartment)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Department
                                            SET [Name] = @name,
                                                Budget = @budget
                                            WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@name", updatedDepartment.Name));
                    cmd.Parameters.Add(new SqlParameter("@budget", updatedDepartment.Budget));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();

                    return NoContent();
                }
            }
        }
    }
}
