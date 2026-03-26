using System.Windows;
using System.Windows.Media;
using PinMap.Models;

namespace PinMap.UserControls;

public class RenderElement : FrameworkElement
{
    // =========================
    // Dependency Properties
    // =========================

    public static readonly DependencyProperty PointsProperty =
        DependencyProperty.Register(nameof(Points),
            typeof(IReadOnlyList<ChannelPoint>),
            typeof(RenderElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPointsChanged));

    public IReadOnlyList<ChannelPoint> Points
    {
        get => (IReadOnlyList<ChannelPoint>)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public static readonly DependencyProperty ChannelColorsProperty =
        DependencyProperty.Register(nameof(ChannelColors),
            typeof(Dictionary<int, Brush>),
            typeof(RenderElement),
            new FrameworkPropertyMetadata(null, OnColorsChanged));

    public Dictionary<int, Brush> ChannelColors
    {
        get => (Dictionary<int, Brush>)GetValue(ChannelColorsProperty);
        set => SetValue(ChannelColorsProperty, value);
    }

    // =========================
    // 렌더 옵션
    // =========================

    public Transform RenderTransformValue { get; set; }
    public double ScaleValue { get; set; } = 1.0;
    public bool UseDynamicPointSize { get; set; } = true;

    // =========================
    // 성능 최적화 캐시
    // =========================

    private readonly Dictionary<int, Brush> _brushCache = new();

    // =========================
    // 생성자
    // =========================

    public RenderElement()
    {
        // 성능 최적화 (안티앨리어싱 OFF)
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }

    // =========================
    // 캐시 생성
    // =========================

    private void BuildBrushCache()
    {
        _brushCache.Clear();

        if (Points == null)
            return;

        foreach (var p in Points)
        {
            if (_brushCache.ContainsKey(p.Channel))
                continue;

            Brush brush;

            if (ChannelColors != null && ChannelColors.TryGetValue(p.Channel, out var b))
            {
                brush = b;

                // Freeze 가능하면 freeze → 성능 향상
                if (brush.CanFreeze)
                    brush.Freeze();
            }
            else
            {
                brush = Brushes.Black;
            }

            _brushCache[p.Channel] = brush;
        }
    }

    // =========================
    // Property Changed
    // =========================

    private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var r = (RenderElement)d;
        r.BuildBrushCache();
    }

    private static void OnColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var r = (RenderElement)d;
        r.BuildBrushCache();
        r.InvalidateVisual();
    }

    // =========================
    // Render
    // =========================

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (Points == null || Points.Count == 0)
            return;

        // Transform 적용
        dc.PushTransform(RenderTransformValue ?? Transform.Identity);

        // 점 크기 (네 의도 유지)
        double radius = UseDynamicPointSize
            ? Math.Max(ScaleValue * 0.03, 0.3)
            : 1.0;

        // 렌더링 (🔥 객체 생성 없음)
        foreach (var p in Points)
        {
            if (!_brushCache.TryGetValue(p.Channel, out var brush))
                brush = Brushes.Black;

            dc.DrawEllipse(brush, null, new Point(p.X, p.Y), radius, radius);
        }

        dc.Pop();
    }
}