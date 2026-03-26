using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using PinMap.Models;

namespace PinMap.UserControls;

public class RenderElement : FrameworkElement
{
    public static readonly DependencyProperty PointsProperty =
        DependencyProperty.Register(nameof(Points), typeof(IReadOnlyList<ChannelPoint>), typeof(RenderElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public IReadOnlyList<ChannelPoint> Points
    {
        get => (IReadOnlyList<ChannelPoint>)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public Dictionary<int, Brush> ChannelColors { get; set; }
    public Transform RenderTransformValue { get; set; }
    public double ScaleValue { get; set; } = 1.0;
    public bool UseDynamicPointSize { get; set; } = true;

    protected override void OnRender(DrawingContext dc)
    {
        if (Points == null) return;

        // 배경을 투명하게 (배경색 칠하지 않음)

        // 전체 변환 적용
        dc.PushTransform(RenderTransformValue ?? Transform.Identity);

        // [디버그용] 원점(0,0) 표시 (필요 없으면 삭제)
        // dc.DrawEllipse(Brushes.Yellow, null, new Point(0, 0), 2/ScaleValue, 2/ScaleValue);

        double radius = UseDynamicPointSize
            ? Math.Max(ScaleValue * 0.03, 0.3)  // 줌 아웃(축소)할 때 점이 커짐, 줌 인(확대)할 때 작아짐
            : 1.0;

        // 성능을 위해 Pen은 null로 설정 (외곽선 없음)
        var brushCache = new Dictionary<int, Brush>();

        foreach (var p in Points)
        {
            if (!brushCache.TryGetValue(p.Channel, out var brush))
            {
                if (ChannelColors != null && ChannelColors.TryGetValue(p.Channel, out var b))
                {
                    brush = b;
                    if (brush.CanFreeze) brush.Freeze();
                }
                else
                {
                    // 기본색을 검정색으로 변경
                    brush = Brushes.Black;
                }
                brushCache[p.Channel] = brush;
            }

            // 실제 좌표 p.X, p.Y에 그리기
            dc.DrawEllipse(brush, null, new Point(p.X, p.Y), radius, radius);
        }

        dc.Pop();
    }
}