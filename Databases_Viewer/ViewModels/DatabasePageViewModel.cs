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
        }
        public ObservableCollection<T> DisplayList ;
        public async Task<ObservableCollection<T>> ReturnDisplayListAsync()
        {
            List<T> temporaryList = await Repo.GetAllAsync();
            DisplayList = new ObservableCollection<T>(temporaryList);
            Debug.WriteLine("JAMCOOOOO");
            foreach (T entity in temporaryList)
                Debug.WriteLine(entity.ID);
            return DisplayList;
        }
    }
}
