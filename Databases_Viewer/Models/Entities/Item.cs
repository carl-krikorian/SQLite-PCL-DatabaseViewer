using Databases_Viewer.Models.Interfaces;
using System;

namespace Databases_Viewer.Models.Entities
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