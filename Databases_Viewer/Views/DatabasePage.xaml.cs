using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Databases_Viewer.ViewModels;


namespace Databases_Viewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DatabasePage : ContentPage
    {
        public DatabasePage(TableName tableName)
        {
            //Will not load anything and display an alert if Table name does not have a corresponding Class in the project
            if (!FindClass(tableName.Name) || !App.Database.ListOfTables.Contains(tableName))
                App.Current.MainPage.DisplayAlert("Database mismatch", "Lack of correspondance between database and classes", "Ok");
            //Otherwise it loads components and assigns binding Context
            else
            {
                InitializeComponent();
                Title = tableName.Name;
                DatabasePageViewModel databasePageViewModel = new DatabasePageViewModel(tableName);
                BindingContext = databasePageViewModel;
            }
        }
        /// <summary>
        /// Checks All the assemblies for those with base entity and finds the objects types with their namespace
        /// It then removes assembly references and compares to the tableName with the string without assembly references and returns false at the end if not found
        /// </summary>
        /// <param name="ClassName"></param>
        /// <returns>true if it can properly call an instance of it or false otherwise</returns>
        public bool FindClass(string ClassName)
        {
            try
            {
                Type temp = App.Database.GetTypeFromAllAssemblies(ClassName);
                //Type t = Type.GetType("Databases_Viewer.Models." + ClassName);
                if (temp == null)
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