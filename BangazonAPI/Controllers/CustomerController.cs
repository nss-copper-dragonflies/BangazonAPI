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
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration configuration)
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

        // GET: api/Customer
        [HttpGet]
        public IActionResult Get()
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FirstName, LastName FROM Customer";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Customer> customers = new List<Customer>();
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        });
                    }

                    reader.Close();
                    return Ok(customers);
                }
            }
        }

        // GET: api/Customer/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult GetCustomer(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT FirstName, LastName 
                                            FROM Customer
                                            WHERE Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer customer = null;
                    if (reader.Read())
                    {
                        customer = new Customer
                        {
                            Id = id,
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        };
                    }

                    reader.Close();
                    return Ok(customer);
                }
            }
        }

        // POST: api/Customer
        [HttpPost]
        public IActionResult Post([FromBody] Customer newCustomer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Customer (FirstName, LastName)
                                            OUTPUT INSERTED.Id
                                            VALUES (@FirstName, @LastName)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", newCustomer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", newCustomer.LastName));

                    int newId = (int) cmd.ExecuteScalar();

                    newCustomer.Id = newId;
                    return CreatedAtRoute("GetCustomer", new { id = newId }, newCustomer);
                }
            }
        }

        // PUT: api/Customer/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Customer updatedCustomer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Customer
                                            SET FirstName = @FirstName,
                                                LastName = @LastName
                                            WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", updatedCustomer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", updatedCustomer.LastName));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();

                    return NoContent();
                }
            }
        }
    }
}
