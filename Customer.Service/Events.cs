using System;
using Infrastructure.Messaging;

namespace Customer.Service
{
	class CustomerCreated : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public string Name { get; set; }
		public string VatNumber { get; set; }
		public string Email { get; set; }
	}

	class CustomerRenamed : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public string NewName { get; set; }
		public string OldName { get; set; }
	}

	class CustomerEmailChanged : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public string OldEmail { get; set; }
		public string NewEmail { get; set; }
	}

	class CustomerVatNumberChanged : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }

		public string OldVatNumber { get; set; }
		public string NewVatNumber { get; set; }
	}

	class CustomerDeleted : IEvent
	{
		public Guid SourceId { get; set; }
		public int SourceVersion { get; set; }
	}
}
