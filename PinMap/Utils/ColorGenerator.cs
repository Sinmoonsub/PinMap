using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PinMap.Utils;
public static class ColorGenerator
{
    /// <summary>
    /// 1부터 지정된 개수까지의 채널에 대해 랜덤한 색상을 가진 딕셔너리를 생성합니다.
    /// </summary>
    /// <param name="count">생성할 채널의 개수 (예: 132)</param>
    /// <returns>채널 번호와 브러시가 매칭된 딕셔너리</returns>
    public static Dictionary<int, Brush> GenerateRandomColors(int count = 132)
    {
        var random = new Random();
        var colorDict = new Dictionary<int, Brush>();

        for (int i = 1; i <= count; i++)
        {
            // RGB 값을 랜덤으로 생성
            byte r = (byte)random.Next(0, 256);
            byte g = (byte)random.Next(0, 256);
            byte b = (byte)random.Next(0, 256);

            var color = Color.FromRgb(r, g, b);
            var brush = new SolidColorBrush(color);

            // WPF 성능 최적화를 위해 브러시 동결(Freeze)
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            colorDict.Add(i, brush);
        }

        return colorDict;
    }
}