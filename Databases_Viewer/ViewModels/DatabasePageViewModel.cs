using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Databases_Viewer.ViewModels
{
    public class DatabasePageViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public DatabasePageViewModel(TableName tableName)     
        {
            currentTable = tableName;
            App.Database.QueryDatabase("select * from " + currentTable.Name);
            DisplayList = App.Database.lastObservedList;
        }
        private TableName currentTable;
        private ObservableCollection<Object> displayList;
        public ObservableCollection<Object> DisplayList
        {
            get
            {
                return displayList;
            }
            set
            {
                displayList = value;
                NotifyPropertyChanged(nameof(DisplayList));
            }
        }
        public string EntryString { get; set; }
        public ICommand ExecuteInputCommand => new Command(() => DBExecuteInput());
        public void DBExecuteInput()
        {
            App.Database.QueryDatabase(EntryString); //"insert into Item(ID, Text) VALUES ('createdID3','TestText3')"
            ReturnNewLastObservedList();
        }
        public ObservableCollection<Object> ReturnNewLastObservedList()
        {
            DisplayList = App.Database.lastObservedList;
            return DisplayList;
        }
    }
}
