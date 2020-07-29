using Databases_Viewer.Views;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Databases_Viewer.ViewModels
{
    class DatabaseMasterDetailPageViewModel: INotifyPropertyChanged
    {
        public DatabaseMasterDetailPageViewModel()
        {
            SelectedTablePushCommand = new Command<TableName>(SelectedTablePush);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<TableName> displayedList = App.Database.ListOfTables;
        public ObservableCollection<TableName> DisplayedList
        {
            get => displayedList;
            set
            {
                displayedList = value;
                NotifyPropertyChanged(nameof(DisplayedList));
            }
        }
        private bool isbusy = false;
        public bool isBusy 
        {
            get { return isbusy; }
            set
            {
                if (isbusy != value)
                    isbusy = value;
                NotifyPropertyChanged(nameof(isBusy));
            }
        }
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Query { get; set; }
        //for the search bar, it will filter the current Display List
        public ICommand PerformSearch => new Command<string>((string query) =>
        {
             DisplayedList = new ObservableCollection<TableName>( DisplayedList.Where(w => w.Name.ToLower().Contains(Query.ToLower())).ToList());
        });
        //Will refresh Display List by assigning it to App.Database.ListOfTables
        public ICommand RefreshCommand => new Command(() => RefreshDisplayList());
        //Will refresh the list or filter through it using Linq
        public ICommand TextChangeInSearchCommand => new Command(() => SearchInBlank());
        public Command<TableName> SelectedTablePushCommand { get; }
        private void SearchInBlank()
        {
            if (string.IsNullOrWhiteSpace(Query))
            {
                DisplayedList = App.Database.ListOfTables;
            }
            else
            {
                DisplayedList = new ObservableCollection<TableName>(DisplayedList.Where(w => w.Name.ToLower().Contains(Query.ToLower())).ToList());
            }
        }
        private void RefreshDisplayList()
        {
            isBusy = true;
            DisplayedList = new ObservableCollection<TableName>(App.Database.ListOfTables);
            isBusy = false;
        }
        public async void SelectedTablePush(TableName tableName)
        {
            //Will not load anything and display an alert if Table name does not have a corresponding Class in the project
            if (!FindClass(tableName.Name) || !App.Database.ListOfTables.Contains(tableName))
                await App.Current.MainPage.DisplayAlert("Database mismatch", "Lack of correspondance between database and classes", "Ok");
            //Otherwise it loads components and assigns binding Context
            else
            {
                await App.Current.MainPage.Navigation.PushAsync(new DatabasePage(tableName));
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
