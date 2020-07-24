using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Databases_Viewer.Models.Repository;
using Databases_Viewer.Models.Repository.Interfaces;
using GalaSoft.MvvmLight;
using SQLite;
using Syncfusion.Data.Extensions;
using Xamarin.Forms;

namespace Databases_Viewer.Models
{
    public class GenericDatabase: INotifyPropertyChanged
    {
        public SQLiteAsyncConnection _database;
        public static string DBPath;
        public UnitOfWork uow;
        public ObservableCollection<Object> lastObservedList;
        private string lastSelectQuery;
        private ObservableCollection<TableName> listOfTables ;
        public ObservableCollection<TableName> ListOfTables
        {
            get
            {
                return listOfTables;
            }
            set
            {
                listOfTables = value;
                NotifyPropertyChanged(nameof(ListOfTables));
            }
        }

        public static object ExceptionHandlers { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static bool CreateAllDBTables()
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                Debug.WriteLine("DB Table started: " + stopwatch.Elapsed.ToString());

                using (SQLiteConnection conn = new SQLiteConnection(DBPath))
                {
                    var type = typeof(BaseEntity);
                    var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetTypes())
                        .Where(p => type.IsAssignableFrom(p));
                    var entites = types.Where(t => t.FullName.Contains(".Entities")).ToList();
                    var tasks = new List<Task>();
                    foreach (var entityType in entites)
                    {
                        if (!conn.TableMappings.Any(x => x.MappedType == entityType))
                        {
                            conn.CreateTable(entityType, CreateFlags.None);
                        }
                    }
                    Debug.WriteLine("DB Table Creation: " + stopwatch.Elapsed.ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                return false;
            }
        }
        public GenericDatabase(string dbPath)
        {
            DBPath = dbPath;
            _database = new SQLiteAsyncConnection(dbPath);
            CreateAllDBTables();
            ListOfTables = PopulateTableListWithRowCount();
        }
        public ObservableCollection<TableName> ListOfTablesGetter()
        {
            return ListOfTables;
        }
        public async Task<List<TableName>> GetAllTablesAsync()
        {
            string queryString = $"SELECT name FROM sqlite_master WHERE type = 'table'";
            return await _database.QueryAsync<TableName>(queryString).ConfigureAwait(false);
        }
        public ObservableCollection<TableName> PopulateTableListWithRowCount()
        {
            List<TableName> temporaryList = this.GetAllTablesAsync().Result;
            temporaryList.RemoveAll(r => r.Name == "sqlite_sequence");
            using (SQLiteConnection _conn = new SQLiteConnection(DBPath))
            {
                foreach (TableName tableName in temporaryList)
                {
                    tableName.Count = _conn.ExecuteScalar<int>("SELECT COUNT(*) FROM " + tableName.Name);
                }
            }
            return new ObservableCollection<TableName>(temporaryList);
        }
        public bool UpdateSpecificTableCount(string tableName)
        {
            using (SQLiteConnection _conn = new SQLiteConnection(DBPath))
            {
                for(int i=0; i < ListOfTables.Count; i++)
                {
                    if ( ListOfTables[i].Name == tableName)
                    {
                        ListOfTables[i].Count = _conn.ExecuteScalar<int>("SELECT COUNT(*) FROM " + tableName);
                        return true;
                    }
                }
                return false;
            }
        }
        public void printTableListInfo()
        {
            Debug.WriteLine("Printing Table counts!!");
            foreach (TableName t in ListOfTables)
            {
                Debug.WriteLine(t.Count);
            }
        }
        public List<Item> GetItemList()
        {
            var r = _database.Table<Item>().ToListAsync();
            r.Wait();
            return r.Result;
        }
        //parses the input String to see which operation and calls the appropriate method
        public bool QueryDatabase(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;
            try
            {
                var firstWord = Regex.Replace(query.Split()[0], @"[^0-9a-zA-Z\ ]+", "");
                string treatedQuery = CleanQuery(query);
                string[] split = treatedQuery.Split(' ');
                if (firstWord.ToLower() == "select")
                {
                    return WhenQuerySelect(query,split);
                }
                else if (firstWord.ToLower() == "update")
                {
                    return WhenQueryUpdate(query, split);
                }
                else if (firstWord.ToLower() == "insert")
                {
                    return WhenQueryInsert(query, split);
                }
                else if (firstWord.ToLower() == "delete")
                {
                    WhenQueryDelete(query, split);
                }

                    return false;
            }
            catch (Exception e)
            {
                App.Current.MainPage.DisplayAlert("Exception", e.ToString(), "Ok");
                return false;
            }
        }
        public bool WhenQuerySelect(string query, string [] splitQuery)
        {
            int TableIndex;
            for (TableIndex = 0; TableIndex < splitQuery.Length; TableIndex++)
                if (splitQuery[TableIndex] == "from")
                    break;
            string tableName = splitQuery[TableIndex + 1];
            Type TableType = Type.GetType("Databases_Viewer.Models." + tableName);
            using (SQLiteConnection conn = new SQLiteConnection(DBPath))
            {
                var map = conn.GetMapping(TableType);
                lastObservedList = new ObservableCollection<Object>(conn.Query(map, query));
                lastSelectQuery = query;
                return true;
            }
        }
        public bool WhenQueryUpdate(string query, string[] splitQuery)
        {
            int modifiedRows;
            using (SQLiteConnection conn = new SQLiteConnection(DBPath))
            {
                modifiedRows = conn.Execute(query);
            }
            QueryDatabase(lastSelectQuery);
            App.Current.MainPage.DisplayAlert("Update Successful", "Number of rows affected = " + modifiedRows, "OK");
            Debug.WriteLine("Update TableName is " + splitQuery[1]);
            PopulateFromLastQuery(splitQuery[1]);
            return true;
        }
        public bool WhenQueryInsert(string query, string[] splitQuery)
        {
            int TableIndex;
            for (TableIndex = 0; TableIndex < splitQuery.Length; TableIndex++)
                if (splitQuery[TableIndex] == "into")
                    break;
            string tableString = splitQuery[TableIndex + 1];
            Debug.WriteLine("the table being changed is " + tableString);
            int modifiedRows;
            using (SQLiteConnection conn = new SQLiteConnection(DBPath))
            {
                modifiedRows = conn.Execute(query);
            }
            UpdateSpecificTableCount(tableString);
            PopulateFromLastQuery(tableString);
            App.Current.MainPage.DisplayAlert("Insert Successful", "Number of rows affected = " + modifiedRows, "OK");
            return true;
        }
        public bool WhenQueryDelete(string query, string[] splitQuery)
        {
            int TableIndex;
            for (TableIndex = 0; TableIndex < splitQuery.Length; TableIndex++)
                if (splitQuery[TableIndex] == "from")
                    break;
            string tableName = splitQuery[TableIndex + 1];
            Debug.WriteLine("the table being changed is " + tableName);
            int modifiedRows;
            using (SQLiteConnection conn = new SQLiteConnection(DBPath))
            {
                modifiedRows = conn.Execute(query);
            }
            UpdateSpecificTableCount(tableName);
            PopulateFromLastQuery(tableName);
            App.Current.MainPage.DisplayAlert("Delete Successful", "Number of rows affected = " + modifiedRows, "OK");
            return true;
        }
        private string CleanQuery(string query)
        {
            string treatedQuery = Regex.Replace(query, @"[^a-zA-Z]", " ");
            treatedQuery = Regex.Replace(treatedQuery, @"(\n?)[^\S\n]+(\n?)", m =>
            !string.IsNullOrEmpty(m.Groups[1].Value) || !string.IsNullOrEmpty(m.Groups[2].Value) // If any \n matched
                ? $"{m.Groups[1].Value}{m.Groups[2].Value}" // Concat Group 1 and 2 values
                : " ");  // Else, replace the 1+ whitespaces matched with a space
            treatedQuery = Regex.Replace(treatedQuery, @"\n{3,}", "\n\n"); // Replace 3+ \ns with two \ns
            return treatedQuery;
        }
        private bool PopulateFromLastQuery(string tableName)
        {
            if (string.IsNullOrWhiteSpace(this.lastSelectQuery))
            {
                return QueryDatabase("select * from " + tableName);
            }
            else
                return QueryDatabase(lastSelectQuery);
        }
    }
}

public class TableName : ObservableObject
{
    public TableName() { }
    public TableName(string name) { Name = name; }
    public string Name { get; set; }
    public int Count { get; set; }
}