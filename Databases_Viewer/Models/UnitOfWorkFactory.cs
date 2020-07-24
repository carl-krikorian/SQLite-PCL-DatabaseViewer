
using Databases_Viewer.Models.Repository.Interfaces;

namespace Databases_Viewer.Models
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        string dbPath;
        public UnitOfWorkFactory(string dbPath)
        {

            this.dbPath = dbPath;
        }
        public IUnitOfWork Create()
        {
            return new UnitOfWork(dbPath);
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
