using CSharpToJson.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpToJson.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.AddScoped<ICodeAnalyzer, CSharpCodeAnalyzer>();
        services.AddScoped<IJsonCodeWriter, CSharpJsonCodeWriter>();

        return services;
    }
}