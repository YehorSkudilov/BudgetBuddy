using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetBuddyAPI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        //services.AddDbContext<AppDbContext>(opt =>
        //    opt.UseSqlite("Data Source=budgetbuddy.db"));
        services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("Default")));
        services.AddHttpClient<PlaidClient>();

        return services;
    }
}