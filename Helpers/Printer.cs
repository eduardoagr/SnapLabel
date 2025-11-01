namespace SnapLabel.Helpers;

public enum TextAlign {
    Left,
    Center,
    Right
}

public static class Printer {

    const int PaperWidth = 384; // 58mm paper
    const float FontSize = 20f;
    const float LineSpacing = 8f;
    const int Padding = 20;

    public static async Task<bool> PrintTextAsync(IPeripheral peripheral, string text, TextAlign defaultAlignment = TextAlign.Left) {

        var baseTypeface = SKTypeface.Default;
        float maxTextWidth = PaperWidth - Padding * 2;

        var parsed = new List<(string Text, TextAlign Align, bool Bold, bool Italic, bool Underline)>();
        foreach(var raw in text.Split('\n')) {
            if(string.IsNullOrWhiteSpace(raw)) {
                parsed.Add(("", defaultAlignment, false, false, false));
                continue;
            }

            var line = raw.Trim();
            var align = defaultAlignment;

            if(line.StartsWith("<C>")) { align = TextAlign.Center; line = line[3..]; }
            else if(line.StartsWith("<R>")) { align = TextAlign.Right; line = line[3..]; }
            else if(line.StartsWith("<L>")) { align = TextAlign.Left; line = line[3..]; }

            bool bold = line.Contains("<B>");
            bool italic = line.Contains("<I>");
            bool underline = line.Contains("<U>");

            line = line
                .Replace("<B>", "").Replace("</B>", "")
                .Replace("<I>", "").Replace("</I>", "")
                .Replace("<U>", "").Replace("</U>", "")
                .Replace("<C>", "").Replace("</C>", "")
                .Replace("<R>", "").Replace("</R>", "")
                .Replace("<L>", "").Replace("</L>", "");

            // Basic word wrap
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

        var lineHeight = FontSize + LineSpacing;
        var totalHeight = (int)(parsed.Count * lineHeight + Padding * 2);
        if(totalHeight < FontSize + Padding * 2)
            totalHeight = (int)(FontSize + Padding * 2);

        var bmp = new SKBitmap(PaperWidth, totalHeight);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.White);

        float y = Padding + FontSize;
        foreach(var (txt, align, bold, italic, underline) in parsed) {
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

            var textWidth = font.MeasureText(txt);
            float x = align switch {
                TextAlign.Center => (PaperWidth - textWidth) / 2f,
                TextAlign.Right => PaperWidth - textWidth - Padding,
                _ => Padding
            };

            canvas.DrawText(txt, x, y, font, paint);

            if(underline && !string.IsNullOrEmpty(txt)) {
                float underlineY = y + 2;
                canvas.DrawLine(x, underlineY, x + textWidth, underlineY, paint);
            }

            y += lineHeight;
        }

        var result = await GetWritablePrinterAsync(peripheral);
        if(result == null || result.Value.characteristic == null)
            return false;

        var (characteristic, service) = result.Value;
        return await SendBitmapAsync(peripheral, service.Uuid, characteristic!, bmp);
    }



    // ✅ Fixed QR Printing (No more cutoff!)
    public static async Task<bool> PrintQrAsync(IPeripheral peripheral, string content) {
        var result = await GetWritablePrinterAsync(peripheral);
        if(result == null || result.Value.characteristic == null)
            return false;

        var (characteristic, service) = result.Value;

        var qrGen = new QRCodeGenerator();
        var data = qrGen.CreateQrCode(content, QRCodeGenerator.ECCLevel.L);
        var qrCode = new PngByteQRCode(data);
        var pngBytes = qrCode.GetGraphic(20); // Adjust size, 20 = sharp

        using var stream = new MemoryStream(pngBytes);
        using var qr = SKBitmap.Decode(stream);
        if(qr == null)
            return false;

        int qrSize = Math.Min(qr.Width, PaperWidth - 40); // Keep it square
                                                          // Use nearest-neighbor sampling for crisp QR edges
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

    public static async Task<bool> PrintBitmapAsync(IPeripheral peripheral, SKBitmap original) {
        var result = await GetWritablePrinterAsync(peripheral);
        if(result == null || result.Value.characteristic == null)
            return false;

        var (characteristic, service) = result.Value;

        // Center horizontally on 58mm paper
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

    // ✅ Send Bitmap → ESC/POS data
    private static async Task<bool> SendBitmapAsync(IPeripheral peripheral, string serviceUuid, BleCharacteristicInfo characteristic, SKBitmap bitmap) {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var escInit = new byte[] { 0x1B, 0x40 }; // ESC @
        var mono = ToMono(bitmap);
        var raster = BuildRaster(mono, bitmap.Width, bitmap.Height);
        var data = escInit.Concat(raster).ToArray();

        var withResponse = characteristic.Properties.HasFlag(CharacteristicProperties.Write);

        foreach(var chunk in Chunk(data, 20)) {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await peripheral.WriteCharacteristicAsync(serviceUuid, characteristic.Uuid, chunk, withResponse, cts.Token);
        }

        return true;
    }

    // ✅ Convert to 1-bit pixels
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
                            b |= (byte)(1 << (7 - bit)); // black
                    }
                }
                result[index++] = b;
            }
        }
        return result;
    }

    // ✅ Correct ESC/POS raster formatting
    private static byte[] BuildRaster(byte[] data, int widthPixels, int heightPixels) {
        int widthBytes = (widthPixels + 7) / 8;
        byte wLow = (byte)(widthBytes & 0xFF);
        byte wHigh = (byte)((widthBytes >> 8) & 0xFF);
        byte hLow = (byte)(heightPixels & 0xFF);
        byte hHigh = (byte)((heightPixels >> 8) & 0xFF);

        return new byte[] {
            0x1D, 0x76, 0x30, 0x00,
            wLow, wHigh,
            hLow, hHigh
        }.Concat(data).ToArray();
    }

    private static IEnumerable<byte[]> Chunk(byte[] data, int size) {
        for(int i = 0; i < data.Length; i += size)
            yield return data.Skip(i).Take(size).ToArray();
    }

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
}