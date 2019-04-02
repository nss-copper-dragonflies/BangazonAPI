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
        public IEnumerable<Customer> GetAllCustomers(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "product")
                    {
                        cmd.CommandText = @"SELECT c.Id AS CustomerId,
			                                c.FirstName, 
			                                c.LastName,
			                                p.title,
			                                p.[description],
			                                p.price,
			                                p.quantity
		                                FROM Customer c
			                                LEFT JOIN Product p on c.Id = p.CustomerId
                                        WHERE 1 = 1";
                    }
                    else if (include == "payment")
                    {
                        cmd.CommandText = @"SELECT c.Id AS CustomerId,
			                                c.FirstName, 
			                                c.LastName,
                                            a.Id AS PaymentTypeId,
			                                a.[name],
			                                a.acctNumber
		                                FROM Customer c
			                                LEFT JOIN PaymentType a on c.Id = a.CustomerId
                                        WHERE 1 = 1";
                    }
                    else
                    {
                        cmd.CommandText = @"SELECT Id AS CustomerId, FirstName, LastName 
                                                FROM Customer
                                                WHERE 1 = 1";
                    }

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        cmd.CommandText += @" AND
                                                (c.FirstName LIKE @q OR
                                                 c.LastName LIKE @q)";
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    Dictionary<int, Customer> customers = new Dictionary<int, Customer>();
                    while (reader.Read())
                    {
                        int customerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                        if (!customers.ContainsKey(customerId))
                        {
                            Customer customer = new Customer
                            {
                                Id = customerId,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName"))
                            };
                            customers.Add(customerId, customer);
                        }

                        //if (include == "products" || include == "product")
                        //{
                        //    if (!reader.IsDBNull(reader.GetOrdinal("productId")))
                        //    {
                        //        Customer currentCustomer = customers[customerId];
                        //        currentCustomer.Products.Add(
                        //            new Product
                        //            {
                        //                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                        //                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                        //                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        //                Price = reader.GetInt32(reader.GetOrdinal("Price")),
                        //                Title = reader.GetString(reader.GetOrdinal("Title")),
                        //                Description = reader.GetString(reader.GetOrdinal("Description")),
                        //                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                        //            }
                        //            );
                        //    }
                        //}

                        if (include == "payment")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                            {
                                Customer currentCustomer = customers[customerId];
                                currentCustomer.PaymentTypes.Add(
                                    new PaymentType
                                    {
                                        id = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                                        Name = reader.GetString(reader.GetOrdinal("name")),
                                        AcctNumber = reader.GetInt32(reader.GetOrdinal("acctNumber")),
                                        CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))
                                    }
                                );
                            }
                        }
                    }

                    reader.Close();

                    return customers.Values.ToList();
                }
            }
        }

        // GET: api/Customer/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult Get(int id)
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
