using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Databases_Viewer.ViewModels;
using System.Runtime.CompilerServices;

namespace Databases_Viewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DatabasePage : ContentPage
    {
        public DatabasePage(TableName tableName)
        {
            InitializeComponent();
            DatabasePageViewModel databasePageViewModel = new DatabasePageViewModel(tableName);
            BindingContext = databasePageViewModel;
        }
    }
}