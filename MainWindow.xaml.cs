using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Input;
using System.IO;

namespace WpfApp1;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new();
    private readonly WriteableBitmap _bitmap;
    private readonly Random _rng = new();
    private int _f;
    private bool[,] _field = new bool[1000, 1000];
    public MainWindow()
    {
        InitializeComponent();
        _bitmap = new((int)image.Width, (int)image.Height, 96, 100, PixelFormats.Bgr32, null);
        image.Source = _bitmap;
        var p = 0.1;
        for (int y = 0; y < 1000; y++)
        {
            for (int x = 0; x < 1000; x++)
            {
                _field[x, y] = _rng.NextDouble() < p;
            }
        }
        _timer.Interval = TimeSpan.FromSeconds(0.00001);
        _timer.Tick += Tick;
        _timer.Start();
    }

    private Point Interpolate(Point a, Point b, double t)
    {
        return new(a.X * t + b.X * (1 - t), a.Y * t + b.Y * (1 - t));
    }
    private Point Normalize(Point point, double dist)
    {
        var coeff = Math.Sqrt(point.X * point.X + point.Y * point.Y) / dist;
        return new Point(point.X * dist, point.Y * dist);
    }
    private unsafe void Tick(object? sender, EventArgs e)
    {
        Next();
        _bitmap.Lock();
        for (int y = 0; y < _bitmap.PixelHeight; y++)
        {
            for (int x = 0; x < _bitmap.PixelWidth; x++)
            {
                var c = _field[x / 4, y / 4] ? 255 : 0;
                var color = FromRgb(c, c, c);
                var ptr = _bitmap.BackBuffer + x * 4 + _bitmap.BackBufferStride * y;
                unsafe
                {
                    *((int*)ptr) = (color.R << 16) | (color.G << 8) | (color.B);
                }
            }
        }
        _f++;
        _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
        _bitmap.Unlock();
    }
    public Color HsvToRgb(int h, byte s, byte v)
    {
        var result = new Color();
        var hue = h % 360;
        var hv = (hue % 60) * 255 / 60;
        var a = 0;
        var b = hv;
        var c = 255 - hv;
        var d = 255;
        void DoubleInterval(int min, int max, int r, int g, int b)
        {
            if (min <= hue && max > hue)
            {
                result = Normalize(FromRgb(r, g, b), v);
            }
        }
        DoubleInterval(0, 60, d, b, a);
        DoubleInterval(60, 120, c, d, a);
        DoubleInterval(120, 180, a, d, b);
        DoubleInterval(180, 240, a, c, d);
        DoubleInterval(240, 300, b, a, d);
        DoubleInterval(300, 360, d, a, c);
        return Interpolation(result, Color.FromRgb(v, v, v), s);
    }
    public Color Interpolation(Color a, Color b, byte c)
    {
        return FromRgb((a.R * c + b.R * (255 - c)) / 255, (a.G * c + b.G * (255 - c)) / 255, (a.B * c + b.B * (255 - c)) / 255);
    }
    private Color FromRgb(int r, int g, int b)
    {
        return Color.FromRgb(((byte)(r & 255)), ((byte)(g & 255)), ((byte)(b & 255)));
    }
    public Color Normalize(Color color, byte lightness)
    {
        var r = color.R;
        var g = color.G;
        var b = color.B;
        var max = Math.Max(r, Math.Max(g, b));
        if (max == 0)
        {
            return Color.FromRgb(lightness, lightness, lightness);
        }
        var normalizer = (double)lightness / max;
        return Color.FromRgb((byte)(r * normalizer), ((byte)(g * normalizer)), ((byte)(b * normalizer)));

    }
    public void Next()
    {
        var newField = new bool[1000, 1000];
        int F(int x, int y)
        {
            return _field[x, y] ? 1 : 0;
        }
        for (int x = 1; x < 999; x++)
        {
            for (int y = 1; y < 999; y++)
            {
                var c = F(x - 1, y - 1) + F(x, y - 1) * 2 + F(x + 1, y - 1) + F(x - 1, y) + F(x + 1, y) + F(x - 1, y + 1) + F(x, y + 1) + F(x + 1, y + 1);
                if(_field[x, y] & (c == 2) | (c == 3))
                {
                    newField[x, y] = true;
                    continue;
                }
                if(c == 3)
                {
                    newField[x, y] = true;
                }
            }
        }
        _field = newField;
    }
}