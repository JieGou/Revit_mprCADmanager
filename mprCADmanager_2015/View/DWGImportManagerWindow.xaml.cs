using System.Windows;

namespace mprCADmanager.View
{
    // ReSharper disable once InconsistentNaming
    public partial class DWGImportManagerWindow
    {
        private const string LangItem = "mprCADmanager";

        public DWGImportManagerWindow()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(LangItem, "h1");
        }

        private void DWGImportManagerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }
    }
}
