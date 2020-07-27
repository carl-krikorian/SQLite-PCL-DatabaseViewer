using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Databases_Viewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DatabaseMasterDetailPage : ContentPage
    {
        ViewCell givenCell;
        public DatabaseMasterDetailPage()
        {
            InitializeComponent();
            Title = "Tables";
        }
        //Is used to remove the orange highLight color when a Cell is tapped
        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            if (givenCell != null)
                givenCell.View.BackgroundColor = Color.Transparent;
            var viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.Transparent;
                givenCell = viewCell;
            }
        }
        // Assigns null to the Selected Item of the ListView so it can be selected again when back button is pressed
        private void TableListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}