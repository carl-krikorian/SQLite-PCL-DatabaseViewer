using Databases_Viewer.Models.Repository.Interfaces;
using System;

namespace Databases_Viewer.Models
{
    public class Item: BaseEntity
    {
        public Item(){ }
        public Item( string text)
        {
            Text = text;
        }
        public string Text { get; set; }
    }
}