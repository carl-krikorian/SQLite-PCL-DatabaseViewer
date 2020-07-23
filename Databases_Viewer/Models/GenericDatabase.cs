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
using GalaSoft.MvvmLight;
using SQLite;
using Syncfusion.Data.Extensions;
using Xamarin.Forms;

namespace Databases_Viewer.Models
{
    public class GenericDatabase: INotifyPropertyChanged
    {
        public SQLiteAsyncConnection _database;
        public string DBPath;
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
        //PropertyChanged += (object sender, PropertyChangedEventArgs e) => { // logic goes here }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public GenericDatabase(string dbPath)
        {
            DBPath = dbPath;
            _database = new SQLiteAsyncConnection(dbPath);
            uow = new UnitOfWork(dbPath);
            _database.CreateTableAsync<Animal>();
            //_database.DropTableAsync<Animal>();
            //_database.InsertAsync(new Animal("test", "test Link", "Test Description")).Wait();
            _database.CreateTableAsync<Item>().Wait();
            _database.CreateTableAsync<TestItem>().Wait();
            ListOfTables = PopulateTableListWithRowCount();
            //_database.InsertAsync(new Item(1,"i1"));
            //_database.InsertAsync(new Item(2,"i2"));*/
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
            temporaryList.RemoveAll(r => r.name == "sqlite_sequence");
            using (SQLiteConnection _conn = new SQLiteConnection(DBPath))
            {
                foreach (TableName tableName in temporaryList)
                {
                    tableName.count = _conn.ExecuteScalar<int>("SELECT COUNT(*) FROM " + tableName.name);
                    //tableName.count = await (_database.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM "+ tableName.name));
                }
            }
            return new ObservableCollection<TableName>(temporaryList);
        }
        public bool UpdateSpecificTableCount(string tableName)
        {
            using (SQLiteConnection _conn = new SQLiteConnection(DBPath))
            {
                //Debug.WriteLine("Updating table " + tableName);
                for(int i=0; i < ListOfTables.Count; i++)
                {
                    if ( ListOfTables[i].name == tableName)
                    {
                        ListOfTables[i].count = _conn.ExecuteScalar<int>("SELECT COUNT(*) FROM " + tableName);
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
                Debug.WriteLine(t.count);
            }
        }
        public List<Item> GetItemList()
        {
            var r = _database.Table<Item>().ToListAsync();
            r.Wait();
            return r.Result;
        }
        public bool QueryDatabase(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;
            try
            {
                var firstWord = Regex.Replace(query.Split()[0], @"[^0-9a-zA-Z\ ]+", "");
                string treatedQuery = CleanQuery(query);
                Debug.WriteLine("treated Query is " + treatedQuery);
                string[] split = treatedQuery.Split(' ');
                //parses the input String to see which operation is being used as well as 
                if (firstWord.ToLower() == "select")
                {
                    return WhenQuerySelect(query,split);
                }
                else if (firstWord.ToLower() == "update")
                {
                    int modifiedRows;
                    using (SQLiteConnection conn = new SQLiteConnection(DBPath))
                    {
                        modifiedRows = conn.Execute(query);
                    }
                    QueryDatabase(lastSelectQuery);
                    App.Current.MainPage.DisplayAlert("Update Successful", "Number of rows affected = " + modifiedRows, "OK");
                    Debug.WriteLine("Update TableName is " + split[1]);
                    PopulateFromLastQuery(split[1]);
                    return true;
                }
                else if (firstWord.ToLower() == "insert")
                {
                    int TableIndex;
                    for (TableIndex = 0; TableIndex < split.Length; TableIndex++)
                        if (split[TableIndex] == "into")
                            break;
                    string tableString = split[TableIndex + 1];
                    Debug.WriteLine("the table being changed is " + tableString);
                    int modifiedRows;
                    using (SQLiteConnection conn = new SQLiteConnection(DBPath))
                    {
                        modifiedRows = conn.Execute(query);
                    }
                    UpdateSpecificTableCount(tableString);
                    printTableListInfo();
                    PopulateFromLastQuery(tableString);
                    App.Current.MainPage.DisplayAlert("Insert Successful", "Number of rows affected = " + modifiedRows, "OK");
                    return true;
                }
                else if (firstWord.ToLower() == "delete")
                {
                    int TableIndex;
                    for (TableIndex = 0; TableIndex < split.Length; TableIndex++)
                        if (split[TableIndex] == "from")
                            break;
                    string tableName = split[TableIndex + 1];
                    Debug.WriteLine("the table being changed is " + tableName);
                    int modifiedRows;
                    using (SQLiteConnection conn = new SQLiteConnection(DBPath))
                    {
                        modifiedRows = conn.Execute(query);
                    }
                    UpdateSpecificTableCount(tableName);
                    //printTableListInfo();
                    PopulateFromLastQuery(tableName);
                    App.Current.MainPage.DisplayAlert("Delete Successful", "Number of rows affected = " + modifiedRows, "OK");
                    return true;
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
            //Debug.WriteLine("tableName is " + tableName);
            Type TableType = Type.GetType("Databases_Viewer.Models." + tableName);
            //Debug.WriteLine("we found the type! " + TableType.FullName);
            using (SQLiteConnection conn = new SQLiteConnection(DBPath))
            {
                var map = conn.GetMapping(TableType);
                lastObservedList = new ObservableCollection<Object>(conn.Query(map, query));
                lastSelectQuery = query;
                return true;
            }
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
    public TableName(string Name) { name = Name; }
    public string name { get; set; }
    public int count { get; set; }
}