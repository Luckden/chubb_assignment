namespace Claims.Api.Infrastructure.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(KafkaOptions.SectionName);
        services.Configure<KafkaOptions>(section);
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        services.AddSingleton<KafkaTopicProvisioner>();

        // Always on: events are written to the outbox whether or not Kafka is reachable, and the relay catches up.
        services.AddHostedService<OutboxRelay>();

        if (section.GetValue<bool>(nameof(KafkaOptions.ConsumerEnabled)))
        {
            services.AddHostedService<KafkaConsumerService>();
        }

        return services;
    }
}
