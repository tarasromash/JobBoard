using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Common;

public class IntegrationTestWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("job-board-tests")
        .WithUsername("tests_user")
        .WithPassword("tests_password")
        .Build();

    private WebApplicationFactory<Program>? _authenticatedFactory;

    // One authenticated host per fixture, so tests do not start a new server each time.
    public WebApplicationFactory<Program> AuthenticatedFactory
        => _authenticatedFactory ??= this.WithWebHostBuilderMock();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false);
            config.AddEnvironmentVariables();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveServiceByType(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveServiceByType(typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

            services.AddDbContext<ApplicationDbContext>(options => options
                .UseNpgsql(_dbContainer.GetConnectionString(),
                    npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention());
        });
    }

    public Task InitializeAsync() => _dbContainer.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
