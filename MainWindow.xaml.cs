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
    private bool[,] _bField = new bool[1000, 1000];
    private int[,] _iField = new int[1000, 1000];
    private FieldType _fieldType;
    private Color[] _colors;
    private string? _error;
    public MainWindow()
    {
        InitializeComponent();
        _bitmap = new((int)image.Width, (int)image.Height, 96, 100, PixelFormats.Bgr32, null);
        image.Source = _bitmap;
        _fieldType = FieldType.Bool;
        var p = 0.1;
        var countOfColors = 4;
        _error = Test.Run();
        switch (_fieldType)
        {
            case FieldType.Bool:
                for (int y = 0; y < 1000; y++)
                {
                    for (int x = 0; x < 1000; x++)
                    {
                        _bField[x, y] = _rng.NextDouble() < p;
                    }
                }
                break;
            case FieldType.Int:
                for (int y = 0; y < 1000; y++)
                {
                    for (int x = 0; x < 1000; x++)
                    {
                        _iField[x, y] = _rng.Next(countOfColors);
                    }
                }
                break;
        }
        _colors = new Color[] { FromRgb(220, 220, 220), FromRgb(0, 0, 0) };

        _timer.Interval = TimeSpan.FromSeconds(0.00001);
        _timer.Tick += Tick;
        _timer.Start();
    }
    public void Next(Rule rule, int startX, int endX, int startY, int endY)
    {
        NextLL(startX, endX, startY, endY, rule.Birth, rule.Survival);
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
        Next(Parser.Parse("B45678/S5678"), 0, 500, 0, 500);
        _bitmap.Lock();
        for (int y = 0; y < _bitmap.PixelHeight; y++)
        {
            for (int x = 0; x < _bitmap.PixelWidth; x++)
            {
                var color = _colors[_bField[x / 2, y / 2] ? 1 : 0];
                var ptr = _bitmap.BackBuffer + x * 4 + _bitmap.BackBufferStride * y;
                unsafe
                {
                    *((int*)ptr) = (color.R << 16) | (color.G << 8) | (color.B);
                }
            }
        }
        this.Title = _error ?? this.Title;
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
    public void NextLL(int startX, int endX, int startY, int endY, BitArray birth, BitArray survival)
    {
        int F(int x, int y)
        {
            return _bField[(x + endX - startX) % (endX - startX), (y + endY - startY) % (endY - startY)] ? 1 : 0;
        }
        var newField = new bool[1000, 1000];
        for (int x = startX + 1; x < endX; x++)
        {
            for (int y = startY + 1; y < endY; y++)
            {
                var c = F(x - 1, y - 1) + F(x, y - 1) + F(x + 1, y - 1) + F(x - 1, y) + F(x + 1, y) + F(x - 1, y + 1) + F(x, y + 1) + F(x + 1, y + 1);
                if (_bField[x, y] & survival[c])
                {
                    newField[x, y] = true;
                }
                if (!_bField[x, y] & birth[c])
                {
                    newField[x, y] = true;
                }
            }
        }
        _bField = newField;
    }
    public void NextLL(int startX, int endX, int startY, int endY, BitArray birth, BitArray survival, int generations)
    {
        int F(int x, int y)
        {
            return _iField[x, y] == 1 ? 1 : 0;
        }
        var newField = new int[1000, 1000];
        for (int x = startX + 1; x < endX - 1; x++)
        {
            for (int y = startY + 1; y < endY - 1; y++)
            {
                var c = F(x - 1, y - 1) + F(x, y - 1) + F(x + 1, y - 1) + F(x - 1, y) + F(x + 1, y) + F(x - 1, y + 1) + F(x, y + 1) + F(x + 1, y + 1);
                if (survival[c] & _iField[x, y] == 1)
                {
                    newField[x, y] = 1;
                }
                else
                {
                    if (birth[c])
                    {
                        newField[x, y] = 1;
                    }
                }
                if (_iField[x, y] > 1)
                {
                    _iField[x, y]++;
                }
                if (!survival[c] & _iField[x, y] == 1)
                {
                    _iField[x, y] = 2;
                }
                if (_iField[x, y] == generations)
                {
                    _iField[x, y] = 0;
                }
            }
        }
        _iField = newField;
    }
}