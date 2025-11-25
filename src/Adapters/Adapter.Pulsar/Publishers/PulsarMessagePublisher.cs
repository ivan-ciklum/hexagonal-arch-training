using ClubExample.Core.OutputPorts;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using ClubExample.Adapter.Pulsar.Configuration;

namespace ClubExample.Adapter.Pulsar.Publishers;

public sealed class PulsarMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly IPulsarClient _pulsarClient;
    private readonly ILogger<PulsarMessagePublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PulsarMessagePublisher(
        IOptions<PulsarOptions> options,
        ILogger<PulsarMessagePublisher> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        // Configure JSON serialization for message payloads
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Initialize Pulsar client with configuration
        var pulsarOptions = options.Value;
        
        try
        {
            var clientBuilder = PulsarClient.Builder()
                .ServiceUrl(new Uri(pulsarOptions.ServiceUrl));

            _pulsarClient = clientBuilder.Build();

            _logger.LogInformation(
                "Pulsar client initialized successfully. ServiceUrl: {ServiceUrl}",
                pulsarOptions.ServiceUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Pulsar client");
            throw new InvalidOperationException("Failed to initialize Pulsar message publisher", ex);
        }
    }

    public async Task PublishAsync<T>(
        string topic, 
        T message, 
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentNullException.ThrowIfNull(message);

        IProducer<byte[]>? producer = null;

        try
        {
            _logger.LogDebug(
                "Publishing message to topic: {Topic}, MessageType: {MessageType}",
                topic,
                typeof(T).Name);

            // Create producer for the topic
            producer = _pulsarClient.NewProducer(Schema.ByteArray)
                .Topic(topic)
                .Create();

            // Serialize message to JSON
            var messageBytes = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);

            // Build and send message
            var messageId = await producer.Send(messageBytes, cancellationToken);

            _logger.LogInformation(
                "Message published successfully. Topic: {Topic}, MessageId: {MessageId}, MessageType: {MessageType}",
                topic,
                messageId,
                typeof(T).Name);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Message publishing was cancelled. Topic: {Topic}, MessageType: {MessageType}",
                topic,
                typeof(T).Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish message. Topic: {Topic}, MessageType: {MessageType}",
                topic,
                typeof(T).Name);

            throw new InvalidOperationException(
                $"Failed to publish message to topic '{topic}'", ex);
        }
        finally
        {
            // Clean up producer resources
            if (producer != null)
            {
                await producer.DisposeAsync();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _pulsarClient.DisposeAsync();
            _logger.LogInformation("Pulsar client disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing Pulsar client");
        }
    }
}
