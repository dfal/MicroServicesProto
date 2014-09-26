using System;
using Infrastructure;
using Infrastructure.EventSourcing;

namespace Customer.Service
{
	class CommandHandler
	{
		readonly EventSourcedRepository<Customer> repository;

		public CommandHandler(EventSourcedRepository<Customer> repository)
		{
			this.repository = repository;

			FillUpFakeData();
		}

		void FillUpFakeData()
		{
			OnCreateCustomer(new CreateCustomerMessage
			{
				CustomerId = Guid.NewGuid(),
				Name = "Good customer",
				Email = "good@customer.com",
				VatNumber = "123456789"
			});

			OnCreateCustomer(new CreateCustomerMessage
			{
				CustomerId = Guid.NewGuid(),
				Name = "Awesome customer",
				Email = "awesome@customer.com",
				VatNumber = "3456123456"
			});

			OnCreateCustomer(new CreateCustomerMessage
			{
				CustomerId = Guid.NewGuid(),
				Name = "The best customer",
				Email = "best@customer.com",
				VatNumber = "89457832"
			});
		}

		public void Handle(string command, byte[] body)
		{
			switch (command)
			{
				case "CreateCustomer":
					OnCreateCustomer(JsonSerializer.Deserialize<CreateCustomerMessage>(body));
					break;
				case "UpdateCustomer":
					OnUpdateCustomer(JsonSerializer.Deserialize<UpdateCustomerMessage>(body));
					break;
				case "DeleteCustomer":
					OnDeleteCustomer(JsonSerializer.Deserialize<DeleteCustomerMessage>(body));
					break;
				default:
					throw new InvalidOperationException(string.Format("Command '{0}' is not supported", command));
			}
		}

		void OnCreateCustomer(CreateCustomerMessage message)
		{
			var customer = new Customer(message.CustomerId, message.Name, message.VatNumber, message.Email);
			repository.Save(customer, Guid.NewGuid().ToString());
		}

		void OnUpdateCustomer(UpdateCustomerMessage message)
		{
			var customer = repository.Get(message.CustomerId);
			
			customer.SetName(message.Name);
			customer.SetEmail(message.Email);
			customer.SetVatNumber(message.VatNumber);

			repository.Save(customer, Guid.NewGuid().ToString());
		}

		void OnDeleteCustomer(DeleteCustomerMessage message)
		{
			var customer = repository.Get(message.CustomerId);
			customer.Delete();
			
			repository.Save(customer, Guid.NewGuid().ToString());
		}
	}

	class CreateCustomerMessage
	{
		public Guid CustomerId;
		public string Name;
		public string VatNumber;
		public string Email;
	}

	class UpdateCustomerMessage
	{
		public Guid CustomerId;
		public string Name;
		public string VatNumber;
		public string Email;
	}

	class DeleteCustomerMessage
	{
		public Guid CustomerId;
	}
}
