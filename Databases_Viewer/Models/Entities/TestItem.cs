using Databases_Viewer.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Databases_Viewer.Models.Entities
{
    public class TestItem : BaseEntity
    {
        public TestItem() { }
        public TestItem(int field) { SomeIntegerField = field; }
        public int SomeIntegerField { get; set; }
    }
}
