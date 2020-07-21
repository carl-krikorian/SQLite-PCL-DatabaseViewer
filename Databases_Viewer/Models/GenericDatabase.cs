using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Databases_Viewer.Models.Repository;
using SQLite;
using Syncfusion.Data.Extensions;

namespace Databases_Viewer.Models
{
    public class GenericDatabase
    {
        public SQLiteAsyncConnection _database;
        public string DBPath;
        public UnitOfWork uow;
        public List<TableName> ListOfTables;
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
        public async Task<List<TableName>> GetAllTablesAsync()
        {
            string queryString = $"SELECT name FROM sqlite_master WHERE type = 'table'";
            return await _database.QueryAsync<TableName>(queryString).ConfigureAwait(false);
        }
        public List<TableName> PopulateTableListWithRowCount()
        {
            List<TableName> temporaryList = this.GetAllTablesAsync().Result;
            temporaryList.RemoveAll(r => r.name == "sqlite_sequence");
            foreach(TableName tableName in temporaryList)
            {
                SQLiteConnection _conn = new SQLiteConnection(DBPath);
                tableName.count = _conn.ExecuteScalar<int>("SELECT COUNT(*) FROM " + tableName.name);
                //tableName.count = await (_database.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM "+ tableName.name));
            }
            return temporaryList;
        }
        public List<Item> GetItemList()
        {
            var r = _database.Table<Item>().ToListAsync();
            r.Wait();
            return r.Result;
        }

    }
}

public class TableName
{
    public TableName() { }
    public TableName(string Name) { name = Name; }
    public string name { get; set; }
    public int count { get; set; }
}