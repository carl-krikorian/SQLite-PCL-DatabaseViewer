using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace Databases_Viewer.Models.Interfaces
{
    public abstract class BaseEntity : ObservableObject
    {
        [PrimaryKey]
        public string ID { get; set; } = Guid.NewGuid().ToString();
        //public int IsActive { get; set; } = 1;
        //[CompareIgnore]
        //public DateTime StampDate { get; set; }
    }
}
