using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace epj.RadialDial.Maui;

public class RadialDial : SKCanvasView
{
    private const float StartAngle = -90.0f;

    private int _size;
    private SKCanvas _canvas;
    private SKRect _dialRect;
    private SKPoint _dialCenter;
    private SKRect _scaleRect;
    private SKPoint _scaleCenter;
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

    public int Min
    {
        get => (int)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }
    public int Max
    {
        get => (int)GetValue(MaxProperty);
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

    public bool TouchInputEnabled
    {
        get => (bool)GetValue(TouchInputEnabledProperty);
        set => SetValue(TouchInputEnabledProperty, value);
    }

    public bool ShowScale
    {
        get => (bool)GetValue(ShowScaleProperty);
        set => SetValue(ShowScaleProperty, value);
    }

    public int ScaleUnits
    {
        get => (int)GetValue(ScaleUnitsProperty);
        set => SetValue(ScaleUnitsProperty, value);
    }

    public float ScaleDistance
    {
        get => (float)GetValue(ScaleDistanceProperty);
        set => SetValue(ScaleDistanceProperty, value);
    }

    public float ScaleLength
    {
        get => (float)GetValue(ScaleLengthProperty);
        set => SetValue(ScaleLengthProperty, value);
    }

    public float ScaleThickness
    {
        get => (float)GetValue(ScaleThicknessProperty);
        set => SetValue(ScaleThicknessProperty, value);
    }

    public Color DialColor
    {
        get => (Color)GetValue(DialColorProperty);
        set => SetValue(DialColorProperty, value);
    }

    public Color BaseColor
    {
        get => (Color)GetValue(BaseColorProperty);
        set => SetValue(BaseColorProperty, value);
    }

    public Color ScaleColor
    {
        get => (Color)GetValue(ScaleColorProperty);
        set => SetValue(ScaleColorProperty, value);
    }

    public static readonly BindableProperty InternalPaddingProperty = BindableProperty.Create(nameof(InternalPadding), typeof(float), typeof(RadialDial), 20.0f, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(RadialDial), 200.0f, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty MinProperty = BindableProperty.Create(nameof(Min), typeof(int), typeof(RadialDial), 0, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty MaxProperty = BindableProperty.Create(nameof(Max), typeof(int), typeof(RadialDial), 60, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(float), typeof(RadialDial), 10.0f, BindingMode.TwoWay, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty SnapToNearestIntegerProperty = BindableProperty.Create(nameof(SnapToNearestInteger), typeof(bool), typeof(RadialDial), true);

    public static readonly BindableProperty TouchInputEnabledProperty = BindableProperty.Create(nameof(TouchInputEnabled), typeof(bool), typeof(RadialDial), false, propertyChanged: OnTouchInputEnabledPropertyChanged);

    public static readonly BindableProperty DialColorProperty = BindableProperty.Create(nameof(DialColor), typeof(Color), typeof(RadialDial), Colors.Red, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty BaseColorProperty = BindableProperty.Create(nameof(BaseColor), typeof(Color), typeof(RadialDial), Colors.LightGray, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty ScaleColorProperty = BindableProperty.Create(nameof(ScaleColor), typeof(Color), typeof(RadialDial), Colors.LightGray, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty ShowScaleProperty = BindableProperty.Create(nameof(ShowScale), typeof(bool), typeof(RadialDial), true, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty ScaleUnitsProperty = BindableProperty.Create(nameof(ScaleUnits), typeof(int), typeof(RadialDial), 5, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty ScaleDistanceProperty = BindableProperty.Create(nameof(ScaleDistance), typeof(float), typeof(RadialDial), 20.0f, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty ScaleLengthProperty = BindableProperty.Create(nameof(ScaleLength), typeof(float), typeof(RadialDial), 30.0f, propertyChanged: OnBindablePropertyChanged);

    public static readonly BindableProperty ScaleThicknessProperty = BindableProperty.Create(nameof(ScaleThickness), typeof(float), typeof(RadialDial), 10.0f, propertyChanged: OnBindablePropertyChanged);

    public RadialDial()
    {
        IgnorePixelScaling = false;
        EnableTouchEvents = TouchInputEnabled;
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
        var scaleOffset = InternalPadding;
        var dialOffset = StrokeWidth / 2 + InternalPadding + ScaleLength + ScaleDistance;

        //setup the drawing rectangle and center for the scale
        _scaleRect = new SKRect(scaleOffset, scaleOffset, _size - scaleOffset, _size - scaleOffset);
        _scaleCenter = new SKPoint(_scaleRect.MidX, _scaleRect.MidY);

        //setup the drawing rectangle and center for the dial
        _dialRect = new SKRect(dialOffset, dialOffset, _size - dialOffset, _size - dialOffset);
        _dialCenter = new SKPoint(_dialRect.MidX, _dialRect.MidY);

        DrawScale();
        DrawBase();
        DrawDial();
    }

    private void DrawScale()
    {
        if (!ShowScale)
        {
            return;
        }

        //calculate amount and divisor for scale units
        var scaleDivisor = (Max - Min) / (float)ScaleUnits;
        var unitCount = (int)Math.Floor(scaleDivisor);

        if ((scaleDivisor - unitCount) * ScaleUnits > 1.0f)
        {
            unitCount += 1;
        }

        //account for scale fractions
        var clipFactor = unitCount / scaleDivisor;
        var clippedAngle = 360.0f * clipFactor;

        //calculate angles for scale
        var angles = new float[unitCount];
        for (var i = 0; i < unitCount; i++)
        {
            angles[i] = clippedAngle / unitCount * i;
        }

        //draw scale units for each angle
        foreach (var angle in angles)
        {
            var rad = angle.DegreeToRadian();

            var p0 = rad.ToPointOnCircle(_scaleCenter, _scaleRect.Width / 2);
            var p1 = rad.ToPointOnCircle(_scaleCenter, _scaleRect.Width / 2 - ScaleLength);

            using (var path = new SKPath())
            {
                path.AddPoly(new[] { p0, p1 }, close: false);
                _canvas.Save();
                _canvas.RotateDegrees(StartAngle, _dialCenter.X, _dialCenter.Y);
                _canvas.DrawPath(path, new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = ScaleColor.ToSKColor(),
                    StrokeWidth = ScaleThickness,
                    IsAntialias = true
                });
                _canvas.Restore();
            }
        }
    }

    private void DrawDial()
    {
        float sweepAngle;
        var deltaMaxMin = Max - Min;

        if (_hasTouch)
        {
            //calculate the angle of the touch input
            var touchAngle = _touchPoint
                .MapToCircle(_dialCenter, _dialRect.Width / 2)
                .ToAngle(_dialCenter);

            sweepAngle = (touchAngle + StartAngle).NormalizeAngleTo360();

            var resultValue = Min + (deltaMaxMin / 360.0f * sweepAngle);

            if (SnapToNearestInteger)
            {
                //round to nearest integer and update sweepAngle
                var snapValue = (float)Math.Round(resultValue);
                sweepAngle = 360.0f / deltaMaxMin * (snapValue - Min);
                Value = snapValue;
            }
            else
            {
                Value = resultValue;
            }
        }
        else
        {
            sweepAngle = 360.0f / deltaMaxMin * (Value - Min);
        }

        using (var dialPath = new SKPath())
        {
            dialPath.AddArc(_dialRect, StartAngle, sweepAngle);
            _canvas.DrawPath(dialPath, new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = DialColor.ToSKColor(),
                StrokeWidth = StrokeWidth,
                IsAntialias = true
            });
        }
    }

    private void DrawBase()
    {
        using (var basePath = new SKPath())
        {
            basePath.AddArc(_dialRect, 0, 360);
            _canvas.DrawPath(basePath, new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = BaseColor.ToSKColor(),
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

    private static void OnTouchInputEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((RadialDial)bindable).EnableTouchEvents = (bool)newValue;
        ((RadialDial)bindable).InvalidateSurface();
    }
}