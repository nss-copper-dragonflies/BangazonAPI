using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

//Author: Brittany Ramos-Janeway
//Function: Performs the GET single, GET all, POST, and PUT for the Employee Resource

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {

        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // Gets all employees from the database and their corresponding department and computer information
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.FirstName, 
                                            e.LastName, e.IsSupervisor, 
                                            e.DepartmentId, d.[Name], d.Budget, 
                                            c.Id AS ComputerId, c.Make, c.Manufacturer, 
                                            c.PurchaseDate, c.DecomissionDate
                                        FROM Employee e 
		                                    INNER JOIN Department d ON  e.DepartmentId = d.Id
		                                    INNER JOIN ComputerEmployee o ON e.Id = o.EmployeeId
		                                    INNER JOIN Computer c ON o.ComputerId = c.Id";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();

                    //Reads through the employee data and adds each one to the list of employees
                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Department = new Department
                            {
                                id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            }
                        };

                        //must check to see if a computer is assigned to an employee
                        if (!reader.IsDBNull(reader.GetOrdinal("ComputerId")))
                        {
                            employee.Computer = new Computer
                            {
                                id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                purchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                //must first check whether the decomission date returns null, then return accordingly
                                DecommisionDate = reader.IsDBNull(reader.GetOrdinal("DecomissionDate"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("DecomissionDate")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                            };
                        }
                            
                        employees.Add(employee);
                    }
                    reader.Close();

                    return Ok(employees);
                }
            }
        }

        // Gets an individual employee and their corresponding department and computer information
        [HttpGet("{id}", Name = "GetEmployee")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.FirstName, 
                                            e.LastName, e.IsSupervisor, 
                                            e.DepartmentId, d.[Name], d.Budget, 
                                            c.Id AS ComputerId, c.Make, c.Manufacturer, 
                                            c.PurchaseDate, c.DecomissionDate
                                        FROM Employee e 
		                                    INNER JOIN Department d ON  e.DepartmentId = d.Id
		                                    INNER JOIN ComputerEmployee o ON e.Id = o.EmployeeId
		                                    INNER JOIN Computer c ON o.ComputerId = c.Id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee employee = null;

                    if (reader.Read())
                    {
                        employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Department = new Department
                            {
                                id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            }
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("ComputerId")))
                        {
                            employee.Computer = new Computer
                            {
                                id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                purchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                //must first check whether the decomission date returns null, then return accordingly
                                DecommisionDate = reader.IsDBNull(reader.GetOrdinal("DecomissionDate"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("DecomissionDate")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                            };
                        }
                   
                    }
                    reader.Close();

                    return Ok(employee);
                }
            }
        }

        // Posts a new employee to the database
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Employee 
                                            (FirstName, LastName, 
                                            IsSupervisor, DepartmentId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, 
                                            @IsSupervisor, @DepartmentId)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                    cmd.Parameters.Add(new SqlParameter("@IsSupervisor", employee.IsSupervisor));
                    cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));

                    int newId = (int)cmd.ExecuteScalar();
                    employee.Id = newId;
                    return CreatedAtRoute(new { id = newId }, employee);
                }
            }
        }

        // Put method will modify existing employees
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee
                                            SET FirstName = @FirstName,
                                                LastName = @LastName,
                                                IsSupervisor = @IsSupervisor,
                                                DepartmentId = @DepartmentId";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@IsSupervisor", employee.IsSupervisor));
                        cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //catches attempts to modify employees that do not exist
        private bool EmployeeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @" SELECT Id, FirstName, LastName, 
                                            IsSupervisor, DepartmentId
                                        FROM Employee
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
