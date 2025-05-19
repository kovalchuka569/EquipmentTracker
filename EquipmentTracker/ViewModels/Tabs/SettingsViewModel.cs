using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using Prism.Unity;
using Syncfusion.SfSkinManager;

namespace UI.ViewModels.Tabs
{
    public class SettingsViewModel : BindableBase
    {
        private string _changeThemeButtonImage;
        private string _selectedTheme;

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    ChangeThemeButtonImage = SelectedTheme == "Windows11Dark"
                        ? "pack://application:,,,/Views/NavDrawer/Assets/sun.png"
                        : "pack://application:,,,/Views/NavDrawer/Assets/moon.png";
                }
            }
        }

        public string ChangeThemeButtonImage
        {
            get => _changeThemeButtonImage;
            set => SetProperty(ref _changeThemeButtonImage, value);
        }
        
        public SettingsViewModel()
        {
            
        }

        private void ApplyTheme(string themeName)
        {
            SfSkinManager.ApplyStylesOnApplication = true;
            SfSkinManager.SetTheme(PrismApplication.Current.MainWindow, new Theme(themeName));
        }
    }
}
