﻿using Databases_Viewer.Models.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Databases_Viewer.Models
{
    public class TestItem : BaseEntity
    {
        public TestItem() { }
        public TestItem(string field) { SomeField = field; }
        public string SomeField;
    }
}