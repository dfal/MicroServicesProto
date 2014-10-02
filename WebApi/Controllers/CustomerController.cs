using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Infrastructure;
using RabbitMQ.Client;

namespace WebApi.Controllers
{
	[CorsPolicy]
	public class CustomerController : ApiController
	{
		readonly IConnection connection;
		const string CustomerQueryQueue = "customer.query.queue";
		const string CustomerCommandQueue = "customer.command.queue";

		public CustomerController(IConnection connection)
		{
			this.connection = connection;
		}
		
		[HttpGet]
		public HttpResponseMessage Get(Guid id, string correlationId = null)
		{
			using (var service = new RpcClient(connection, CustomerQueryQueue))
			{
				var result = service.Call("FindCustomer", new { customerId = id, correlationId }, 60000);
				
				if (result != null && result.Length > 0)
					return OK(result);

				return Request.CreateErrorResponse(HttpStatusCode.NotFound,
					string.Format("Customer with id '{0}' was not found.", id));
			}
		}

		[HttpGet]
		public HttpResponseMessage Get(string correlationId = null)
		{
			using (var service = new RpcClient(connection, CustomerQueryQueue))
			{
				var result = service.Call("GetAllCustomers", new { orderBy = "name", desc = false, correlationId }, 60000);
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
				properties.MessageId = Guid.NewGuid().ToString();

				channel.BasicPublish("", CustomerCommandQueue, properties, new { customerId = id }.ToJsonBytes());
				return CreateResponse(HttpStatusCode.Accepted, new { correlationId = properties.MessageId });
			}
		}

		[HttpPost]
		public HttpResponseMessage CreateCustomer([FromBody] Customer customer)
		{
			using (var channel = connection.CreateModel())
			{
				var properties = channel.CreateBasicProperties();
				properties.Type = "CreateCustomer";
				properties.MessageId = Guid.NewGuid().ToString();
				
				channel.BasicPublish("", CustomerCommandQueue, properties, new
				{
					CustomerId = Guid.NewGuid(),
					customer.Name,
					customer.Email,
					customer.VatNumber
				}.ToJsonBytes());
				return CreateResponse(HttpStatusCode.Accepted, new { correlationId = properties.MessageId });
			}
		}

		[HttpPut]
		public HttpResponseMessage UpdateCustomer([FromBody] Customer customer)
		{
			using (var channel = connection.CreateModel())
			{
				var properties = channel.CreateBasicProperties();
				properties.Type = "UpdateCustomer";
				properties.MessageId = Guid.NewGuid().ToString();

				channel.BasicPublish("", CustomerCommandQueue, properties, new 
				{
					CustomerId = customer.Id,
					customer.Name,
					customer.Email,
					customer.VatNumber
				}.ToJsonBytes());
				return CreateResponse(HttpStatusCode.Accepted, new { correlationId = properties.MessageId });
			}
		}

		HttpResponseMessage OK(byte[] result)
		{
			var response = CreateResponse(HttpStatusCode.OK);
			response.Content = new ByteArrayContent(result);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			return response;
		}

		HttpResponseMessage CreateResponse(HttpStatusCode statusCode, object content = null)
		{
			var response = Request.CreateResponse(statusCode);
			if (content == null) return response;
			
			response.Content = new ByteArrayContent(JsonSerializer.Serialize(content));
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			
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
