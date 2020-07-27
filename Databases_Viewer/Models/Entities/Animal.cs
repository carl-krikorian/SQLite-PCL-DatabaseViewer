using System;
using System.Collections.Generic;
using System.Text;
using Databases_Viewer.Models.Repository.Interfaces;
using SQLite;

namespace Databases_Viewer.Models.Entities
{
    public class Animal: BaseEntity
    {
        public Animal() { }
        public Animal(string name, string imageLink, string description)
        {
            Name = name;
            ImageLink = imageLink;
            Description = description;
        }
        public string Name { get; set; }
        public string ImageLink { get; set; }
        public string Description { get; set; }
    }
}