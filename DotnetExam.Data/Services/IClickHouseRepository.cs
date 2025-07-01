using DotnetExam.Data.Models.Common;

namespace DotnetExam.Data.Services;

public interface IClickHouseRepository<TEntity> where TEntity : EntityBase
{
    Task AddAsync(TEntity entity);
    Task RemoveAsync(int id);
    Task<IEnumerable<TEntity>> GetAllAsync();

    public Task UpdateAsync(TEntity entity);
    // Task<List<T>> FindAsync(string whereClause);
    // Task<List<T>> FindByAsync(string columnName, object value);
    // Task<List<T>> FindByAndAsync(Dictionary<string, object> conditions);
    // Task<List<T>> FindByOrAsync(Dictionary<string, object> conditions);

}