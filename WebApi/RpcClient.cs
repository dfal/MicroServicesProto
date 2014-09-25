using System;
using Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApi
{
	class RpcClient : IDisposable
	{
		const string ReplyQueue = "web.api.rpc.client.response.queue";
		
		readonly string rpcExchange;
		readonly IModel channel;
		readonly QueueingBasicConsumer consumer;

		public RpcClient(IConnection connection, string rpcExchange)
		{
			this.rpcExchange = rpcExchange;
			
			channel = connection.CreateModel();
			channel.ExchangeDeclare(rpcExchange, "fanout", false);

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
			properties.Type = query;
			properties.ReplyTo = ReplyQueue;
			properties.CorrelationId = correlationId;

			channel.BasicPublish(rpcExchange, "", properties, body);

			while (true)
			{
				BasicDeliverEventArgs ea;
				if (consumer.Queue.Dequeue(millisecondsTimeout, out ea))
				{
					if (ea.BasicProperties.CorrelationId == correlationId)
					{
						return ea.Body;
					}
				}
				else
				{
					throw new TimeoutException(String.Format("Request '{0}' is timed out", query));
				}
			}
		}

		public void Dispose()
		{
			channel.Dispose();
		}
	}
}