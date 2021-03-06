﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

//JORDAN ROSAS: PaymentType Controller. This method will have the HTTP methods to get post put and delete data 

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypeController(IConfiguration config)
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
        // GET: api/PaymentType
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn  = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"Select id, [name], AcctNumber, CustomerId from PaymentType";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<PaymentType> paymentTypes = new List<PaymentType>();
                    while(reader.Read())
                    {
                        PaymentType newPaymentType = new PaymentType
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))
                        };
                        paymentTypes.Add(newPaymentType);
                    }
                    reader.Close();
                    return Ok(paymentTypes);   
                } 
            }
        }

        // GET: api/PaymentType/5
        [HttpGet("{id}", Name = "GetPaymentTypes")]
        public async Task<IActionResult> Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"Select id, [name], AcctNumber, CustomerId 
                                        from PaymentType 
                                        where id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    PaymentType specific_PaymentType = null;

                    if (reader.Read())
                    {
                        specific_PaymentType = new PaymentType
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))
                        };
                    }
                    reader.Close();
                    return Ok(specific_PaymentType);
                }
            }
        }

        // POST: api/PaymentType
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentType paymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO PaymentType ([Name], AcctNumber, CustomerId)
                                        OUTPUT INSERTED.id
                                        Values (@name, @AcctNumber, @CustomerId)";
                    cmd.Parameters.Add(new SqlParameter("@name", paymentType.Name));
                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));
                    cmd.ExecuteNonQuery();

                    int newId = (int)cmd.ExecuteScalar();
                    paymentType.id = newId;
                    return CreatedAtRoute(new { id = newId }, paymentType);
                }

            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute]int id, [FromBody] PaymentType paymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType
                                            SET 
                                            AcctNumber =  @acctNumber,
                                            Name = @name,
                                            customerId = @customerId
                                        WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@customerId", paymentType.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@acctNumber", paymentType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@name", paymentType.Name));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsEffected = cmd.ExecuteNonQuery();
                        if (rowsEffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows Effected");
                    }
                }
            }
            catch (Exception)
            {
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    
                    cmd.CommandText = @"delete from paymenttype where id = @id 
                                        and not exists(select paymenttypeid from [order] where paymenttypeid = @id)";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if(rowsAffected == 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status403Forbidden);
                    }
                    else
                    {
                        return new StatusCodeResult(StatusCodes.Status200OK);
                    }
                }
            }
           
        }

        private bool PaymentTypeExists(int id)
        {

            using (SqlConnection conn = Connection)
            {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = $@"SELECT Id, [Name], AcctNumber, CustomerId
                                           FROM PaymentType 
                                          WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
