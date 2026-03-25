using System;
using System.Globalization;
using System.IO;
using PinMap.Models;

namespace PinMap.Utils
{
    /// <summary>
    /// 채널 정보와 좌표를 담는 레코드
    /// </summary>
    public class PointDataLoader
    {
        /// <summary>
        /// 파일로부터 데이터를 로드하고, 로드 시점에 Y축을 반전하여 WPF 좌표계 방향에 맞춥니다.
        /// </summary>
        public IReadOnlyList<ChannelPoint> LoadPointsFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return new List<ChannelPoint>().AsReadOnly();

            try
            {
                var list = new List<ChannelPoint>();

                foreach (var line in File.ReadLines(filePath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(new[] { '\t', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 3) continue;

                    string channelStr = parts[0].TrimStart('Z', 'z');

                    if (int.TryParse(channelStr, out int ch) &&
                        double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double x) &&
                        double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double y))
                    {
                        // 개선 사항: 파일 로드 시점에 Y축 좌표를 반전시켜 WPF의 Y-Down 좌표계 방향에 맞춤
                        // 이렇게 하면 이후 시각화 로직에서 추가적인 반전 계산 없이 직관적으로 처리가 가능합니다.
                        list.Add(new ChannelPoint(ch, x, -y));
                    }
                }
                return list.AsReadOnly();
            }
            catch (Exception)
            {
                return new List<ChannelPoint>().AsReadOnly();
            }
        }
    }
}
