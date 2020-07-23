using Databases_Viewer.Views;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Databases_Viewer.ViewModels
{
    class DatabaseMasterDetailPageViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<TableName> TablesList = App.Database.ListOfTables;
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
        private TableName _selectedTable { get; set; }
        public TableName SelectedTable
        {
            get
            { return _selectedTable; }
            set
            {
                if (_selectedTable != value)
                {
                    _selectedTable = value;
                    //NotifyPropertyChanged(nameof(SelectedTable));
                    if (_selectedTable != null)
                        TableListView_ItemSelected(SelectedTable);
                }
            }
        }
        public async void TableListView_ItemSelected(TableName tableName)
        {
            //TableName t = new TableName();
           // t.name = "jeff";
            //Debug.WriteLine("Table name is " + tableName.name);
            await App.Current.MainPage.Navigation.PushAsync(new DatabasePage(tableName));
            _selectedTable = null;
            SelectedTable = null;
        }
        public ICommand PerformSearch => new Command<string>((string query) =>
        {
             DisplayedList = new ObservableCollection<TableName>( DisplayedList.Where(w => w.name.ToLower().Contains(Query.ToLower())).ToList());
        });
        public ICommand RefreshCommand => new Command(() => RefreshDisplayList());
        public ICommand TextChangeInSearchCommand => new Command(() => SearchInBlank());
        private void SearchInBlank()
        {

            if (string.IsNullOrWhiteSpace(Query))
            {
                DisplayedList = TablesList;
            }
            else
            {
                DisplayedList = new ObservableCollection<TableName>(DisplayedList.Where(w => w.name.ToLower().Contains(Query.ToLower())).ToList());
            }
        }
        private void RefreshDisplayList()
        {
            isBusy = true;
            DisplayedList = new ObservableCollection<TableName>(App.Database.ListOfTables);
            isBusy = false;
        }
    }
}
