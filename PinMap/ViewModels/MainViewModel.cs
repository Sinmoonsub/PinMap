using System.Collections.Generic;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PinMap.Models;
using PinMap.UserControls;

namespace PinMap.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private IPointDisplayControl _mapControl;

        [ObservableProperty]
        private IEnumerable<ChannelPoint> _myPoints;

        [ObservableProperty]
        private Dictionary<int, Brush> _myColors;

        [ObservableProperty]
        private string _statusText;

        public MainViewModel()
        {
            MyColors = new Dictionary<int, Brush>
            {
                { 1, Brushes.Red    },
                { 2, Brushes.Blue   },
                { 3, Brushes.Green  },
                { 4, Brushes.Orange },
            };
        }

        public void RegisterMapControl(IPointDisplayControl control)
        {
            _mapControl = control;
        }

        [RelayCommand]
        private void LoadData()
        {
            var random = new Random();
            var points = new List<ChannelPoint>();

            for (int ch = 1; ch <= 4; ch++)
            {
                for (int i = 0; i < 2500; i++)
                {
                    points.Add(new ChannelPoint
                    {
                        X = random.NextDouble() * 1000,
                        Y = random.NextDouble() * 1000,
                        Channel = ch
                    });
                }
            }

            MyPoints = points;
            StatusText = $"총 {points.Count}개 포인트 로드 완료";
        }

        [RelayCommand]
        private void ResetView()
        {
            _mapControl?.ResetView();
        }

        [RelayCommand]
        private void ClearData()
        {
            MyPoints = null;
            StatusText = "데이터 초기화 완료";
        }

        public void Dispose()
        {
            MyPoints = null;
            _mapControl = null;
        }
    }
}