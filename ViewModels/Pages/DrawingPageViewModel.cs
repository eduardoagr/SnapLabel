namespace SnapLabel.ViewModels;
/// <summary>
/// ViewModel for the Drawing Page.
/// Manages canvas drawing, shape recognition, and printing through a BLE thermal printer.
/// </summary>
public partial class DrawingPageViewModel : ObservableObject {

    #region 🔧 Fields & Dependencies

    private readonly IBleManager? ble;

    #endregion

    #region 💡 Observable Properties

    /// <summary>
    /// Currently connected Bluetooth peripherals (printers).
    /// </summary>
    public IEnumerable<IPeripheral>? ConnectedPeripheral { get; private set; }

    /// <summary>
    /// Whether printing is available (true when a printer is connected).
    /// </summary>
    [ObservableProperty]
    public partial bool IcanPrint { get; set; } = false;

    /// <summary>
    /// Canvas drawing width (in device-independent units).
    /// </summary>
    [ObservableProperty]
    public partial double CanvasWidth { get; set; } = 300;

    /// <summary>
    /// Canvas drawing height (in device-independent units).
    /// </summary>
    [ObservableProperty]
    public partial double CanvasHeight { get; set; } = 300;

    /// <summary>
    /// The collection of all drawn lines or shapes on the canvas.
    /// </summary>
    public ObservableCollection<IDrawingLine> Lines { get; set; } = [];

    #endregion

    #region 🧭 Constructor & Initialization

    /// <summary>
    /// Initializes a new instance of the <see cref="DrawingPageViewModel"/> class,
    /// sets up Bluetooth tracking and message subscriptions.
    /// </summary>
    public DrawingPageViewModel(IBleManager ble, IShellService _shellService) {
        this.ble = ble;

        // Listen for BLE connection changes and refresh UI state accordingly
        WeakReferenceMessenger.Default.Register<string>(this, (r, message) => {
            if(message == "IsConnected" || message == "IsDisconnected")
                RefreshBluetoothStatus();
        });
    }

    /// <summary>
    /// Refreshes the Bluetooth connection status and updates the print availability flag.
    /// </summary>
    public void RefreshBluetoothStatus() {
        ConnectedPeripheral = ble?.GetConnectedPeripherals();
        IcanPrint = ConnectedPeripheral?.Any() == true;
    }

    #endregion

    #region 🖨️ Commands — Printing & Canvas Actions

    /// <summary>
    /// Converts the current canvas drawing into an image and sends it to the connected BLE printer.
    /// </summary>
    [RelayCommand]
    private async Task Print() {
        // Generate bitmap from the drawing lines
        await using var stream = await DrawingViewService.GetImageStream(
            ImageLineOptions.FullCanvas(
                Lines,
                new Size(300, 300),
                null,
                new Size(CanvasWidth, CanvasHeight))
        );

        stream.Seek(0, SeekOrigin.Begin);
        using var bitmap = SKBitmap.Decode(stream);

        // Pick the first connected printer and send the image
        var peripheral = ConnectedPeripheral!.First();
        await Printer.PrintBitmapAsync(peripheral, bitmap);
    }

    /// <summary>
    /// Clears the current canvas by removing all lines and shapes.
    /// </summary>
    [RelayCommand]
    private void ClearCanvas() {
        Lines.Clear();
    }

    /// <summary>
    /// Triggered when a drawing stroke is completed.
    /// Detects if it resembles a simple geometric shape (square or circle)
    /// and replaces it with a perfect version of that shape.
    /// </summary>
    [RelayCommand]
    private void HandleDrawingCompleted(IDrawingLine line) {
        var points = line.Points;

        if(LooksLikeSquare(points))
            ReplaceLastLineWith(CreateSquare(CenterOf(points)));
        else if(LooksLikeCircle(points))
            ReplaceLastLineWith(CreateCircle(CenterOf(points)));
    }

    #endregion

    #region 🧩 Shape Recognition & Replacement

    /// <summary>
    /// Replaces the last drawn line in the collection with a new geometric shape.
    /// </summary>
    private void ReplaceLastLineWith(IDrawingLine newShape) {
        if(Lines.Count > 0) {
            Lines.RemoveAt(Lines.Count - 1);
            Lines.Add(newShape);
        }
    }

    /// <summary>
    /// Determines whether a stroke is a closed path (start and end points close together).
    /// </summary>
    private bool IsClosed(IList<PointF> points) {
        if(points.Count < 3)
            return false;
        return Distance(points.First(), points.Last()) < 20;
    }

    /// <summary>
    /// Determines if the drawn shape roughly forms a square.
    /// </summary>
    private bool LooksLikeSquare(IList<PointF> points) {
        if(!IsClosed(points))
            return false;

        var (width, height) = GetBoundingBox(points);
        var ratio = width / height;

        // Near-equal width/height suggests a square
        return ratio > 0.8f && ratio < 1.2f;
    }

    /// <summary>
    /// Determines if the drawn shape roughly forms a circle
    /// based on variance of radii from its center.
    /// </summary>
    private bool LooksLikeCircle(IList<PointF> points) {
        if(!IsClosed(points) || points.Count < 10)
            return false;

        var center = CenterOf(points);
        var radii = points.Select(p => Distance(center, p)).ToList();
        var avg = radii.Average();
        var variance = radii.Average(r => Math.Abs(r - avg));

        return variance < 15;
    }

    #endregion

    #region 📐 Geometry Helpers

    /// <summary>
    /// Gets the geometric center (average) of a collection of points.
    /// </summary>
    private PointF CenterOf(IList<PointF> points) {
        var avgX = points.Average(p => p.X);
        var avgY = points.Average(p => p.Y);
        return new PointF((float)avgX, (float)avgY);
    }

    /// <summary>
    /// Computes Euclidean distance between two points.
    /// </summary>
    private float Distance(PointF a, PointF b) {
        return Vector2.Distance(new Vector2(a.X, a.Y), new Vector2(b.X, b.Y));
    }

    /// <summary>
    /// Calculates the width and height of a bounding box around the stroke.
    /// </summary>
    private (float Width, float Height) GetBoundingBox(IList<PointF> points) {
        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);
        return (maxX - minX, maxY - minY);
    }

    #endregion

    #region 🟦 Shape Creation

    /// <summary>
    /// Generates a clean square shape centered at a specified point.
    /// </summary>
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

    /// <summary>
    /// Generates a clean circular shape centered at a specified point.
    /// </summary>
    private IDrawingLine CreateCircle(PointF center) {
        float radius = 50;
        int segments = 36; // Smoothness of the circle
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

    #endregion
}
