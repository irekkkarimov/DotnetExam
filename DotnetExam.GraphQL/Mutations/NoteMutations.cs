using DotnetExam.Data.Models;
using DotnetExam.Data.Services;
using Microsoft.Extensions.Logging;

namespace DotnetExam.GraphQL.Mutations;

public class NoteMutations(IClickHouseRepository<Note> noteRepository, ILogger<NoteMutations> logger)
{
    [GraphQLName("addNote")]
    public async Task<Note> AddAsync(Note note)
    {
        try
        {
            await noteRepository.AddAsync(note);

            return note;
        }
        catch (Exception e)
        {
            throw LogErrorAndGetGraphQlException(e);
        }
    }

    [GraphQLName("updateNote")]
    public async Task<int> UpdateAsync(Note note)
    {
        try
        {
            await noteRepository.UpdateAsync(note);

            return note.Id;
        }
        catch (Exception e)
        {
            throw LogErrorAndGetGraphQlException(e);
        }
    }

    [GraphQLName("deleteNote")]
    public async Task<int> DeleteAsync(int id)
    {
        try
        {
            await noteRepository.RemoveAsync(id);

            return id;
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