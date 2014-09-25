using Topshelf;

namespace Customer.Service
{
	class Program
	{
		static void Main()
		{
			HostFactory.Run(x =>
			{
				x.Service<CustomerService>(s =>
				{
					s.ConstructUsing(name => new CustomerService());
					s.WhenStarted(invs => invs.Start());
					s.WhenStopped(invs => invs.Stop());
				});
				
				x.RunAsLocalSystem();
				x.SetDescription("Customer Service Host");
				x.SetDisplayName("Customer Service");
				x.SetServiceName("CustomerService");
			});
		}
	}
}
