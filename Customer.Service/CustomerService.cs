using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Customer.Service
{
	class CustomerService
	{
		const string QueueHost = "localhost";
		const string CommandsQueue = "customer.command.queue";
		const string CommandsErrorQueue = "customer.command.error.queue";
		const string EventsExchange = "customer.event.exchange";

		IConnection connection;
		IModel channel;
		QueueingBasicConsumer consumer;
		Task worker;
		volatile bool running;

		static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public void Start()
		{
			connection = new ConnectionFactory { HostName = QueueHost, RequestedHeartbeat = 30 }.CreateConnection();
			
			channel = connection.CreateModel();
			channel.ExchangeDeclare(EventsExchange, "fanout", true);
			channel.QueueDeclare(queue: CommandsQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);
			channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
			consumer = new QueueingBasicConsumer(channel);
			channel.BasicConsume(queue: CommandsQueue, noAck: false, consumer: consumer);

			var repository = new EventSourcedRepository<Customer>(new InMemoryEventStore(Publish));
			var commandHandler = new CommandHandler(repository);

			running = true;
			worker = Task.Factory.StartNew(() => CommandHandlingTask(commandHandler));
		}

		void CommandHandlingTask(CommandHandler commandHandler)
		{
			while (running)
			{
				BasicDeliverEventArgs ea;
				try { ea = consumer.Queue.Dequeue(); }
				catch (EndOfStreamException) { break; }// The consumer was cancelled, the model closed, or the connection went away.

				try
				{
					commandHandler.Handle(ea.BasicProperties.MessageId, ea.BasicProperties.Type, ea.Body);
					channel.BasicAck(ea.DeliveryTag, false);
					Logger.Info("[{0}]: {1}", ea.BasicProperties.Type, Encoding.UTF8.GetString(ea.Body));
				}
				catch (Exception ex)
				{
					channel.BasicReject(ea.DeliveryTag, false);
					Logger.Error(ex.ToString());
					channel.QueueDeclare(queue: CommandsErrorQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
					channel.BasicPublish("", CommandsErrorQueue, ea.BasicProperties, ea.Body);
				}
				
			}
		}

		public void Stop()
		{
			running = false;
			
			worker.Wait(TimeSpan.FromSeconds(10));
			
			channel.Dispose();
			connection.Dispose();
		}

		void Publish(IEvent e)
		{
			var properties = channel.CreateBasicProperties();
			properties.Type = e.GetType().Name;

			var body = JsonSerializer.Serialize(e);

			channel.BasicPublish(EventsExchange, "", properties, body);
		}
	}
}
