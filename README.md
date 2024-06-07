<div align="center">
  <p align="center">
  <img src="https://github.com/bleichroeder/RabbitHutch/blob/main/logo.png?raw=true" width="350" title="hover text">
</p>
  <p align="center">
    A C# library designed to simplify the creation and management of RabbitMQ publishers and consumers. RabbitHutch provides an intuitive interface for developers to integrate RabbitMQ messaging into their applications seamlessly.
  </p>
</div>

## Publisher Types

### ```RabbitPublisher<T>```
RabbitPublisher<T> is a standard RabbitMQ publisher. It connects directly to a RabbitMQ exchange and publishes messages of type T to it.

### ```QueueingRabbitPublisher<T>```
QueueingRabbitPublisher<T> adds a layer of reliability by providing an internal message queue. This queue acts as a buffer in case of RabbitMQ outages, ensuring that messages are not lost and can be published once the connection is restored. This publisher is ideal for scenarios where message delivery guarantees are critical.

### ```DummyRabbitConsumer<T>```
DummyRabbitConsumer<T> is designed for demonstration and testing purposes. It does not create an actual connection with RabbitMQ, making it useful for testing application logic without requiring a live RabbitMQ instance. This consumer simulates message consumption and is helpful in development environments.

## Usage/Examples

### ```RabbitHutch.DependencyInjection```
#### Adding a ```IRabbitPublisher```
In this example, we configure RabbitHutch to add a dummy publisher for ```WeatherForecast``` objects. This publisher will send messages to a specified RabbitMQ exchange.
```
builder.Services.BuildRabbitHutch(hutchBuilder =>
{
    // Add a RabbitPublisher.
    hutchBuilder.AddDummyPublisher<WeatherForecast>(new RabbitPublisherSettings()
    {
        ConnectionString = new Uri(CONNECTION_STRING),
        ExchangeName = EXCHANGE_NAME,
        ContentType = MediaTypeNames.Application.Json,
        AutomaticRecovery = true,
        EnableAcks = true

    }, publisherConfigurator =>
    {
        publisherConfigurator.WithConnectionLifecycleProfile(ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile());
        publisherConfigurator.WithRoutingKeyFormatter(forecast => nameof(forecast));
        publisherConfigurator.WithSerializer(forecast => forecast.Serialize());
        publisherConfigurator.WithName("MyCustomDummyPublisher");
        publisherConfigurator.RegisterAsHostedService();
    });
});
```
#### Using a Publisher
The ```IRabbitPublisherFactory``` is injected and used to access any of your registered publishers by name/type.
```
[ApiController]
[Route("[controller]")]
public class WeatherForecastController(IRabbitPublisherFactory rabbitPublisherFactory) : ControllerBase
{
    private readonly IRabbitPublisher<WeatherForecast> _rabbitPublisher = rabbitPublisherFactory.GetPublisher<WeatherForecast>();

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<WeatherForecast> Get()
    {
        WeatherForecast forecast = new();

        await _rabbitPublisher.PublishAsync(forecast);

        return forecast;
    }
}
```

### Adding a ```IRabbitConsumer```
Here, we configure RabbitHutch to add a dummy consumer for ```WeatherForecast``` objects. This consumer will consume messages from a specified RabbitMQ queue.

```
builder.Services.BuildRabbitHutch(hutchBuilder =>
{
    // Add a RabbitConsumer.
    hutchBuilder.AddDummyConsumer<WeatherForecast>(new RabbitConsumerSettings()
    {
        ConnectionString = new Uri(CONNECTION_STRING),
        ExchangeName = EXCHANGE_NAME,
        QueueName = "WeatherForecastQueue",
        NackOnFalse = true,
        RequeueOnNack = false,
        AutomaticRecovery = true,
        RoutingKeys = ["#"],
        PrefetchCount = 1

    }, consumerConfigurator =>
    {
        consumerConfigurator.WithConnectionLifecycleProfile(ConnectionLifecycleProfiles.DefaultConnectionLifecycleProfile());
        consumerConfigurator.WithDeserializer(bytes => WeatherForecast.Deserialize(bytes));
        consumerConfigurator.WithNewMessageDelegate(LogIfFreezing);
        consumerConfigurator.WithName("MyCustomDummyConsumer");
        consumerConfigurator.RegisterAsHostedService();
    });
});

```
#### Using a Consumer
Basic consumers will run asynchronously for the life of the service, but can be accessed via DI using the ```IRabbitConsumerFactory``` for manual interaction.

### Manual Creation of Publishers and Consumers
If you prefer not to use dependency injection, you can manually create ```IRabbitPublisher``` and ```IRabbitConsumer``` instances.

```
DummyRabbitPublisher<string> publisher = new(new RabbitPublisherSettings()
{
    ConnectionString = new Uri("amqp://guest:guest@localhost:5672/"),
    ExchangeName = "TestExchange"
}, Logger);
```
```
DummyRabbitConsumer<TestClass> consumer = new(new RabbitConsumerSettings()
{
    ConnectionString = new Uri("amqp://guest:guest@localhost:5672/"),
    ExchangeName = "TestExchange"
},
async (testClass) =>
{
    messageConsumed = true;

    return await Task.FromResult(messageConsumed);

}, Logger);

consumer.Start();
```

## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

## Contact

David Herzfeld - [LinkedIn](https://www.linkedin.com/in/david-herzfeld-869344116) - davidbherzfeld@gmail.com
