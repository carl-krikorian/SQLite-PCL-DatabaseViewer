using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Databases_Viewer.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Databases_Viewer.ViewModels;
using System.Collections.ObjectModel;
using Microsoft.CSharp.RuntimeBinder;
using System.Windows.Input;

namespace Databases_Viewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DatabasePage : ContentPage
    {
        public DatabasePage()
        { InitializeComponent(); }
        public DatabasePage(TableName tableName)
        {
            if (!FindClass(tableName.Name) || !App.Database.ListOfTables.Contains(tableName))
                App.Current.MainPage.DisplayAlert("Database mismatch", "Lack of correspondance between database and classes", "Ok");
            else
            {
                InitializeComponent();
                Title = tableName.Name;
                DatabasePageViewModel databasePageViewModel = new DatabasePageViewModel(tableName);
                BindingContext = databasePageViewModel;
            }
        }
        public bool FindClass(string ClassName)
        {
            try
            {
                Item i = new Item();
                Type t = Type.GetType("Databases_Viewer.Models." + ClassName);
                if (t == null)
                    return false;
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return false;
            }
        }
    }
}