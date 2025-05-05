using System.Collections.ObjectModel;
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
        
        private DelegateCommand _changeThemeCommand;

        public DelegateCommand ChangeThemeCommand =>
            _changeThemeCommand ??= new DelegateCommand(OnChangeTheme);
        public SettingsViewModel()
        {
            SelectedTheme = Properties.Settings.Default.Theme;
            
        }

        private void OnChangeTheme()
        {
            SelectedTheme = SelectedTheme == "Windows11Dark" ? "Windows11Light" : "Windows11Dark";
            ApplyTheme(SelectedTheme);
            Properties.Settings.Default.Theme = SelectedTheme;
            Properties.Settings.Default.Save();
        }

        private void ApplyTheme(string themeName)
        {
            SfSkinManager.ApplyStylesOnApplication = true;
            SfSkinManager.SetTheme(PrismApplication.Current.MainWindow, new Theme(themeName));
        }
    }
}
