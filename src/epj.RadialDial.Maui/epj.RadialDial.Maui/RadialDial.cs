using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace epj.RadialDial.Maui;

public class RadialDial : SKCanvasView
{
    public int Size { get; set; }
    public float InternalPadding { get; set; } = 20.0f;
    public float StartAngle { get; set; } = -90.0f;
    public float StrokeWidth { get; set; } = 200.0f;

    private SKPoint _center;
    private SKRect _drawRect;
    private SKImageInfo _info;
    private SKPoint _touchPoint;
    
    public RadialDial()
    {
        IgnorePixelScaling = false;
        EnableTouchEvents = true;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        var canvas = e.Surface.Canvas;
        canvas.Clear();

        _info = e.Info;

        Size = Math.Min(_info.Size.Width, _info.Size.Height);

        //offsets are used to always center the dial inside the canvas and move the stroke inwards only
        var horizontalOffset = StrokeWidth / 2 + InternalPadding;
        var verticalOffset = StrokeWidth / 2 + InternalPadding;

        //setup the rectangle which we will draw in and the center point of the dial
        _drawRect = new SKRect(horizontalOffset, verticalOffset, Size - horizontalOffset, Size - verticalOffset);
        _center = new SKPoint(_drawRect.MidX, _drawRect.MidY);

        //calculate the angle of the touch input
        var touchAngle = Utils
            .PointOnCircle(_touchPoint, _center, _drawRect.Width / 2)
            .ToAngle(_center);

        //calculate the sweepAngle and map it to the 0..360 range
        var sweepAngle = (touchAngle + StartAngle).MapTo360();

        using (var path = new SKPath())
        {
            path.AddArc(_drawRect, StartAngle, sweepAngle);
            canvas.DrawPath(path, new SKPaint
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
}