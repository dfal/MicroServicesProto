using System.Web.Http;

namespace WebApi
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.EnableCors();
			config.Routes.MapHttpRoute("allCustomers", "api/customers/{id}", new { controller = "Customer", id = RouteParameter.Optional });
		}
	}
}
