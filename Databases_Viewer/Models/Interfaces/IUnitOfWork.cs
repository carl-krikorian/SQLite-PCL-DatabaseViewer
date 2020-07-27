using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Threading.Tasks;
namespace Databases_Viewer.Models.Interfaces
{
    public interface IUnitOfWork
    {
        SQLiteConnection GetConnection<T>() where T : BaseEntity;
        SQLiteAsyncConnection GetAsyncConnection<T>() where T : BaseEntity;
        void Commit();
        void Rollback();
    }
}
