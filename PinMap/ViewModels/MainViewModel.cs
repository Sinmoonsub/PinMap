using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using PinMap.Models;
using System.IO;
using PinMap.Utils;
using CommunityToolkit.Mvvm.Input;



namespace PinMap.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<ChannelPoint> _points;

    [ObservableProperty]
    private Dictionary<int, Brush> channelColors = new()
        {
            {1, Brushes.Red},
            {2, Brushes.Blue},
            {3, Brushes.Green},
            {4, Brushes.Orange}
        };

    private readonly PointDataLoader _dataLoader;

    public MainViewModel()
    {

        _dataLoader = new PointDataLoader();



        // 데이터 처리 엔진을 통해 파일을 로드하고 Points 컬렉션에 대입합니다.
        // 파일 경로 "TextFile.txt"는 실행 파일 위치 기준입니다.
        Points = _dataLoader.LoadPointsFromFile("TextFile.txt");
    }

    [RelayCommand]
    private void Test()
    {
        channelColors = new()
        {
            {1, Brushes.Red},
            {2, Brushes.Blue},
            {3, Brushes.Green},
            {4, Brushes.Orange},
            {5, Brushes.Orange},
            {6, Brushes.Orange},
            {7, Brushes.Orange}
        };
    }

    /*
    /// <summary>
    /// 비즈니스 로직: 파일로부터 데이터를 읽어 ChannelPoint 리스트로 변환합니다.
    /// </summary>
    private IReadOnlyList<ChannelPoint> LoadPointsFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            // 실제 환경에서는 사용자에게 알림을 띄우거나 로그를 남깁니다.
            return new List<ChannelPoint>().AsReadOnly();
        }

        try
        {
            var list = new List<ChannelPoint>();

            // 성능 최적화: 전체 파일을 메모리에 로드하지 않고 한 줄씩 읽기
            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 파싱 로직
                var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;

                // 'Z01' -> 1 변환
                string channelStr = parts[0].TrimStart('Z', 'z');

                if (int.TryParse(channelStr, out int ch) &&
                    double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                    double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
                {
                    list.Add(new ChannelPoint(ch, x, -y));
                }
            }

            return list.AsReadOnly();
        }
        catch (Exception)
        {
            // 예외 발생 시 빈 리스트 반환 (안전성 확보)
            return new List<ChannelPoint>().AsReadOnly();
        }
    }
    */

}