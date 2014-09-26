using System;
using Infrastructure.EventSourcing;

namespace Customer.Service
{
	class Customer : EventSourced
	{
		bool deleted;
		string name;
		string email;
		string vatNumber;

		public Customer(Guid id)
			: base(id)
		{
			Handles<CustomerCreated>(OnCreated);
			Handles<CustomerDeleted>(OnDeleted);
			Handles<CustomerRenamed>(OnRenamed);
			Handles<CustomerEmailChanged>(OnEmailChanged);
			Handles<CustomerVatNumberChanged>(OnVatNumberChanged);
		}

		public Customer(Guid id, string name, string vatNumber, string email)
			: this(id)
		{
			Apply(new CustomerCreated
			{
				Name = name,
				VatNumber = vatNumber,
				Email = email
			});
		}

		public void Delete()
		{
			Apply(new CustomerDeleted());
		}

		public void SetName(string newName)
		{
			AssertNotDeleted();

			if (name != newName) Apply(new CustomerRenamed
			{
				OldName = name,
				NewName = newName
			});
		}

		public void SetEmail(string newEmail)
		{
			AssertNotDeleted();
			if (email != newEmail) Apply(new CustomerEmailChanged
			{
				OldEmail = email,
				NewEmail = newEmail
			});
		}

		public void SetVatNumber(string newVatNumber)
		{
			AssertNotDeleted();
			if (vatNumber != newVatNumber) Apply(new CustomerVatNumberChanged
			{
				OldVatNumber = vatNumber,
				NewVatNumber = newVatNumber
			});
		}

		void AssertNotDeleted()
		{
			if (deleted)
				throw new InvalidOperationException("Customer already deleted.");
		}

		void OnCreated(CustomerCreated @event)
		{
			name = @event.Name;
			email = @event.Email;
			vatNumber = @event.VatNumber;
			deleted = false;
		}

		void OnRenamed(CustomerRenamed @event)
		{
			name = @event.NewName;
		}

		void OnEmailChanged(CustomerEmailChanged @event)
		{
			email = @event.NewEmail;
		}

		void OnVatNumberChanged(CustomerVatNumberChanged @event)
		{
			vatNumber = @event.NewVatNumber;
		}

		void OnDeleted(CustomerDeleted @event)
		{
			deleted = true;
		}
	}
}
