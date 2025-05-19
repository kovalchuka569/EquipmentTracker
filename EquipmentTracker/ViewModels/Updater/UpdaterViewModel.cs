using System.Net.Http;
using System.Reflection;
using Squirrel;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using System.Threading.Tasks;
using Notification.Wpf;
using System.Windows;

namespace UI.ViewModels.Updater;

public class UpdaterViewModel : BindableBase
{
    UpdateManager _updateManager;
    private readonly IRegionManager _regionManager;
    private readonly NotificationManager _notificationManager;
    private string _updateStatusContent = "Перевірка оновлень...";
    private bool _updateQuestionVisibility;
    private bool _busyIndicatorVisibility = true;
    private bool _finallyUpdateTextVisibility;
    private string _finallyUpdateText;
    private string _finallyUpdateTextForeground;

    public string UpdateStatusContent
    {
        get => _updateStatusContent;
        set => SetProperty(ref _updateStatusContent, value);
    }

    public bool UpdateQuestionVisibility
    {
        get => _updateQuestionVisibility;
        set => SetProperty(ref _updateQuestionVisibility, value);
    }

    public bool BusyIndicatorVisibility
    {
        get => _busyIndicatorVisibility;
        set => SetProperty(ref _busyIndicatorVisibility, value);
    }

    public bool FinallyUpdateTextVisibility
    {
        get => _finallyUpdateTextVisibility;
        set => SetProperty(ref _finallyUpdateTextVisibility, value);
    }

    public string FinallyUpdateTextForeground
    {
        get => _finallyUpdateTextForeground;
        set => SetProperty(ref _finallyUpdateTextForeground, value);
    }

    public string FinallyUpdateText
    {
        get => _finallyUpdateText;
        set => SetProperty(ref _finallyUpdateText, value);
    }

    public DelegateCommand UpdateCommand { get; }
    public DelegateCommand NoUpdateCommand { get; }
    public DelegateCommand UserControlLoadedCommand { get; }
    public UpdaterViewModel(NotificationManager notificationManager, IRegionManager regionManager)
    {
        _notificationManager = notificationManager;
        _regionManager = regionManager;
        
        UpdateCommand = new DelegateCommand(OnUpdate);
        NoUpdateCommand = new DelegateCommand(OnNavigateInAuth);
        UserControlLoadedCommand = new DelegateCommand(OnUserControlLoaded);
    }

    private async void OnUserControlLoaded()
    {
        try
        {
            UpdateStatusContent = "Перевірка оновлень...";
            BusyIndicatorVisibility = true;
            await Task.Delay(1500);

            if (!await IsInternetAvailable())
            {
                UpdateStatusContent = "Відсутнє підключення до мережі";
                _notificationManager.Show("Для оновлення програми, будь-ласка підключіться до мережі інтернет!", NotificationType.Warning);
                await Task.Delay(1500);
                OnNavigateInAuth();
                return;
            } 

            _updateManager = await UpdateManager.GitHubUpdateManager("https://github.com/kovalchuka569/EquipmentTracker");
            var updateInfo = await _updateManager.CheckForUpdate();

            if (updateInfo.ReleasesToApply.Any())
            {
                UpdateQuestionVisibility = true;
            }
            else
            {
                UpdateStatusContent = "Нових оновлень не знайдено";
                await Task.Delay(1500);
                OnNavigateInAuth();
            }
        }
        catch (Exception ex)
        {
            UpdateStatusContent = $"Помилка оновлення: {ex.Message}";
            await Task.Delay(2000);
            OnNavigateInAuth();
        }
        finally
        {
            BusyIndicatorVisibility = false;
        }
    }

    private static async Task<bool> IsInternetAvailable()
    {
        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3)
            };
            using var response = await client.GetAsync("https://www.google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async void OnUpdate()
    {
        try
        {
            BusyIndicatorVisibility = true;
            UpdateQuestionVisibility = false;
            UpdateStatusContent = "Оновлення...";
            await Task.Delay(1500);
            await _updateManager.UpdateApp();
            UpdateStatusContent = "Оновлення завершено...";
            await Task.Delay(1500);

            FinallyUpdateTextVisibility = true;
            FinallyUpdateText = "Успішно оновлено! Будь-ласка перезавантажте програму";
            FinallyUpdateTextForeground = "Blue";
        }
        catch (Exception ex)
        {
            FinallyUpdateTextVisibility = true;
            FinallyUpdateText = $"Помилка оновлення: {ex.Message}";
            FinallyUpdateTextForeground = "Red";
        }
        finally
        {
            BusyIndicatorVisibility = false;
        }
       

    }

    private void OnNavigateInAuth()
    {
        _regionManager.RequestNavigate("MainRegion", "AuthorizationView");
    }

  

    
}