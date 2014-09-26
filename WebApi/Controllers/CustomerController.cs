using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using RabbitMQ.Client;

namespace WebApi.Controllers
{
	[CorsPolicy]
	public class CustomerController : ApiController
	{
		readonly IConnection connection;
		const string CustomerQueryExchange = "customer.query.exchange";
		const string CustomerCommandQueue = "customer.command.queue";

		public CustomerController(IConnection connection)
		{
			this.connection = connection;
		}
		
		[HttpGet]
		public HttpResponseMessage Get(Guid id)
		{
			using (var service = new RpcClient(connection, CustomerQueryExchange))
			{
				var result = service.Call("GetCustomer", new { customerId = id }, 60000);
				
				if (result != null && result.Length > 0)
					return OK(result);

				return Request.CreateErrorResponse(HttpStatusCode.NotFound,
					string.Format("Customer with id '{0}' was not found.", id));
			}
		}

		[HttpGet]
		public HttpResponseMessage Get()
		{
			using (var service = new RpcClient(connection, CustomerQueryExchange))
			{
				var result = service.Call("GetAllCustomers", new { orderBy = "name", desc = false });
				return OK(result);
			}
		}

		[HttpDelete]
		public HttpResponseMessage Delete(Guid id)
		{
			using (var channel = connection.CreateModel())
			{
				var properties = channel.CreateBasicProperties();
				properties.Type = "DeleteCustomer";

				channel.BasicPublish("", CustomerCommandQueue, properties, new { customerId = id }.ToJsonBytes());
				return CreateResponse(HttpStatusCode.Accepted);
			}
		}

		[HttpPost]
		public HttpResponseMessage CreateCustomer([FromBody] Customer customer)
		{
			using (var channel = connection.CreateModel())
			{
				var properties = channel.CreateBasicProperties();
				properties.Type = "CreateCustomer";
				
				channel.BasicPublish("", CustomerCommandQueue, properties, new
				{
					CustomerId = Guid.NewGuid(),
					customer.Name,
					customer.Email,
					customer.VatNumber
				}.ToJsonBytes());
				return CreateResponse(HttpStatusCode.Accepted);
			}
		}

		[HttpPut]
		public HttpResponseMessage UpdateCustomer([FromBody] Customer customer)
		{
			using (var channel = connection.CreateModel())
			{
				var properties = channel.CreateBasicProperties();
				properties.Type = "UpdateCustomer";

				channel.BasicPublish("", CustomerCommandQueue, properties, new 
				{
					CustomerId = customer.Id,
					customer.Name,
					customer.Email,
					customer.VatNumber
				}.ToJsonBytes());
				return CreateResponse(HttpStatusCode.Accepted);
			}
		}

		HttpResponseMessage OK(byte[] result)
		{
			var response = CreateResponse(HttpStatusCode.OK);
			response.Content = new ByteArrayContent(result);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			return response;
		}

		HttpResponseMessage CreateResponse(HttpStatusCode statusCode)
		{
			var response = Request.CreateResponse(statusCode);

			return response;
		}

		public class Customer
		{
			public Guid Id;
			public string Name;
			public string Email;
			public string VatNumber;
		}
	}
}
