using System.Web.Http;
using System.Web.Http.Dispatcher;
using RabbitMQ.Client;
using WebApi.Controllers;

namespace WebApi
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		const string QueueHost = "localhost";

		protected void Application_Start()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);

			var connection = new ConnectionFactory() {HostName = QueueHost, RequestedHeartbeat = 30}.CreateConnection();
			
			GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new ControllerActivator(connection));
		}
	}
}
