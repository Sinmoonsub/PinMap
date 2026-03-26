using System;
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
    // Dependency Properties
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
        _transformGroup.Children.Add(_scale);
        _transformGroup.Children.Add(_translate);

        Renderer.RenderTransformValue = _transformGroup;

        SizeChanged += (s, e) =>
        {
            if (_mode == ScaleMode.FitToView) FitToView();
        };

        MouseWheel += PointDisplayControl_MouseWheel;
        MouseDoubleClick += PointDisplayControl_MouseDoubleClick;
    }

    private void PointDisplayControl_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        _mode = ScaleMode.Manual; // 휠 사용 시 FitToView 모드 해제

        double zoomFactor = e.Delta > 0 ? 1.1 : 0.9; // 위로 스크롤: 확대, 아래: 축소

        // 화면 중심을 기준으로 줌
        Point screenCenter = new Point(ActualWidth / 2.0, ActualHeight / 2.0);

        // 변환 이전의 데이터 좌표 계산
        Point dataCenterPoint = InverseTransformPoint(screenCenter);

        // 스케일 적용
        double newScaleX = _scale.ScaleX * zoomFactor;
        double newScaleY = _scale.ScaleY * zoomFactor;

        // 최소/최대 줌 레벨 제한 (선택사항)
        const double minZoom = 0.1;
        const double maxZoom = 10.0;
        newScaleX = Math.Clamp(newScaleX, minZoom, maxZoom);
        newScaleY = Math.Clamp(newScaleY, minZoom, maxZoom);

        _scale.ScaleX = newScaleX;
        _scale.ScaleY = newScaleY;

        // 화면 중심이 같은 데이터 좌표에 유지되도록 트랜슬레이션 조정
        Point newScreenCenterDataPoint = InverseTransformPoint(screenCenter);
        _translate.X += (dataCenterPoint.X - newScreenCenterDataPoint.X) * newScaleX;
        _translate.Y += (dataCenterPoint.Y - newScreenCenterDataPoint.Y) * newScaleY;

        UpdateRenderer();
        e.Handled = true;
    }

    private void PointDisplayControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // 마우스 우클릭 더블클릭 감지
        if (e.ChangedButton == MouseButton.Right)
        {
            _mode = ScaleMode.FitToView;
            FitToView();
            e.Handled = true;
        }
    }

    private Point InverseTransformPoint(Point screenPoint)
    {
        // 스크린 좌표를 데이터 좌표로 변환
        double dataX = (screenPoint.X - _translate.X) / _scale.ScaleX;
        double dataY = (screenPoint.Y - _translate.Y) / _scale.ScaleY;
        return new Point(dataX, dataY);
    }

    /// <summary>
    /// 외부에서 강제로 색상 정보를 갱신하고 화면을 다시 그리도록 합니다.
    /// Dictionary 내부 값만 변경했을 때 호출하십시오.
    /// </summary>
    public void RefreshColors()
    {
        Renderer.ChannelColors = this.ChannelColors;
        Renderer.InvalidateVisual();
    }

    /// <summary>
    /// 모든 데이터와 변환을 초기화하고 다시 그립니다.
    /// </summary>
    public void Redraw()
    {
        Renderer.InvalidateVisual();
    }

    // [FitToView, Mouse Events 등 이전 로직 유지...]

    public void FitToView()
    {
        if (_dataBounds.IsEmpty || ActualWidth == 0 || ActualHeight == 0) return;

        double margin = 0.9;
        double scaleX = (ActualWidth * margin) / _dataBounds.Width;
        double scaleY = (ActualHeight * margin) / _dataBounds.Height;
        double scale = Math.Min(scaleX, scaleY);

        _scale.ScaleX = scale;
        _scale.ScaleY = scale;

        double dataCenterX = (_dataBounds.Left + _dataBounds.Right) / 2.0;
        double dataCenterY = (_dataBounds.Top + _dataBounds.Bottom) / 2.0;

        _translate.X = (ActualWidth / 2.0) - (dataCenterX * scale);
        _translate.Y = (ActualHeight / 2.0) - (dataCenterY * scale);

        UpdateRenderer();
    }

    private void UpdateRenderer()
    {
        Renderer.ScaleValue = _scale.ScaleX;
        Renderer.InvalidateVisual();
    }

    private void CalcBounds(IReadOnlyList<ChannelPoint> points)
    {
        if (points == null || points.Count == 0) { _dataBounds = Rect.Empty; return; }
        double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;
        foreach (var p in points)
        {
            if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
            if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;
        }
        _dataBounds = new Rect(new Point(minX, minY), new Point(maxX, maxY));
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
        // 새로운 Dictionary 객체가 할당될 때 호출됨
        ctrl.Renderer.ChannelColors = e.NewValue as Dictionary<int, Brush>;
        ctrl.Renderer.InvalidateVisual();
    }
}