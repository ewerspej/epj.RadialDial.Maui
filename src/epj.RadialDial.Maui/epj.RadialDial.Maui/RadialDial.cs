﻿using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace epj.RadialDial.Maui;

public class RadialDial : SKCanvasView
{
    public int Size { get; set; }
    public float InternalPadding { get; set; } = 20.0f;
    public float StartAngle { get; set; } = 0.0f;
    public float SweepAngle { get; set; } = 320.0f;
    public float StrokeWidth { get; set; } = 200.0f;

    private SKPoint _center;
    private SKRect _drawRect;
    private SKImageInfo _info;
    private float _adjustedStartAngle;

    public RadialDial()
    {
        IgnorePixelScaling = false;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        var canvas = e.Surface.Canvas;
        canvas.Clear();

        _info = e.Info;

        Size = Math.Min(_info.Size.Width, _info.Size.Height);

        //offsets are used to always center the dial inside the canvas
        //var horizontalOffset = (_info.Size.Width - Size) / 2;
        //var verticalOffset = (_info.Size.Height - Size) / 2;
        var horizontalOffset = StrokeWidth / 2 + InternalPadding;
        var verticalOffset = StrokeWidth / 2 + InternalPadding;

        //setup the rectangle which we will draw in and the center point of the dial
        _drawRect = new SKRect(horizontalOffset, verticalOffset, Size - horizontalOffset, Size - verticalOffset);
        _center = new SKPoint(_drawRect.MidX, _drawRect.MidY);

        //the coordinate system of SkiaSharp starts with 0 degrees at 3 o'clock (polar coordinates),
        //but we want 0 degrees at 0 o'clock, so we rotate everything by -90 degrees.
        _adjustedStartAngle = StartAngle - 90.0f;
        
        using (var path = new SKPath())
        {
            path.AddArc(_drawRect, _adjustedStartAngle, SweepAngle);
            canvas.DrawPath(path, new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Colors.Red.ToSKColor(),
                StrokeWidth = StrokeWidth,
                IsAntialias = true
            });
        }
    }
}