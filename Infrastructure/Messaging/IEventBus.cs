namespace Infrastructure.Messaging
{
	public interface IEventBus
	{
		void Publish<T>(Envelope<T> @event) where T : IEvent;
	}
}
