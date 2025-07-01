using DotnetExam.Data.Contexts;
using DotnetExam.Data.Models;
using DotnetExam.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetExam.Data;

public static class AddDataExtensions
{
    public static IServiceCollection AddDataConfigured(this IServiceCollection services, string clickhouseConnectionString)
    {
        services.AddSingleton<AppClickHouseContext>(_ => new AppClickHouseContext(clickhouseConnectionString));
        services.AddScoped<IClickHouseRepository<Note>, ClickHouseRepository<Note>>();

        return services;
    }
}