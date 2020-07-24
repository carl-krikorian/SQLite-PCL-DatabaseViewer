using SQLite;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Databases_Viewer.Models.Repository.Interfaces;


namespace Databases_Viewer.Models
{
    public class UnitOfWork : IUnitOfWork
    {
        static string _databasePath;
        static readonly Lazy<SQLiteAsyncConnection> _databaseAsyncConnectionHolder =
           new Lazy<SQLiteAsyncConnection>(() =>
           {
               var conn = new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
               conn.EnableWriteAheadLoggingAsync();
               return conn;
           });
        static readonly Lazy<SQLiteConnection> _databaseConnectionHolder =
          new Lazy<SQLiteConnection>(() =>
          {
              var conn = new SQLiteConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
              conn.EnableWriteAheadLogging();
              return conn;
          });
        static SQLiteAsyncConnection DatabaseAsyncConnection => _databaseAsyncConnectionHolder.Value;
        public SQLiteAsyncConnection GetAsyncConnection<T>() where T : BaseEntity
        {
            Task.Run(async () =>
            {
                if (!DatabaseAsyncConnection.TableMappings.Any(x => x.MappedType == typeof(T)))
                {
                    await DatabaseAsyncConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
                }
            });
            return DatabaseAsyncConnection;
        }
        static SQLiteConnection DatabaseConnection => _databaseConnectionHolder.Value;
        public SQLiteConnection GetConnection<T>() where T : BaseEntity
        {
            Task.Run(() =>
           {
               if (!DatabaseConnection.TableMappings.Any(x => x.MappedType == typeof(T)))
               {
                   DatabaseConnection.CreateTables(CreateFlags.None, typeof(T));
               }
           });
            return DatabaseConnection;
        }
        public UnitOfWork(string dbPath)
        {
            _databasePath = dbPath;
            BeginTransaction();
        }
        public void Commit()
        {
            //if (DatabaseConnection.GetConnection().IsInTransaction)
            //    DatabaseConnection.GetConnection().Commit();
        }
        public void Rollback()
        {
            //if (DatabaseConnection.GetConnection().IsInTransaction)
            //    DatabaseConnection.GetConnection().Rollback();
        }
        private void BeginTransaction()
        {
            //if (DatabaseConnection.GetConnection().IsInTransaction)
            //    DatabaseConnection.GetConnection().BeginTransaction();
        }
        public void FinalDispose()
        {
            if (DatabaseConnection != null)
            {
                Commit();
            }
        }
        public void Dispose()
        {
            Commit();
        }
    }
}
