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

namespace Databases_Viewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DatabasePage : ContentPage
    {
        public DatabasePage()
        { InitializeComponent(); }
        public DatabasePage(TableName tableName)
        {
            if (!FindClass(tableName.name) || !App.Database.ListOfTables.Contains(tableName))
                App.Current.MainPage.DisplayAlert("Database mismatch", "Lack of correspondance between database and classes", "Ok");
            else
            {
                currentTable = tableName;
                InitializeComponent();
                
            }
        }

        private TableName currentTable;

        protected override async void OnAppearing()
        {
            DataGrid.ItemsSource = await InvokeDatabasePageViewModelAsync(Activator.CreateInstance(Type.GetType("Databases_Viewer.Models." + currentTable.name)));
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

        async Task<object> InvokeDatabasePageViewModelAsync(object o)
        {
            Type genericType = typeof(DatabasePageViewModel<>).MakeGenericType(new Type[] { o.GetType() });
            var genericInstance = Activator.CreateInstance(genericType);
            var task = (Task)genericType.GetMethod("ReturnDisplayListAsync").Invoke(genericInstance, null);
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }

    }
}