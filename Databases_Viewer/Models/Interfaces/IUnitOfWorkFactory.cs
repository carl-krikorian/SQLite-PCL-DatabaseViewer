using System;
using System.Collections.Generic;
using System.Text;

namespace Databases_Viewer.Models.Interfaces
{ 
    public interface IUnitOfWorkFactory: IDisposable
    {
        IUnitOfWork Create();
    }
}
