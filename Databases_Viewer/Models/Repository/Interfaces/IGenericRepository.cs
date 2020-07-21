using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Databases_Viewer.Models.Repository.Interfaces
{
    public interface IGenericRepository<T> : IGetMaxDate where T : BaseEntity
    {
        Task<List<T>> AsQueryableAsync();
        Task<int> CountAsync();
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllAsync(string queryString);
        Task<int> InsertAsync(T item);
        Task<T> GetAsync(string idValue);
        Task<T> GetAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetAsync<TValue>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, TValue>> orderBy = null);
        Task<int> InsertOrReplaceAsync(T item);
        Task<int> DeleteAsync(T item);
        Task<int> DeleteAllAsync(List<T> items);
        Task<int> ExecuteAsync(string query, params object[] args);
        Task<decimal?> GetDecimalAsync(string query, params object[] args);
        Task<int?> GetIntAsync(string query, params object[] args);
        Task<int> InsertAllAsync(IEnumerable<T> items);
        Task<int> InsertOrReplaceAllAsync(IEnumerable<T> items);
        Task<int> UpdateAsync(T item);
        Task ClearTableAsync(string tableName);
        Task ClearTableAsync();
    }

    public interface IGetMaxDate
    {
        Task<DateTime> GetMaxStampDateAsync(string tableName);
    }
}
