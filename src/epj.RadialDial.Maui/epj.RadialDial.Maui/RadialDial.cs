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

    public static readonly BindableProperty InternalPaddingProperty = BindableProperty.Create(nameof(InternalPadding), typeof(float), typeof(RadialDial), 20.0f, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(RadialDial), 200.0f, propertyChanged: OnBindablePropertyChanged);

    //TODO: add Value (with BindingMode.TwoWay), Min (>= 0) and Max properties as well as a boolean property to round to nearest integer

    public RadialDial()
    {
        IgnorePixelScaling = false;
        EnableTouchEvents = true;
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

        //calculate the angle of the touch input
        var touchAngle = Utils
            .PointOnCircle(_touchPoint, _center, _drawRect.Width / 2)
            .ToAngle(_center);

        //calculate the sweepAngle and map it to the 0..360 range
        var sweepAngle = (touchAngle + StartAngle).MapTo360();

        //TODO: set Value property with mapped value in Min/Max range (and rounded to nearest integer, if requested)

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

        _touchPoint = e.Location;

        InvalidateSurface();

        e.Handled = true;
    }

    private static void OnBindablePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((RadialDial)bindable).InvalidateSurface();
    }
}