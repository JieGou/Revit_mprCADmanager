namespace mprCADmanager.View
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;

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

        private async void PopupCopied_OnOpened(object sender, EventArgs e)
        {
            await Task.Delay(500).ConfigureAwait(true);
            PopupCopied.IsOpen = false;
        }
    }
}
