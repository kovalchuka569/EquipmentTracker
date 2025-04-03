using System.Windows.Media;
using System.Windows;
using Data.Properties;
using Data.AppDbContext;

namespace UI.ViewModels.Auth
{
    class ExpanderViewModel : BindableBase
    {
        #region Properties
        private string _dbHost = Settings.Default.DbHost;
        private string _dbName = Settings.Default.DbName;
        private string _dbUser = Settings.Default.DbUser;
        private string _dbPassword = Settings.Default.DbPassword;

        private string _buttonSaveLabel = "Зберегти";

        private Brush _buttonSaveContentColor = Brushes.Black;
        private Brush _statusColor = Brushes.Gray;
        
        private readonly AppDbContext _context;

        public Brush StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }
        public Brush ButtonSaveContentColor
        {
            get => _buttonSaveContentColor;
            set => SetProperty(ref _buttonSaveContentColor, value);
        }
        public string ButtonSaveLabel
        {
            get => _buttonSaveLabel;
            set => SetProperty(ref _buttonSaveLabel, value);
        }
        public string DbHost
        {
            get => _dbHost;
            set
            {
                SetProperty(ref _dbHost, value);
                UpdateButtonSave();
            }
        }
        public string DbName
        {
            get => _dbName;
            set
            {
                SetProperty(ref _dbName, value);
                UpdateButtonSave();
            }
        }
        public string DbUser
        {
            get => _dbUser;
            set
            {
                SetProperty(ref _dbUser, value);
                UpdateButtonSave();
            }
        }
        public string DbPassword
        {
            get => _dbPassword;
            set
            {
                SetProperty(ref _dbPassword, value);
                UpdateButtonSave();
            }
        }
        #endregion
        #region Commands
        public DelegateCommand SaveCommand { get; private set; }
        #endregion
        #region Constructor
        public ExpanderViewModel(AppDbContext context)
        {
            _context = context;
            TestDbConnectionAsync();
            SaveCommand = new DelegateCommand(OnSave);
        }
        #endregion
        #region OnSave
        private void OnSave()
        {
            Settings.Default.DbName = DbName;
            Settings.Default.DbHost = DbHost;
            Settings.Default.DbUser = DbUser;
            Settings.Default.DbPassword = DbPassword;
            Settings.Default.Save();
            TestDbConnectionAsync();
            ButtonSaveLabel = "Збережено ✓";
            ButtonSaveContentColor = Brushes.Green;
        }
        #endregion
        #region TestDbConnectionAsync
        private async Task TestDbConnectionAsync()
        {
            using (var db = _context)
            try
                {
                    bool canConnect = await Task.Run(() =>
                    {
                        using (var db = _context)
                        {
                            return db.Database.CanConnect();
                        }
                    });
                    StatusColor = canConnect ? Brushes.Lime : Brushes.Red;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка з'єднання: {ex.Message}");
                    StatusColor = Brushes.Red;
                }
        }
        #endregion
        #region UpdateButtonSave
        private void UpdateButtonSave()
        {
            if (!string.IsNullOrEmpty(DbUser) && !string.IsNullOrEmpty(DbHost) && !string.IsNullOrEmpty(DbName) && !string.IsNullOrEmpty(DbPassword))
            {
                ButtonSaveLabel = "Зберегти";
                ButtonSaveContentColor = Brushes.Black;
            }
        }
        #endregion
    }
}
