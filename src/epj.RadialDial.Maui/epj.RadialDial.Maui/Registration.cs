namespace epj.RadialDial.Maui;

public static class Registration
{
    public static MauiAppBuilder UseRadialDial(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(h =>
        {
            h.AddHandler<RadialDial, RadialDialHandler>();
        });

        return builder;
    }
}