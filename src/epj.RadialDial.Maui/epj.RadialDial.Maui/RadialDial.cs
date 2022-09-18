using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace epj.RadialDial.Maui;

public class RadialDial : SKCanvasView
{
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var info = e.Info;
        var smallest = Math.Min(info.Width, info.Height);

        var x = info.Width * 0.5f - smallest * 0.5f;
        var y = info.Height * 0.5f - smallest * 0.5f;

        var arcRect = new SKRect((float)x, (float)y, (float)(x + smallest), (float)(y + smallest));

        canvas.DrawArc(arcRect, 0.0f, 270.0f, true, new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = Colors.Red.ToSKColor(),
            StrokeWidth = 5,
            IsAntialias = true
        });

        canvas.Translate(arcRect.Width * 0.5f, arcRect.MidY - arcRect.Height * 0.5f);
    }
}