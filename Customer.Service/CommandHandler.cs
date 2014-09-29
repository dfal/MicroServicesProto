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
			OnCreateCustomer(Guid.NewGuid().ToString(), new CreateCustomerMessage
			{
				CustomerId = Guid.NewGuid(),
				Name = "Good customer",
				Email = "good@customer.com",
				VatNumber = "123456789"
			});

			OnCreateCustomer(Guid.NewGuid().ToString(), new CreateCustomerMessage
			{
				CustomerId = Guid.NewGuid(),
				Name = "Awesome customer",
				Email = "awesome@customer.com",
				VatNumber = "3456123456"
			});

			OnCreateCustomer(Guid.NewGuid().ToString(), new CreateCustomerMessage
			{
				CustomerId = Guid.NewGuid(),
				Name = "The best customer",
				Email = "best@customer.com",
				VatNumber = "89457832"
			});
		}

		public void Handle(string commandId, string command, byte[] body)
		{
			switch (command)
			{
				case "CreateCustomer":
					OnCreateCustomer(commandId, JsonSerializer.Deserialize<CreateCustomerMessage>(body));
					break;
				case "UpdateCustomer":
					OnUpdateCustomer(commandId, JsonSerializer.Deserialize<UpdateCustomerMessage>(body));
					break;
				case "DeleteCustomer":
					OnDeleteCustomer(commandId, JsonSerializer.Deserialize<DeleteCustomerMessage>(body));
					break;
				default:
					throw new InvalidOperationException(string.Format("Command '{0}' is not supported", command));
			}
		}

		void OnCreateCustomer(string commandId, CreateCustomerMessage message)
		{
			var customer = new Customer(message.CustomerId, message.Name, message.VatNumber, message.Email);
			repository.Save(customer, commandId);
		}

		void OnUpdateCustomer(string commandId, UpdateCustomerMessage message)
		{
			var customer = repository.Get(message.CustomerId);
			
			customer.SetName(message.Name);
			customer.SetEmail(message.Email);
			customer.SetVatNumber(message.VatNumber);

			repository.Save(customer, commandId);
		}

		void OnDeleteCustomer(string commandId, DeleteCustomerMessage message)
		{
			var customer = repository.Get(message.CustomerId);
			customer.Delete();
			
			repository.Save(customer, commandId);
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
