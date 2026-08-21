using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfiguration<TOptions>(this IServiceCollection services,
        IConfiguration configuration, bool validate = false)
        where TOptions : class, new()
    {
        var binder = services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(typeof(TOptions).Name));
        services.Configure<TOptions>(configuration);
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TOptions>>().Value);

        if (validate)
        {
            binder
                //.ValidateDataAnnotations()
                .ValidateOnStart();
        }

        return services;
    }
}