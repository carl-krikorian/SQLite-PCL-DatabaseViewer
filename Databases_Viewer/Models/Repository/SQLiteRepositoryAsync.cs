using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SQLite;
using System.Globalization;
using System.Threading.Tasks;
using Databases_Viewer.Models.Repository.Interfaces;
using Polly;

namespace Databases_Viewer.Models
{
    public class SQLiteRepositoryAsync<T> : IGenericRepository<T> where T : BaseEntity, new()
    {
        IUnitOfWork unitOfWork;
        int numRetries;
        SQLiteAsyncConnection db;
        public SQLiteRepositoryAsync(IUnitOfWork unitOfWork, int numRetries = 12)
        {
            this.unitOfWork = unitOfWork;
            this.numRetries = numRetries;
            db = this.unitOfWork.GetAsyncConnection<T>();
        }
        static TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromMilliseconds(Math.Pow(2, attemptNumber));
        public async Task<List<T>> AsQueryableAsync()
        {
            return await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
                ExecuteAsync(async () => await db.Table<T>().ToListAsync()).ContinueWith((t) =>
                {
                    if (t.IsFaulted) throw t.Exception;
                    return t.Result;
                });
        }
        public async Task<List<T>> GetAsync<TValue>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, TValue>> orderBy = null)
        {
            var query = db.Table<T>();
            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = query.OrderBy<TValue>(orderBy);
            return await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
                ExecuteAsync(async () => await query.ToListAsync()).ContinueWith((t) =>
                {
                    if (t.IsFaulted) throw t.Exception;
                    return t.Result;
                });
        }
        public async Task<T> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
                ExecuteAsync(async () => await db.FindAsync<T>(predicate)).ContinueWith((t) =>
                {
                    if (t.IsFaulted) throw t.Exception;
                    return t.Result;
                });
        }
        public async Task<int> InsertAsync(T entity)
        {
            return await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
                ExecuteAsync(async () => await db.InsertAsync(entity)).ContinueWith((t) =>
                {
                    if (t.IsFaulted) throw t.Exception;
                    return t.Result;
                });
        }
        public async Task<int> UpdateAsync(T entity)
        {
            return await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
                ExecuteAsync(async () => await db.UpdateAsync(entity)).ContinueWith((t) =>
                {
                    if (t.IsFaulted) throw t.Exception;
                    return t.Result;
                });
        }
        public async Task<int> CountAsync()
        {
            return await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
              ExecuteAsync(async () => await db.Table<T>().CountAsync()).ContinueWith((t) =>
              {
                  if (t.IsFaulted) throw t.Exception;
                  return t.Result;
              });
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
           ExecuteAsync(async () => await db.Table<T>().ToListAsync()).ContinueWith((t) =>
           {
               if (t.IsFaulted) throw t.Exception;
               return t.Result;
           });
        }
        public async Task<List<T>> GetAllAsync(string queryString)
        {
            return await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
           ExecuteAsync(async () => await db.QueryAsync<T>(queryString)).ContinueWith((t) =>
           {
               if (t.IsFaulted) throw t.Exception;
               return t.Result;
           });
        }
        public async Task<T> GetAsync(string idValue)
        {
            var query = String.Format("SELECT * FROM {0} WHERE {1} = '{2}'", GetTableName(), GetPrimaryKeys().First(), idValue);
            return (await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
              ExecuteAsync(async () => await db.QueryAsync<T>(query))).FirstOrDefault();
        }
        public async Task<int> InsertOrReplaceAsync(T item)
        {
            //return Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
            //          ExecuteAsync(async () => await db.InsertOrReplaceAsync(item));
            return await Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).
                      ExecuteAsync(async () => await db.InsertOrReplaceAsync(item)).ContinueWith((t) =>
                        {
                            if (t.IsFaulted) throw t.Exception;
                            return t.Result;
                        });
            //return db.InsertOrReplaceAsync(item).ContinueWith((t) => {
            //    if (t.IsFaulted) throw t.Exception;
            //    return t.Result;
            //});
        }
        public async Task<int> InsertOrReplaceAllAsync(IEnumerable<T> items)
        {
            int rowsAffected = 0;
            foreach (var item in items)
                rowsAffected += await InsertOrReplaceAsync(item).ContinueWith((t) =>
                {
                    if (t.IsFaulted) throw t.Exception;
                    return t.Result;
                }); ;
            return await Task.FromResult(rowsAffected);
        }
        public async Task<int> DeleteAsync(T entity)
        {
            return await db.DeleteAsync(entity).ContinueWith((t) =>
            {
                if (t.IsFaulted) throw t.Exception;
                return t.Result;
            }); ;
        }
        public async Task<int> DeleteAllAsync(List<T> items)
        {
            int rowsAffected = 0;
            foreach (var item in items)
                rowsAffected += await DeleteAsync(item).ContinueWith((t) =>
                {
                    if (t.IsFaulted) throw t.Exception;
                    return t.Result;
                }); ;
            return await Task.FromResult(rowsAffected);
        }
        public async Task<int> InsertAllAsync(IEnumerable<T> items)
        {
            return await db.InsertAllAsync(items).ContinueWith((t) =>
                       {
                           if (t.IsFaulted) throw t.Exception;
                           return t.Result;
                       }); ;
        }
        public async Task<int> UpdateAllAsync(IEnumerable<T> items)
        {
            int rowsAffected = 0;
            foreach (var item in items)
            {
                rowsAffected += await db.InsertOrReplaceAsync(item).ContinueWith((t) =>
                                    {
                                        if (t.IsFaulted) throw t.Exception;
                                        return t.Result;
                                    }); ;
            }
            return await Task.FromResult(rowsAffected);
        }
        public async Task ClearTableAsync(string tableName)
        {
            var query = String.Format("DELETE FROM {0}", tableName);
            await db.ExecuteAsync(query).ContinueWith((t) =>
                        {
                            if (t.IsFaulted) throw t.Exception;
                            return t.Result;
                        }); ;
        }
        public async Task ClearTableAsync()
        {
            var query = String.Format("DELETE FROM {0}", GetTableName());
            await db.ExecuteAsync(query).ContinueWith((t) =>
                        {
                            if (t.IsFaulted) throw t.Exception;
                            return t.Result;
                        }); ;
        }
        public async Task CreateTable()
        {
            await db.CreateTableAsync<T>().ContinueWith((t) =>
            {
                if (t.IsFaulted) throw t.Exception;
                return t.Result;
            }); ;
        }
        public async Task DropTable()
        {
            await db.DropTableAsync<T>().ContinueWith((t) =>
            {
                if (t.IsFaulted) throw t.Exception;
                return t.Result;
            }); ;
        }
        private string GetTableName()
        {
            var attributes = typeof(T).GetTypeInfo().GetCustomAttributes(false);
            var columnMapping = attributes.FirstOrDefault(m => m.GetType() == typeof(TableAttribute));
            if (columnMapping != null)
            {
                var mapsTo = (TableAttribute)columnMapping;
                return mapsTo.Name;
            }
            return typeof(T).Name;
        }
        private IEnumerable<PropertyInfo> GetPrimaryKeys()
        {
            var primaryKeysProperty =
                typeof(T).GetTypeInfo()
                    .DeclaredProperties.Where(
                        m => m.GetCustomAttributes(false).Any(at => at.GetType() == typeof(PrimaryKeyAttribute)));

            return primaryKeysProperty;
        }
        public async Task<int> ExecuteAsync(string query, params object[] args)
        {
           return await db.ExecuteAsync(query, args).ContinueWith((t) =>
                        {
                            if (t.IsFaulted) throw t.Exception;
                            return t.Result;
                        }); ;
        }
        public async Task<DateTime> GetMaxStampDateAsync(string tableName)
        {
            DateTime maxStampDate = DateTime.ParseExact("01/01/2001 00:00:00", "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            try
            {
                var _query = $@"SELECT * from {tableName} Order By StampDate Desc limit 1";
                var result = await GetAllAsync(_query);
                if (result == null || result.Count() == 0)
                    return DateTime.ParseExact("01/01/2001 00:00:00", "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                DateTime dateStampDate = (DateTime)result.FirstOrDefault().GetType().GetProperty("StampDate").GetValue(result.FirstOrDefault());
                dateStampDate = dateStampDate.AddMinutes(1);
                try
                {
                    maxStampDate = DateTime.ParseExact(dateStampDate.ToString("MM/dd/yyyy HH:mm:ss"), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                }
                catch
                {
                    return maxStampDate;
                }

                return maxStampDate;
            }
            catch
            {
                return maxStampDate;
            }
        }
        private Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
        public async Task<decimal?> GetDecimalAsync(string query, params object[] args)
        {
           return await db.ExecuteScalarAsync<decimal?>(query, args).ContinueWith((t) =>
                   {
                       if (t.IsFaulted) throw t.Exception;
                       return t.Result;
                   }); ;
        }
        public async Task<int?> GetIntAsync(string query, params object[] args)
        {
           return await db.ExecuteScalarAsync<int?>(query, args).ContinueWith((t) =>
                   {
                       if (t.IsFaulted) throw t.Exception;
                       return t.Result;
                   }); ;
        }
        public async Task<bool> GetBoolAsync(string query, params object[] args)
        {
           return await db.ExecuteScalarAsync<bool>(query, args).ContinueWith((t) =>
            {
                if (t.IsFaulted) throw t.Exception;
                return t.Result;
            });
        }
    }
}
