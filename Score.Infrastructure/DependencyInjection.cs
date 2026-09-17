using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Score.Core.Repositories;
using Score.Infrastructure.Data;
using Score.Infrastructure.Repositories;

namespace Score.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddScoreInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A database connection string is required.", nameof(connectionString));
        }

        services.AddDbContext<ScoreDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IScoreRepository, EfScoreRepository>();

        return services;
    }
}
