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
        DatabaseMasterDetailPageViewModel databaseMasterDetailPageViewModel;
        public DatabaseMasterDetailPage()
        {
            InitializeComponent();
            /*databaseMasterDetailPageViewModel = new DatabaseMasterDetailPageViewModel();
            BindingContext = databaseMasterDetailPageViewModel;*/
        }
        protected override void OnAppearing()
        {
            //TableListView.ItemsSource = null;
            //databaseMasterDetailPageViewModel.DisplayedList = App.Database.ListOfTables;
            //TableListView.ItemsSource = App.Database.ListOfTables;
        }
    }
}