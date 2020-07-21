using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Databases_Viewer.Views;
using Databases_Viewer.Models;
using System.IO;

namespace Databases_Viewer
{
    public partial class App : Application
    {
        static GenericDatabase database;

        public static GenericDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = new GenericDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Animal.db3"));
                }
                return database;
            }
        }
        public App()
        {
            InitializeComponent();


            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
