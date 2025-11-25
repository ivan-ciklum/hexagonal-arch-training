namespace ClubExample.Core.OutputPorts;

public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the specified topic.
    /// </summary>
    /// <typeparam name="T">The type of message to publish.</typeparam>
    /// <param name="topic">The topic name where the message will be published.</param>
    /// <param name="message">The message content to publish.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default) where T : class;
}
