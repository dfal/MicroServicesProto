using System;
using System.Net;
using System.Text;
using System.Web.Http;
using Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApi
{
	class RpcClient : IDisposable
	{
		const string ReplyQueue = "web.api.rpc.client.response.queue";
		
		readonly string rpcQueue;
		readonly IModel channel;
		readonly QueueingBasicConsumer consumer;

		public RpcClient(IConnection connection, string rpcQueue)
		{
			this.rpcQueue = rpcQueue;
			
			channel = connection.CreateModel();
			channel.ExchangeDeclare(rpcQueue, "fanout", false);

			channel.QueueDeclare(ReplyQueue, false, true, true, null);

			consumer = new QueueingBasicConsumer(channel);
			channel.BasicConsume(ReplyQueue, true, consumer);
		}

		public byte[] Call(string query, object body, int millisecondsTimeout = 1000)
		{
			return Call(query, JsonSerializer.Serialize(body), millisecondsTimeout);
		}

		public byte[] Call(string query, byte[] body, int millisecondsTimeout = 1000)
		{
			var correlationId = Guid.NewGuid().ToString();
			var properties = channel.CreateBasicProperties();
			properties.MessageId = Guid.NewGuid().ToString();
			properties.Type = query;
			properties.ReplyTo = ReplyQueue;
			properties.CorrelationId = correlationId;

			channel.BasicPublish("", rpcQueue, properties, body);

			while (true)
			{
				BasicDeliverEventArgs ea;
				if (!consumer.Queue.Dequeue(millisecondsTimeout, out ea))
					throw new TimeoutException(String.Format("Request '{0}' is timed out", query));
				
				if (ea.BasicProperties.CorrelationId != correlationId) continue;
					
				CheckError(ea);
				return ea.BasicProperties.Type == "NULL" ? null : ea.Body;
			}
		}

		static void CheckError(BasicDeliverEventArgs ea)
		{
			if (ea.BasicProperties.Type != "ERROR") return;
			var error = Convert.ToString(ea.BasicProperties.Headers["errorCode"]);
			switch (error)
			{
				case "NotFound":
					throw new HttpResponseException(HttpStatusCode.NotFound);
				default:
					throw new Exception(Encoding.UTF8.GetString(ea.Body));
			}
		}

		public void Dispose()
		{
			channel.Dispose();
		}
	}
}