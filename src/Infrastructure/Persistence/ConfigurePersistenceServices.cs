using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class ConfigurePersistenceServices
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured");

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddScoped<VacancyRepository>();
        services.AddScoped<IVacancyRepository>(sp => sp.GetRequiredService<VacancyRepository>());
        services.AddScoped<IVacancyQueries>(sp => sp.GetRequiredService<VacancyRepository>());
    }
}
