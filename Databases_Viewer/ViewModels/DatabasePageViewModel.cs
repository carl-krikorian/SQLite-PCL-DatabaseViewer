using Databases_Viewer.Models;
using Databases_Viewer.Models.Repository.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Databases_Viewer.ViewModels
{
    public class DatabasePageViewModel<T>: INotifyPropertyChanged where T :BaseEntity, new() 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private readonly SQLiteRepository<T> Repo;
        public DatabasePageViewModel()
        {
            Repo = new SQLiteRepository<T>(App.Database.uow);
            ExecuteInputCommand = new Command(DBExecuteInput);
        }
        private ObservableCollection<T> displayList;
        public ObservableCollection<T> DisplayList
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
        public async Task<ObservableCollection<T>> ReturnDisplayListAsync()
        {
            List<T> temporaryList = await Repo.GetAllAsync();
            DisplayList = new ObservableCollection<T>(temporaryList);
            return DisplayList;
        }
        public string EntryString { get; set; }
        public ICommand ExecuteInputCommand { get; } 
        public async void DBExecuteInput()
        {
            Debug.WriteLine("start of command");
            App.Database.QueryDatabase(EntryString); //"insert into Item(ID, Text) VALUES ('createdID3','TestText3')"
            await this.ReturnDisplayListAsync();
            Debug.WriteLine("end of command");
        }
    }
}
