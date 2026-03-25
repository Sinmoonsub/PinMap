using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PinMap.Models;

namespace PinMap.UserControls;

public partial class PointDisplayControl : UserControl
{
    private ScaleTransform _scale = new ScaleTransform(1, 1);
    private TranslateTransform _translate = new TranslateTransform();
    private TransformGroup _transformGroup;

    private Point _lastMousePos;
    private Rect _dataBounds = Rect.Empty;
    private ScaleMode _mode = ScaleMode.FitToView;

    // ============================================================
    // Dependency Properties (외부 바인딩을 위한 속성)
    // ============================================================

    public static readonly DependencyProperty PointsProperty =
        DependencyProperty.Register(nameof(Points),
            typeof(IReadOnlyList<ChannelPoint>),
            typeof(PointDisplayControl),
            new PropertyMetadata(null, OnPointsChanged));

    public IReadOnlyList<ChannelPoint> Points
    {
        get => (IReadOnlyList<ChannelPoint>)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public static readonly DependencyProperty ChannelColorsProperty =
        DependencyProperty.Register(nameof(ChannelColors),
            typeof(Dictionary<int, Brush>),
            typeof(PointDisplayControl),
            new PropertyMetadata(null, OnColorsChanged));

    public Dictionary<int, Brush> ChannelColors
    {
        get => (Dictionary<int, Brush>)GetValue(ChannelColorsProperty);
        set => SetValue(ChannelColorsProperty, value);
    }

    // ============================================================
    // 생성자 및 초기화
    // ============================================================

    public PointDisplayControl()
    {
        InitializeComponent();

        _transformGroup = new TransformGroup();
        // 1. 배율(Scale) 적용 후 2. 이동(Translate) 적용
        _transformGroup.Children.Add(_scale);
        _transformGroup.Children.Add(_translate);

        Renderer.RenderTransformValue = _transformGroup;

        SizeChanged += (s, e) =>
        {
            if (_mode == ScaleMode.FitToView) FitToView();
        };
    }

    private void CalcBounds(IReadOnlyList<ChannelPoint> points)
    {
        if (points == null || points.Count == 0)
        {
            _dataBounds = Rect.Empty;
            return;
        }

        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;

        foreach (var p in points)
        {
            if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
            if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;
        }

        _dataBounds = new Rect(new Point(minX, minY), new Point(maxX, maxY));
    }

    public void FitToView()
    {
        if (_dataBounds.IsEmpty || ActualWidth == 0 || ActualHeight == 0) return;

        double margin = 0.9;
        double scaleX = (ActualWidth * margin) / _dataBounds.Width;
        double scaleY = (ActualHeight * margin) / _dataBounds.Height;
        double scale = Math.Min(scaleX, scaleY);

        // WPF 표준 좌표계 (+Y가 아래)
        _scale.ScaleX = scale;
        _scale.ScaleY = scale;

        double dataCenterX = (_dataBounds.Left + _dataBounds.Right) / 2.0;
        double dataCenterY = (_dataBounds.Top + _dataBounds.Bottom) / 2.0;

        _translate.X = (ActualWidth / 2.0) - (dataCenterX * scale);
        _translate.Y = (ActualHeight / 2.0) - (dataCenterY * scale);

        UpdateRenderer();
    }

    // ============================================================
    // 이벤트 핸들러 (줌/팬)
    // ============================================================

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        double zoomFactor = e.Delta > 0 ? 1.2 : 0.8;
        Point mousePos = e.GetPosition(this);

        double newScale = _scale.ScaleX * zoomFactor;
        if (newScale < 0.001 || newScale > 50) return;

        _translate.X = mousePos.X - (mousePos.X - _translate.X) * zoomFactor;
        _translate.Y = mousePos.Y - (mousePos.Y - _translate.Y) * zoomFactor;

        _scale.ScaleX = newScale;
        _scale.ScaleY = newScale;

        UpdateRenderer();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.MiddleButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed)
        {
            _lastMousePos = e.GetPosition(this);
            CaptureMouse();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (IsMouseCaptured)
        {
            Point currentPos = e.GetPosition(this);
            Vector delta = currentPos - _lastMousePos;

            _translate.X += delta.X;
            _translate.Y += delta.Y;

            _lastMousePos = currentPos;
            Renderer.InvalidateVisual();
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e) => ReleaseMouseCapture();

    private void UpdateRenderer()
    {
        Renderer.ScaleValue = _scale.ScaleX;
        Renderer.InvalidateVisual();
    }

    // ============================================================
    // 속성 변경 알림 처리
    // ============================================================

    private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (PointDisplayControl)d;
        var newPoints = e.NewValue as IReadOnlyList<ChannelPoint>;

        ctrl.Renderer.Points = newPoints;

        if (newPoints != null)
        {
            ctrl.CalcBounds(newPoints);
            if (ctrl._mode == ScaleMode.FitToView) ctrl.FitToView();
        }

        ctrl.Renderer.InvalidateVisual();
    }

    private static void OnColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (PointDisplayControl)d;
        ctrl.Renderer.ChannelColors = e.NewValue as Dictionary<int, Brush>;
        ctrl.Renderer.InvalidateVisual();
    }
}