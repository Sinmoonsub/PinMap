using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using PinMap.Models;
using System.IO;
using PinMap.Utils;
using CommunityToolkit.Mvvm.Input;
using System.Windows;



namespace PinMap.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<ChannelPoint>? _points;

    private Dictionary<int, Brush>? _channelColors;
    public Dictionary<int, Brush>? ChannelColors
    {
        get => _channelColors;
        set
        {
            _channelColors = value;
            OnPropertyChanged(); // 프로퍼티 변경 알림 (INotifyPropertyChanged)
        }
    }

    [ObservableProperty] private Brush _bC;

    private readonly PointDataLoader _dataLoader;

    public MainViewModel()
    {

        _dataLoader = new PointDataLoader();

        // 데이터 처리 엔진을 통해 파일을 로드하고 Points 컬렉션에 대입합니다.
        // 파일 경로 "TextFile.txt"는 실행 파일 위치 기준입니다.
        Points = _dataLoader.LoadPointsFromFile("Sample.txt");
    }

    [RelayCommand]
    private void Test()
    {
        ChannelColors = ColorGenerator.GenerateRandomColors();
    }

    [RelayCommand]
    private void C1()
    {
        BC = Brushes.White;
    }

    [RelayCommand]
    private void C2()
    {
        BC = Brushes.DarkGray;
    }

    [RelayCommand]
    private void C3()
    {
        BC = Brushes.Gray;
    }
}

