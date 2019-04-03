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
//Function: Performs the Get list, Get Single, Post, and Put for the Customer's Resource. Allows for querying,including payment type details, and including customer product details.

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

        // Gets all customers. Customers can include payment type and product type. Customers can be queried.
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
                                            p.Id AS ProductId,
			                                p.title,
                                            p.productTypeId,
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
                        cmd.CommandText = @"SELECT c.Id AS CustomerId, c.FirstName, c.LastName 
                                                FROM Customer c
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

                        if (include == "product")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                            {
                                Customer currentCustomer = customers[customerId];
                                currentCustomer.Products.Add(
                                    new Product
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                        ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                        CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                        Price = reader.GetInt32(reader.GetOrdinal("Price")),
                                        Title = reader.GetString(reader.GetOrdinal("Title")),
                                        Description = reader.GetString(reader.GetOrdinal("Description")),
                                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                                    }
                                    );
                            }
                        }

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

        // Get a single customer based on customer id. The customer information can include customer products and customer payment types.
        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult Get(int id, string include)
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
                                            p.Id AS ProductId,
			                                p.title,
                                            p.productTypeId,
			                                p.[description],
			                                p.price,
			                                p.quantity
		                                FROM Customer c
			                                LEFT JOIN Product p on c.Id = p.CustomerId ";
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
			                                LEFT JOIN PaymentType a on c.Id = a.CustomerId";
                    }
                    else
                    {
                        cmd.CommandText = @"SELECT c.Id AS CustomerId, c.FirstName, c.LastName 
                                                FROM Customer c";
                    }

                    cmd.CommandText += " WHERE CustomerId = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer customer = null;
                    while (reader.Read())
                    {
                        if(customer == null)
                        {
                            customer = new Customer
                            {
                                Id = id,
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName"))
                            };
                        }

                        if (include == "product")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("ProductId")))
                            {
                                customer.Products.Add(
                                    new Product
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                        ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                        CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                        Price = reader.GetInt32(reader.GetOrdinal("Price")),
                                        Title = reader.GetString(reader.GetOrdinal("Title")),
                                        Description = reader.GetString(reader.GetOrdinal("Description")),
                                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                                    }
                                    );
                            }
                        }

                        if (include == "payment")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                            {
                                customer.PaymentTypes.Add(
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
                    return Ok(customer);
                }
            }
        }

        // Post a new customer
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

        // Put an existing customer based on customer Id
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
