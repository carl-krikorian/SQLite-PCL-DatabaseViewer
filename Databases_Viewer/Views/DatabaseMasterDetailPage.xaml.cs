using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Databases_Viewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DatabaseMasterDetailPage : ContentPage
    {
        public DatabaseMasterDetailPage()
        {
            InitializeComponent();
        }

        /*private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(searchBar.Text))
            {
                
            }
        }*/
    }
}