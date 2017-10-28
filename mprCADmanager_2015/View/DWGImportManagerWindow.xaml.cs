using System.Windows;
using System.Windows.Input;
using ModPlusAPI;
using ModPlusAPI.Windows.Helpers;

namespace mprCADmanager.View
{
    // ReSharper disable once InconsistentNaming
    public partial class DWGImportManagerWindow
    {
        public DWGImportManagerWindow()
        {
            InitializeComponent();
            AllowsTransparency = true;
            this.OnWindowStartUp();
        }

        private void DWGImportManagerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            ChkUseWinOpacity.IsChecked =
                bool.TryParse(
                    UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mpCADmanaget", "UseWinOpacity"),
                    out bool b) && b;
        }
        
        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void DWGImportManagerWindow_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Opacity = 1.0;
        }

        private void DWGImportManagerWindow_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (ChkUseWinOpacity.IsChecked != null && ChkUseWinOpacity.IsChecked.Value)
                Opacity = 0.4;
        }

        private void ChkUseWinOpacity_OnChecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpCADmanaget", "UseWinOpacity", true.ToString(), true);
        }

        private void ChkUseWinOpacity_OnUnchecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mpCADmanaget", "UseWinOpacity", false.ToString(), true);
        }
    }
}
