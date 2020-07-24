using Syncfusion.SfDataGrid.XForms;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Databases_Viewer.Ressources
{
    public class GridTheme:DataGridStyle
    {

        public GridTheme()
        {
        }

        public override Color GetHeaderBackgroundColor()
        {
            return Color.FromHex("#9dd8ed");
        }
        public override Color GetHeaderBorderColor()
        {
            return Color.Black;
        }
    }

    
}
