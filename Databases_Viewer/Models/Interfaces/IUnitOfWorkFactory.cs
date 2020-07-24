using System;
using System.Collections.Generic;
using System.Text;

namespace Databases_Viewer.Models.Repository.Interfaces
{
    public interface IUnitOfWorkFactory: IDisposable
    {
        IUnitOfWork Create();
    }
}
