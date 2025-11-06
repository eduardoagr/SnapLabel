namespace SnapLabel.Helpers;

/// <summary>
/// Represents text alignment modes for printing.
/// </summary>
public enum TextAlign {
    Left,
    Center,
    Right
}

/// <summary>
/// Provides methods for rendering text, images, and QR codes, 
/// and sending them as printable bitmaps to a BLE thermal printer.
/// </summary>
public static class Printer {

    // 🧾 Paper and font settings
    const int PaperWidth = 384;
    const float FontSize = 20f;
    const float LineSpacing = 8f;
    const int Padding = 20;

    #region 🖨️ Print Text
    // --------------------------------------------------------------------
    // Handles styled text rendering (with tags for alignment and style)
    // Converts the text into a bitmap and sends it to the printer.
    // --------------------------------------------------------------------

    /// <summary>
    /// Renders styled text with alignment, bold, italic, and underline options,
    /// converts it into a bitmap image, and sends it to a BLE thermal printer.
    /// </summary>
    /// <param name="peripheral">The connected BLE peripheral (printer).</param>
    /// <param name="text">The text content with optional tags like &lt;B&gt;, &lt;I&gt;, &lt;U&gt;, &lt;C&gt;, &lt;R&gt;, etc.</param>
    /// <param name="defaultAlignment">Optional default text alignment (left by default).</param>
    /// <returns>True if printed successfully; otherwise, false.</returns>
    public static async Task<bool> PrintTextAsync(IPeripheral peripheral, string text, TextAlign defaultAlignment = TextAlign.Left) {
        var baseTypeface = SKTypeface.Default;
        float maxTextWidth = PaperWidth - Padding * 2;

        // Parsed lines with formatting data
        var parsed = new List<(string Text, TextAlign Align, bool Bold, bool Italic, bool Underline)>();

        // --- Step 1: Parse text line-by-line and detect inline tags ---
        foreach(var raw in text.Split('\n')) {
            if(string.IsNullOrWhiteSpace(raw)) {
                parsed.Add(("", defaultAlignment, false, false, false));
                continue;
            }

            var line = raw.Trim();
            var align = defaultAlignment;

            // Alignment tags
            if(line.StartsWith("<C>")) { align = TextAlign.Center; line = line[3..]; }
            else if(line.StartsWith("<R>")) { align = TextAlign.Right; line = line[3..]; }
            else if(line.StartsWith("<L>")) { align = TextAlign.Left; line = line[3..]; }

            // Style tags
            bool bold = line.Contains("<B>");
            bool italic = line.Contains("<I>");
            bool underline = line.Contains("<U>");

            // Clean up style tags
            line = line
                .Replace("<B>", "").Replace("</B>", "")
                .Replace("<I>", "").Replace("</I>", "")
                .Replace("<U>", "").Replace("</U>", "")
                .Replace("<C>", "").Replace("</C>", "")
                .Replace("<R>", "").Replace("</R>", "")
                .Replace("<L>", "").Replace("</L>", "");

            // --- Step 2: Word wrapping based on paper width ---
            using var measureFont = new SKFont(baseTypeface, FontSize);
            var words = line.Split(' ');
            var current = "";
            foreach(var w in words) {
                var test = string.IsNullOrEmpty(current) ? w : $"{current} {w}";
                if(measureFont.MeasureText(test) > maxTextWidth) {
                    if(!string.IsNullOrEmpty(current))
                        parsed.Add((current, align, bold, italic, underline));
                    current = w;
                }
                else
                    current = test;
            }
            if(!string.IsNullOrEmpty(current))
                parsed.Add((current, align, bold, italic, underline));
        }

        // --- Step 3: Render text onto a bitmap ---
        var lineHeight = FontSize + LineSpacing;
        var totalHeight = (int)(parsed.Count * lineHeight + Padding * 2);
        if(totalHeight < FontSize + Padding * 2)
            totalHeight = (int)(FontSize + Padding * 2);

        var bmp = new SKBitmap(PaperWidth, totalHeight);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.White);

        float y = Padding + FontSize;
        foreach(var (txt, align, bold, italic, underline) in parsed) {
            // Apply font styles
            SKFontStyle style = SKFontStyle.Normal;
            if(bold && italic)
                style = SKFontStyle.BoldItalic;
            else if(bold)
                style = SKFontStyle.Bold;
            else if(italic)
                style = SKFontStyle.Italic;

            using var tf = SKTypeface.FromFamilyName(null, style);
            using var font = new SKFont(tf, FontSize);
            using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

            // Alignment offset
            var textWidth = font.MeasureText(txt);
            float x = align switch {
                TextAlign.Center => (PaperWidth - textWidth) / 2f,
                TextAlign.Right => PaperWidth - textWidth - Padding,
                _ => Padding
            };

            canvas.DrawText(txt, x, y, font, paint);

            // Optional underline
            if(underline && !string.IsNullOrEmpty(txt)) {
                float underlineY = y + 2;
                canvas.DrawLine(x, underlineY, x + textWidth, underlineY, paint);
            }

            y += lineHeight;
        }

        // --- Step 4: Send bitmap to printer ---
        var result = await GetWritablePrinterAsync(peripheral);
        if(result == null || result.Value.characteristic == null)
            return false;

