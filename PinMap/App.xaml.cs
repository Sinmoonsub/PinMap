using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using PinMap.Models;
using PinMap.ViewModels;

namespace PinMap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainViewModel = new MainViewModel();
            var mainWindow = new MainWindow();

            mainWindow.DataContext = mainViewModel;

            // ★ DataContext 설정 후 컨트롤 등록
            mainWindow.RegisterViewModel(mainViewModel);

            mainWindow.Closed += (sender, args) =>
            {
                mainViewModel.Dispose();
            };

            mainWindow.Show();

            // ★ 창이 완전히 로드된 후 데이터 로드
            mainWindow.Loaded += (s, e) =>
            {
                mainViewModel.LoadDataCommand.Execute(null);
            };

        }
    }
}
