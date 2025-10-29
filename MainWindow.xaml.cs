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
    private Rule _rule;
    Array256 rng_b;
    Array256 rng_s;

    public MainWindow()
    {
        rng_b = RandomArray256(0.1);
        rng_s = RandomArray256(0.1);
        rng_b[0] = false;
        rng_s[0] = false;
        _rule = Parser.Parse("B3/S23/p=0,1") ?? Parser.Parse("B3/S23/p=0,1")!;
        InitializeComponent();
        _bitmap = new((int)image.Width, (int)image.Height, 96, 100, PixelFormats.Bgr32, null);
        image.Source = _bitmap;
        _fieldType = _rule.Generations > 1 ? FieldType.Int : FieldType.Bool;
        var p = _rule.StartDensity;
        _error = Test.Run();
        switch (_fieldType)
        {
            case FieldType.Bool:
                for (int y = 0; y < 500; y++)
                {
                    for (int x = 0; x < 500; x++)
                    {
                        _bField[x, y] = _rng.NextDouble() < p;
                    }
                }
                break;
            case FieldType.Int:
                for (int y = 0; y < 500; y++)
                {
                    for (int x = 0; x < 500; x++)
                    {
                        // _iField[x, y] = _rng.Next(3);
                        _iField[x, y] = _rng.NextDouble() < p ? 1 : 0;
                    }
                }
                break;
        }
        // _colors = new Color[_rule.Generations + 1];
        // for (int i = 0; i < _rule.Generations + 1; i++)
        // {
        //     _colors[i] = FromRgb(0, 0, 0);
        // }
        _colors = new Color[] { FromRgb(220, 220, 220), FromRgb(0, 0, 0), FromRgb(0, 0, 220) };
        // _iField[499, 499] = 1;
        // _iField[499, 500] = 1;
        // _iField[500, 499] = 1;
        // _iField[500, 500] = 1;
        _timer.Interval = TimeSpan.FromSeconds(0.00001);
        _timer.Tick += Tick;
        _timer.Start();
    }

    public void Next(Rule rule, int startX, int endX, int startY, int endY)
    {
        if (rule.Generations > 1)
        {
            NextLL(startX, endX, startY, endY, rule.Birth, rule.Survival, rule.Generations);
        }
        if (rule.Generations <= 1)
        {
            NextLL(startX, endX, startY, endY, rule.Birth, rule.Survival);
        }
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
    private Array256 RandomArray256(double p)
    {
        var result = new Array256(new BitArray(256));
        for (int i = 0; i < 256; i++)
        {
            result[i] = _rng.NextDouble() < p;
        }
        return result;
    }
    private unsafe void Tick(object? sender, EventArgs e)
    {
        Next(_rule, 0, 500, 0, 500);
        _bitmap.Lock();
        if (_fieldType == FieldType.Bool)
        {
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
        }
        if (_fieldType == FieldType.Int)
        {
            for (int y = 0; y < _bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < _bitmap.PixelWidth; x++)
                {
                    var color = _colors[_iField[x / 2, y / 2]];
                    var ptr = _bitmap.BackBuffer + x * 4 + _bitmap.BackBufferStride * y;
                    unsafe
                    {
                        *((int*)ptr) = (color.R << 16) | (color.G << 8) | (color.B);
                    }
                }
            }
        }
        //this.Title = _error ?? this.Title;
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
    public void NextLL(int startX, int endX, int startY, int endY, Array256 birth, Array256 survival)
    {
        int F(int x, int y)
        {
            return _bField[x, y] ? 1 : 0;
        }
        var newField = new bool[_bField.GetLength(0), _bField.GetLength(1)];
        for (int y = startY + 1; y < endY - 1; y++)
        {
            for (int x = startX + 1; x < endX - 1; x++)
            {
                var c = (F(x - 1, y - 1)) + (F(x, y - 1) << 1) + (F(x + 1, y - 1) << 2) + (F(x - 1, y) << 3) + (F(x + 1, y) << 4) + (F(x - 1, y + 1) << 5) + (F(x, y + 1) << 6) + (F(x + 1, y + 1) << 7);
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
    public void NextLL(int startX, int endX, int startY, int endY, Array256 birth, Array256 survival, int generations)
    {
        int F(int x, int y)
        {
            return _iField[x, y] == 1 ? 1 : 0;
        }
        var newField = new int[_bField.GetLength(0), _bField.GetLength(1)];
        for (int y = startY + 1; y < endY - 1; y++)
        {
            for (int x = startX + 1; x < endX - 1; x++)
            {
                var c = F(x - 1, y - 1) + F(x, y - 1) + F(x + 1, y - 1) + F(x - 1, y) + F(x + 1, y) + F(x - 1, y + 1) + F(x, y + 1) + F(x + 1, y + 1);
                if ((_iField[x, y] >= generations) || ((_iField[x, y] == 0 && !birth[c])))
                {
                    newField[x, y] = 0;
                    continue;
                }
                if ((birth[c] && _iField[x, y] == 0) || (survival[c] && _iField[x, y] == 1))
                {
                    newField[x, y] = 1;
                    continue;
                }
                if (!survival[c] && _iField[x, y] == 1)
                {
                    newField[x, y] = 2;
                    continue;
                }
                if (_iField[x, y] > 1)
                {
                    newField[x, y] = _iField[x, y] + 1;
                }
            }
        }
        _iField = newField;
    }
    public void NextLLR(int startX, int endX, int startY, int endY, BitArray birth, BitArray survival, int radius = 1)
    {
        var newField = new bool[_bField.GetLength(0), _bField.GetLength(1)];
        var horizontalSum = new bool[_bField.GetLength(0) - radius, _bField.GetLength(1)];
        for (int y = 0; y < horizontalSum.GetLength(1); y++)
        {
            var startSum = 0;
            for (int i = 0; i < 2 * radius + 1; i++)
            {
                startSum += _bField[i, y] ? 1 : 0;
            }
            for (int x = 0; x < horizontalSum.GetLength(0) - radius; x++)
            {
                startSum = startSum - (_bField[x, y] ? 1 : 0) + (_bField[x + 2 * radius + 1, y] ? 1 : 0);
            }
        }
        var verticalSum = new bool[horizontalSum.GetLength(0), horizontalSum.GetLength(1) - radius];
        for (int x = 0; x < verticalSum.GetLength(1); x++)
        {
            var startSum = 0;
            for (int i = 0; i < 2 * radius + 1; i++)
            {
                startSum += _bField[x, i] ? 1 : 0;
            }
            for (int y = 0; y < verticalSum.GetLength(0) - radius; y++)
            {
                startSum = startSum - (_bField[x, y] ? 1 : 0) + (_bField[x + 2 * radius + 1, y] ? 1 : 0);
            }
        }
        _bField = verticalSum;
    }
    public void NextLLR(int startX, int endX, int startY, int endY, BitArray birth, BitArray survival, int generations, int radius = 1)
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
                if ((_iField[x, y] >= generations) || ((_iField[x, y] == 0 && !birth[c])))
                {
                    newField[x, y] = 0;
                    continue;
                }
                if ((birth[c] && _iField[x, y] == 0) || (survival[c] && _iField[x, y] == 1))
                {
                    newField[x, y] = 1;
                    continue;
                }
                if (!survival[c] && _iField[x, y] == 1)
                {
                    newField[x, y] = 2;
                    continue;
                }
                if (_iField[x, y] > 1)
                {
                    newField[x, y] = _iField[x, y] + 1;
                }
            }
        }
        _iField = newField;
    }
    public void NextCyclic(int startX, int endX, int startY, int endY, BitArray threshold, int states)
    {

        var newField = new int[1000, 1000];
        for (int x = startX + 1; x < endX - 1; x++)
        {
            for (int y = startY + 1; y < endY - 1; y++)
            {
                var next = (_iField[x, y] + 1) % states;
                int F(int X, int Y)
                {
                    return _iField[X, Y] == (_iField[x, y] + 1) % states ? 1 : 0;
                }
                var c = F(x - 1, y - 1) + F(x, y - 1) + F(x + 1, y - 1) + F(x - 1, y) + F(x + 1, y) + F(x - 1, y + 1) + F(x, y + 1) + F(x + 1, y + 1);
                if (threshold[c])
                {
                    _iField[x, y] = next;
                }
            }
        }
        _iField = newField;
    }
}