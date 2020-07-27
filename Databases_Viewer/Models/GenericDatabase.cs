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
        /// <summary>
        /// Constructor that uses the dbPath to create a connection to that database and then creates all tables if not already exists using create All Table
        /// Finally it populates all of these tables using PopulateTableListWithRowCount
        /// </summary>
        /// <param name="dbPath">a string that disctates path to the Database</param>
        public GenericDatabase(string dbPath)
        {
            DBPath = dbPath;
            _database = new SQLiteAsyncConnection(dbPath);
            CreateAllDBTables();
            ListOfTables = PopulateTableListWithRowCount();
        }
        public SQLiteAsyncConnection _database;
        public static string DBPath;
        //last Query is used to display the previous select query in case of an update/delete or insert to see all updates
        private string lastSelectQuery;
        //The last list of objects returned from the select Query
        public ObservableCollection<Object> lastObservedList;
        //The property for the List of TablName objects
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
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// creates tables in the SQLite database that correspond to the items with Enitity Base in the project 
        /// </summary>
        /// <returns>Returns true if successful false if an exception is thrown </returns>
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
        //Constructor that creates all Tables and then populates the List of tableName objects with their names and counts
        /// <summary>
        /// By querying the table SQLITE MASTER we can get all table names.
        /// </summary>
        /// <returns>A list of TableName objects with only the Name property assigned</returns>
        public async Task<List<TableName>> GetAllTablesAsync()
        {
            string queryString = $"SELECT name FROM sqlite_master WHERE type = 'table'";
            return await _database.QueryAsync<TableName>(queryString).ConfigureAwait(false);
        }
        /// <summary>
        /// Uses GetAllTablesAsync() and assigns to each table a count using an SQL Query 
        /// </summary>
        /// <returns>A List of TableNames is returned and used in the other content pages </returns>
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
        /// <summary>
        /// Takes the TableName that needs updatin and calls the Query that it will find its count
        /// </summary>
        /// <param name="tableName">A string with the TableName</param>
        /// <returns>A bool if the query to find the table count is successful it returns true, otherwise it returns false </returns>
        public bool UpdateSpecificTableCount(string tableName)
        {
            try
            {
                using (SQLiteConnection _conn = new SQLiteConnection(DBPath))
                {
                    for (int i = 0; i < ListOfTables.Count; i++)
                    {
                        if (ListOfTables[i].Name == tableName)
                        {
                            ListOfTables[i].Count = _conn.ExecuteScalar<int>("SELECT COUNT(*) FROM " + tableName);
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch(Exception ex)
            {
                App.Current.MainPage.DisplayAlert("Updating table Count Error", ex.ToString(), "OK");
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
        /// <summary>
        /// Uses CleanQuery() on and parses the input String to see which operation is being used and calls the appropriate WhenQuery method to deal with it.
        /// For example: the first word is select and so it calls WhenQuerySelect
        /// </summary>
        /// <param name="query">An SQLDatabase Query</param>
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
        /// <summary>
        /// each WhenQuery function finds the tableName by looping through the splitQuery string. From said tableName it gets a corresponding type from the project
        /// with its map to execute the query using _SQLConnection.Query(TableMapping , string). Check the functions for the specific differences
        /// </summary>
        /// <param name="query"></param>
        /// <param name="splitQuery"></param>
        /// <returns>Returns true if it works and false if an exception is thrown</returns>
        public bool WhenQuerySelect(string query, string [] splitQuery)
        {
            int TableIndex;
            for (TableIndex = 0; TableIndex < splitQuery.Length; TableIndex++)
                if (splitQuery[TableIndex] == "from")
                    break;
            string tableName = splitQuery[TableIndex + 1];
            //Type TableType = Type.GetType("Databases_Viewer.Models." + tableName);
            Type TableType = GetTypeFromAllAssemblies(tableName);
            using (SQLiteConnection conn = new SQLiteConnection(DBPath))
            {
                var map = conn.GetMapping(TableType);
                lastObservedList = new ObservableCollection<Object>(conn.Query(map, query));
                //Assigns lastSelectQuery after doing the query, so that it is assigned after we are certain there were no exceptions
                lastSelectQuery = query;
                return true;
            }
        }
        /// <summary>
        /// The following three last WhenQuery after executing the query and displaying rows affected will call PopulateFromLastQuery() 
        /// to display the lastQuery after changes have been made
        /// </summary>
        /// <param name="query">An SQL Database Query</param>
        /// <param name="splitQuery">A cleaned and split array of SQL Database Query</param>
        /// <returns>Returns true if it works and false if an exception is thrown</returns>
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
        /// <summary>
        /// Will replace every non alphabeical Character with a space then all successive duplicate spaces are removed
        /// </summary>
        /// <param name="query">An SQL Database Query</param>
        /// <returns>A String</returns>
        /// <example>"select * from table" becomes "select from table"</example>
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
        /// <summary>
        /// Will use QueryDatabase(lastquery) to display previous select statement. Or if none lastSelectStatement is null 
        /// it will just get all the information from the table using the tableName 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>Returns true if it works and false if an exception is thrown</returns>
        private bool PopulateFromLastQuery(string tableName)
        {
            if (string.IsNullOrWhiteSpace(this.lastSelectQuery))
            {
                return QueryDatabase("select * from " + tableName);
            }
            else
                return QueryDatabase(lastSelectQuery);
        }
        /// <summary>
        /// Checks All the assemblies for those with base entity and finds the objects types with their namespace
        /// It then removes assembly references from the fullnames and compares to the tableName to assign the type to it
        /// </summary>
        /// <param name="ClassName"></param>
        /// <returns>the type with the same name as the class name inputed</returns>
        public Type GetTypeFromAllAssemblies(string ClassName)
        {
            Type temp = null;
            var type = typeof(BaseEntity);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
            var entities = types.Where(t => t.FullName.Contains(ClassName)).ToList();
            //loops through all the types in the entities list
            for (int Ientities = 0; Ientities < entities.Count; Ientities++)
            {
                string removedAssembliesName = Regex.Replace(entities[Ientities].FullName, @"[.\w]+\.(\w+)", "$1");
                if (removedAssembliesName == ClassName)
                {
                    temp = entities[Ientities];
                }   
            }
            return temp;
        }
    }
}
/// <summary>
///Observable Object used to keep track of Tables in Database
/// </summary>
public class TableName : ObservableObject
{
    public TableName() { }
    public TableName(string name) { Name = name; }
    public string Name { get; set; }
    public int Count { get; set; }
}