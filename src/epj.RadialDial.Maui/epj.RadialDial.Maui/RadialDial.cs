using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace epj.RadialDial.Maui;

public class RadialDial : SKCanvasView
{
    private const float StartAngle = -90.0f;

    private int _size;
    private SKCanvas _canvas;
    private SKPoint _center;
    private SKRect _drawRect;
    private SKImageInfo _info;
    private SKPoint _touchPoint;
    private bool _hasTouch;

    public float InternalPadding
    {
        get => (float)GetValue(InternalPaddingProperty);
        set => SetValue(InternalPaddingProperty, value);
    }

    public float StrokeWidth
    {
        get => (float)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public float Min
    {
        get => (float)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }
    public float Max
    {
        get => (float)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public float Value
    {
        get => (float)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool SnapToNearestInteger
    {
        get => (bool)GetValue(SnapToNearestIntegerProperty);
        set => SetValue(SnapToNearestIntegerProperty, value);
    }

    public static readonly BindableProperty InternalPaddingProperty = BindableProperty.Create(nameof(InternalPadding), typeof(float), typeof(RadialDial), 20.0f, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(RadialDial), 200.0f, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty MinProperty = BindableProperty.Create(nameof(Min), typeof(float), typeof(RadialDial), 0.0f);

    public static readonly BindableProperty MaxProperty = BindableProperty.Create(nameof(Max), typeof(float), typeof(RadialDial), 60.0f);

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(float), typeof(RadialDial), 10.0f, BindingMode.TwoWay, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty SnapToNearestIntegerProperty = BindableProperty.Create(nameof(SnapToNearestInteger), typeof(bool), typeof(RadialDial), true);

    public RadialDial()
    {
        IgnorePixelScaling = false;
        EnableTouchEvents = true;
        _hasTouch = false;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        _canvas = e.Surface.Canvas;
        _canvas.Clear();

        _info = e.Info;
        _size = Math.Min(_info.Size.Width, _info.Size.Height);

        //offsets are used to always center the dial inside the canvas and move the stroke inwards only
        var horizontalOffset = StrokeWidth / 2 + InternalPadding;
        var verticalOffset = StrokeWidth / 2 + InternalPadding;

        //setup the rectangle which we will draw in and the center point of the dial
        _drawRect = new SKRect(horizontalOffset, verticalOffset, _size - horizontalOffset, _size - verticalOffset);
        _center = new SKPoint(_drawRect.MidX, _drawRect.MidY);

        float sweepAngle;
        var deltaMaxMin = Max - Min;

        if (_hasTouch)
        {
            //calculate the angle of the touch input
            var touchAngle = Utils
                .PointOnCircle(_touchPoint, _center, _drawRect.Width / 2)
                .ToAngle(_center);

            //calculate the sweepAngle and map it to the 0..360 range
            sweepAngle = (touchAngle + StartAngle).MapTo360();

            var resultValue = deltaMaxMin / 360.0f * sweepAngle;

            if (SnapToNearestInteger)
            {
                //round to nearest integer and update sweepAngle
                var snapValue = (float)Math.Round(resultValue);
                sweepAngle = 360.0f / deltaMaxMin * snapValue;
                Value = snapValue;
            }
            else
            {
                Value = resultValue;
            }
        }
        else
        {
            sweepAngle = 360.0f / deltaMaxMin * Value;
        }

        using (var path = new SKPath())
        {
            path.AddArc(_drawRect, StartAngle, sweepAngle);
            _canvas.DrawPath(path, new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Colors.Red.ToSKColor(),
                StrokeWidth = StrokeWidth,
                IsAntialias = true
            });
        }
    }

    protected override void OnTouch(SKTouchEventArgs e)
    {
        base.OnTouch(e);

        _hasTouch = true;

        _touchPoint = e.Location;

        InvalidateSurface();

        e.Handled = true;
    }

    private static void OnBindablePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((RadialDial)bindable).InvalidateSurface();
    }
}