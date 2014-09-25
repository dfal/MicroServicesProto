using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using RabbitMQ.Client;

namespace WebApi.Controllers
{
	public class ControllerActivator : IHttpControllerActivator
	{
		readonly IConnection connection;

		public ControllerActivator(IConnection connection)
		{
			this.connection = connection;
		}


		public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
		{
			if (controllerType == typeof (CustomerController))
			{
				return new CustomerController(connection);
			}
			return (IHttpController)Activator.CreateInstance(controllerType);
		}
	}
}