namespace Claims.Api.Infrastructure.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(KafkaOptions.SectionName);
        services.Configure<KafkaOptions>(section);
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        if (section.GetValue<bool>(nameof(KafkaOptions.ConsumerEnabled)))
        {
            services.AddHostedService<KafkaConsumerService>();
        }

        return services;
    }
}
