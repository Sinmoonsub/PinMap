using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PinMap;
using PinMap.Models;

namespace PinMap.UserControls
{
    public partial class PointDisplayControl : UserControl, IPointDisplayControl
    {
        // 변환 상태
        private double _zoom = 1.0;
        private Vector _offset = new(0, 0);
        private Point _lastMousePos;
        private bool _isPanning;

        // 데이터 바운더리
        private Rect _dataBounds;
        private double _baseScale = 1.0;

        public PointDisplayControl()
        {
            InitializeComponent();

            this.SizeChanged += (s, e) => InvalidateVisual();
            this.MouseWheel += OnMouseWheel;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;

            this.Focusable = true;
            this.MouseEnter += (s, e) => this.Focus();
        }

        #region Dependency Properties

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register(nameof(Points), typeof(IEnumerable<ChannelPoint>),
                typeof(PointDisplayControl), new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender, // ★ 변경 시 자동 렌더
                    OnPointsChanged));

        public IEnumerable<ChannelPoint> Points
        {
            get => (IEnumerable<ChannelPoint>)GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }

        public static readonly DependencyProperty ChannelColorsProperty =
            DependencyProperty.Register(nameof(ChannelColors), typeof(Dictionary<int, Brush>),
                typeof(PointDisplayControl), new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender)); // ★ 변경 시 자동 렌더

        public Dictionary<int, Brush> ChannelColors
        {
            get => (Dictionary<int, Brush>)GetValue(ChannelColorsProperty);
            set => SetValue(ChannelColorsProperty, value);
        }

        private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (PointDisplayControl)d;
            ctrl.CalculateDataBounds();
            ctrl.ResetView();
        }

        #endregion

        #region Data Bounds

        private void CalculateDataBounds()
        {
            if (Points == null || !Points.Any()) return;

            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            foreach (var p in Points)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            _dataBounds = new Rect(
                minX, minY,
                Math.Max(0.0001, maxX - minX),
                Math.Max(0.0001, maxY - minY)
            );

            System.Diagnostics.Debug.WriteLine($"[Bounds] {_dataBounds}");
        }

        public void ResetView()
        {
            _zoom = 1.0;
            _offset = new Vector(0, 0);
            InvalidateVisual(); // ★ DrawingVisual 대신 WPF 기본 렌더 호출
        }

        #endregion

        #region OnRender (DrawingVisual 대신 WPF 기본 방식)

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // 배경
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (Points == null || !Points.Any())
            {
                System.Diagnostics.Debug.WriteLine("[OnRender] Points 없음");
                return;
            }

            if (ActualWidth == 0 || ActualHeight == 0)
            {
                System.Diagnostics.Debug.WriteLine("[OnRender] 크기 없음");
                return;
            }

            if (_dataBounds.IsEmpty || _dataBounds.Width == 0)
                CalculateDataBounds();

            double margin = 0.05;
            double innerWidth = ActualWidth * (1 - margin * 2);
            double innerHeight = ActualHeight * (1 - margin * 2);
            _baseScale = Math.Min(innerWidth / _dataBounds.Width, innerHeight / _dataBounds.Height);

            double marginLeft = ActualWidth * margin;
            double marginTop = ActualHeight * margin;

            int drawCount = 0;

            foreach (var p in Points)
            {
                if (ChannelColors == null || !ChannelColors.TryGetValue(p.Channel, out Brush brush))
                    brush = Brushes.Gray;

                double bx = (p.X - _dataBounds.Left) * _baseScale + marginLeft;
                double by = (p.Y - _dataBounds.Top) * _baseScale + marginTop;

                double sx = bx * _zoom + _offset.X;
                double sy = by * _zoom + _offset.Y;

                if (sx < -5 || sx > ActualWidth + 5 || sy < -5 || sy > ActualHeight + 5) continue;

                dc.DrawEllipse(brush, null, new Point(sx, sy), 2, 2);
                drawCount++;
            }

            System.Diagnostics.Debug.WriteLine($"[OnRender] 그려진 점: {drawCount}개 / 전체: {Points.Count()}개");
        }

        #endregion

        #region Mouse Interactions

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            Point mousePos = e.GetPosition(this);

            _offset = (Vector)mousePos - ((Vector)mousePos - _offset) * zoomFactor;
            _zoom *= zoomFactor;
            _zoom = Math.Clamp(_zoom, 0.05, 200.0);

            InvalidateVisual();
            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Middle)
            {
                _isPanning = true;
                _lastMousePos = e.GetPosition(this);
                this.CaptureMouse();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                Point currentPos = e.GetPosition(this);
                Vector delta = currentPos - _lastMousePos;
                _offset += delta;
                _lastMousePos = currentPos;
                InvalidateVisual();
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            this.ReleaseMouseCapture();
        }

        #endregion
    }
}