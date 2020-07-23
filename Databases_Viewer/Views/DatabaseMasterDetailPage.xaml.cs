using Databases_Viewer.ViewModels;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Databases_Viewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DatabaseMasterDetailPage : ContentPage
    {
        ViewCell givenCell;
        public DatabaseMasterDetailPage()
        {
            InitializeComponent();
          
        }
        protected override void OnAppearing()
        {
            //TableListView.ItemsSource = null;
            //databaseMasterDetailPageViewModel.DisplayedList = App.Database.ListOfTables;
            //TableListView.ItemsSource = App.Database.ListOfTables;
        }

        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            if (givenCell != null)
                givenCell.View.BackgroundColor = Color.Transparent;
            var viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.Transparent;
                givenCell = viewCell;
            }
        }

        private void TableListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}