using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace WebApi
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public class CorsPolicyAttribute : Attribute, ICorsPolicyProvider
	{
		readonly CorsPolicy policy;

		public CorsPolicyAttribute()
		{
			policy = new CorsPolicy
			{
				AllowAnyMethod = true,
				AllowAnyHeader = true
			};

			policy.Origins.Add("http://localhost:3000");
		}

		public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return Task.FromResult(policy);
		}
	}
}