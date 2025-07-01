using DotnetExam.Data.Models;
using DotnetExam.Data.Services;
using Microsoft.Extensions.Logging;

namespace DotnetExam.GraphQL.Queries;

public class NoteQueries(IClickHouseRepository<Note> noteRepository, ILogger<NoteQueries> logger)
{
    [GraphQLName("getNotes")]
    public async Task<IEnumerable<Note>> GetAllAsync()
    {
        try
        {
            var allNotes = await noteRepository.GetAllAsync();

            return allNotes;
        }
        catch (Exception e)
        {
            throw LogErrorAndGetGraphQlException(e);
        }
    }
    
    private GraphQLException LogErrorAndGetGraphQlException(Exception e)
    {
        logger.LogError(e, "{Message}", e.Message);
        throw new GraphQLException(e.Message);
    }
}