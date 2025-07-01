using DotnetExam.GraphQL.Mutations;
using DotnetExam.GraphQL.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetExam.GraphQL;

public static class AddGraphQlExtensions
{
    public static IServiceCollection AddGraphQlConfigured(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType<NoteQueries>()
            .AddMutationType<NoteMutations>();

        return services;
    }
}