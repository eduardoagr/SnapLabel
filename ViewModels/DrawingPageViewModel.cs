using CommunityToolkit.Maui.Core.Views;

using System.Numerics;

namespace SnapLabel.ViewModels;

public partial class DrawingPageViewModel : ObservableObject {

    readonly IBleManager? ble;
    readonly IShellService? shellService;

    public IEnumerable<IPeripheral>? ConnectedPeripheral { get; private set; }

    [ObservableProperty]
    public partial bool IcanPrint { get; set; } = false;

    [ObservableProperty]
    public partial double CanvasWidth { get; set; } = 300;

    [ObservableProperty]
    public partial double CanvasHeight { get; set; } = 300;

    public ObservableCollection<IDrawingLine> Lines { get; set; } = [];

    public DrawingPageViewModel(IBleManager ble, IShellService _shellService) {

        this.ble = ble;
        shellService = _shellService;

        WeakReferenceMessenger.Default.Register<string>(this, (r, message) => {

            if(message == "IsConnected" || message == "IsDisconnected")
                RefreshBluetoothStatus();
        });
    }

    public void RefreshBluetoothStatus() {

        ConnectedPeripheral = ble?.GetConnectedPeripherals();
        IcanPrint = ConnectedPeripheral?.Any() == true;
    }

    [RelayCommand]
    private async Task Print() {

        await using var stream = await DrawingViewService.GetImageStream(ImageLineOptions.FullCanvas(Lines,
            new Size(300, 300),
        null, new Size(CanvasWidth, CanvasHeight)));

        stream.Seek(0, SeekOrigin.Begin);
        using var bitmap = SKBitmap.Decode(stream);

        var peripheral = ConnectedPeripheral!.First();

        await Printer.PrintBitmapAsync(peripheral, bitmap);
    }

    [RelayCommand]
    private void ClearCanvas() {
        Lines.Clear();
    }

    [RelayCommand]
    private void HandleDrawingCompleted(IDrawingLine line) {
        var points = line.Points;

        if(LooksLikeSquare(points)) {
            ReplaceLastLineWith(CreateSquare(CenterOf(points)));
        }
        else if(LooksLikeCircle(points)) {
            ReplaceLastLineWith(CreateCircle(CenterOf(points)));
        }
    }

    private void ReplaceLastLineWith(IDrawingLine newShape) {
        if(Lines.Count > 0) {
            Lines.RemoveAt(Lines.Count - 1);
            Lines.Add(newShape);
        }
    }

    private PointF CenterOf(IList<PointF> points) {
        var avgX = points.Average(p => p.X);
        var avgY = points.Average(p => p.Y);
        return new PointF((float)avgX, (float)avgY);
    }

    private float Distance(PointF a, PointF b) {
        return Vector2.Distance(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y));
    }

    private bool IsClosed(IList<PointF> points) {
        if(points.Count < 3)
            return false;
        return Distance(points.First(), points.Last()) < 20;
    }

    private (float Width, float Height) GetBoundingBox(IList<PointF> points) {
        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);
        return (maxX - minX, maxY - minY);
    }

    private bool LooksLikeSquare(IList<PointF> points) {
        if(!IsClosed(points))
            return false;
        var (width, height) = GetBoundingBox(points);
        var ratio = width / height;
        return ratio > 0.8f && ratio < 1.2f;
    }

    private bool LooksLikeCircle(IList<PointF> points) {
        if(!IsClosed(points) || points.Count < 10)
            return false;

        var center = CenterOf(points);
        var radii = points.Select(p => Distance(center, p)).ToList();
        var avg = radii.Average();
        var variance = radii.Average(r => Math.Abs(r - avg));
        return variance < 15;
    }

    private IDrawingLine CreateSquare(PointF center) {
        float size = 100;
        float half = size / 2;

        return new DrawingLine {
            Points =
            [
                new(center.X - half, center.Y - half),
                new(center.X + half, center.Y - half),
                new(center.X + half, center.Y + half),
                new(center.X - half, center.Y + half),
                new(center.X - half, center.Y - half)
            ],
            LineColor = Colors.Black,
            LineWidth = 2
        };
    }

    private IDrawingLine CreateCircle(PointF center) {
        float radius = 50;
        int segments = 36;
        var points = new ObservableCollection<PointF>();

        for(int i = 0; i <= segments; i++) {
            double angle = 2 * Math.PI * i / segments;
            float x = center.X + radius * (float)Math.Cos(angle);
            float y = center.Y + radius * (float)Math.Sin(angle);
            points.Add(new PointF(x, y));
        }

        return new DrawingLine {
            Points = points,
            LineColor = Colors.Black,
            LineWidth = 2
        };
    }



}