        var (characteristic, service) = result.Value;
        return await SendBitmapAsync(peripheral, service.Uuid, characteristic!, bmp);
    }
    #endregion

    #region 📦 Print QR Code
    // --------------------------------------------------------------------
    // Generates a QR code and prints it centered on the thermal printer.
    // --------------------------------------------------------------------

    /// <summary>
    /// Generates and prints a centered QR code for the given text content.
    /// </summary>
    public static async Task<bool> PrintQrAsync(IPeripheral peripheral, byte[] qrBytes) {
        var result = await GetWritablePrinterAsync(peripheral);
        if(result == null || result.Value.characteristic == null)
            return false;

        var (characteristic, service) = result.Value;


        using var stream = new MemoryStream(qrBytes);
        using var qr = SKBitmap.Decode(stream);
        if(qr == null)
            return false;

        // Resize and center
        int qrSize = Math.Min(qr.Width, PaperWidth - 40);
        var qrSampling = new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None);
        var resized = qr.Resize(new SKImageInfo(qrSize, qrSize), qrSampling);
        if(resized == null)
            return false;

        int finalHeight = qrSize + 20;
        var bmp = new SKBitmap(PaperWidth, finalHeight);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(resized, (PaperWidth - qrSize) / 2, 10);

        return await SendBitmapAsync(peripheral, service.Uuid, characteristic!, bmp);
    }
    #endregion

    #region 🖼️ Print Bitmap
    /// <summary>
    /// Prints an existing bitmap, automatically centered on the thermal paper.
    /// </summary>
    public static async Task<bool> PrintBitmapAsync(IPeripheral peripheral, SKBitmap original) {
        var result = await GetWritablePrinterAsync(peripheral);
        if(result == null || result.Value.characteristic == null)
            return false;

        var (characteristic, service) = result.Value;

        // Center bitmap vertically and horizontally
        int paddingTop = 10;
        int paddingBottom = 10;
        int finalHeight = original.Height + paddingTop + paddingBottom;

        var centered = new SKBitmap(PaperWidth, finalHeight);
        using var canvas = new SKCanvas(centered);
        canvas.Clear(SKColors.White);

        int x = (PaperWidth - original.Width) / 2;
        canvas.DrawBitmap(original, x, paddingTop);

        return await SendBitmapAsync(peripheral, service.Uuid, characteristic!, centered);
    }
    #endregion

    #region ⚙️ Bitmap Conversion & BLE Write Helpers

    /// <summary>
    /// Converts a bitmap to ESC/POS raster data and sends it to the printer in BLE chunks.
    /// </summary>
    private static async Task<bool> SendBitmapAsync(IPeripheral peripheral, string serviceUuid, BleCharacteristicInfo characteristic, SKBitmap bitmap) {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var escInit = new byte[] { 0x1B, 0x40 }; // ESC @ = reset printer
        var mono = ToMono(bitmap);
        var raster = BuildRaster(mono, bitmap.Width, bitmap.Height);
        var data = escInit.Concat(raster).ToArray();

        var withResponse = characteristic.Properties.HasFlag(CharacteristicProperties.Write);

        // BLE writes: 20-byte chunks
        foreach(var chunk in Chunk(data, 20)) {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await peripheral.WriteCharacteristicAsync(serviceUuid, characteristic.Uuid, chunk, withResponse, cts.Token);
        }

        return true;
    }

    /// <summary>
    /// Converts an image to monochrome 1-bit pixel data (black or white).
    /// </summary>
    private static byte[] ToMono(SKBitmap bmp) {
        int wBytes = (bmp.Width + 7) / 8;
        int h = bmp.Height;
        var result = new byte[wBytes * h];
        int index = 0;

        for(int y = 0; y < h; y++) {
            for(int xByte = 0; xByte < wBytes; xByte++) {
                byte b = 0;
                for(int bit = 0; bit < 8; bit++) {
                    int x = xByte * 8 + bit;
                    if(x < bmp.Width) {
                        var c = bmp.GetPixel(x, y);
                        var gray = c.Red * 0.3 + c.Green * 0.59 + c.Blue * 0.11;
                        if(gray < 128)
                            b |= (byte)(1 << (7 - bit)); // Black pixel
                    }
                }
                result[index++] = b;
            }
        }
        return result;
    }

    /// <summary>
    /// Builds proper ESC/POS raster command bytes for the given monochrome bitmap.
    /// </summary>
    private static byte[] BuildRaster(byte[] data, int widthPixels, int heightPixels) {
        int widthBytes = (widthPixels + 7) / 8;
        byte wLow = (byte)(widthBytes & 0xFF);
        byte wHigh = (byte)((widthBytes >> 8) & 0xFF);
        byte hLow = (byte)(heightPixels & 0xFF);
        byte hHigh = (byte)((heightPixels >> 8) & 0xFF);

        return new byte[]
        {
            0x1D, 0x76, 0x30, 0x00, // ESC/POS raster bit image command
            wLow, wHigh,
            hLow, hHigh
        }.Concat(data).ToArray();
    }

    /// <summary>
    /// Splits byte data into equal-sized chunks for BLE transfer.
    /// </summary>
    private static IEnumerable<byte[]> Chunk(byte[] data, int size) {
        for(int i = 0; i < data.Length; i += size)
            yield return data.Skip(i).Take(size).ToArray();
    }

    /// <summary>
    /// Discovers the BLE printer service and returns the writable characteristic.
    /// </summary>
    public static async Task<(BleCharacteristicInfo? characteristic, BleServiceInfo service)?> GetWritablePrinterAsync(IPeripheral peripheral) {
        var services = await peripheral.GetServicesAsync();
        var printerService = services.FirstOrDefault(s => s.Uuid.StartsWith("0000ff"));
        if(printerService == null)
            return null;

        var characteristics = await peripheral.GetCharacteristicsAsync(printerService.Uuid);
        var writableCharacteristic = characteristics.FirstOrDefault(ch =>
            ch.Properties.HasFlag(CharacteristicProperties.Write) ||
            ch.Properties.HasFlag(CharacteristicProperties.WriteWithoutResponse));

        return (writableCharacteristic, printerService);
    }

    #endregion
}
