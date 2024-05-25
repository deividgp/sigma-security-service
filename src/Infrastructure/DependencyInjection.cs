namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddPersistence(configuration);
        services.AddRepositories();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped(options =>
        {
            return new ApplicationDbContext(
                configuration.GetConnectionString("Connection")!,
                configuration.GetConnectionString("Database")!
            );
        });

        return services;
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<,>), typeof(MongoRepository<,>));
        services.AddScoped(typeof(IAuthenticationService), typeof(AuthenticationService));
        services.AddScoped(typeof(ITokenService), typeof(TokenService));
    }
}
