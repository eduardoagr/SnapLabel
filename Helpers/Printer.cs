namespace SnapLabel.Helpers {
    public static class Printer {

        const int PaperWidth = 384; // 58mm paper

        // ✅ Print Text (works fine as before)
        public static async Task<bool> PrintTextAsync(IPeripheral peripheral, string text) {

            var result = await GetWritablePrinterAsync(peripheral);
            if(result == null || result.Value.characteristic == null)
                return false;

            var (characteristic, service) = result.Value;


            var fontSize = 20f;
            var lineSpacing = 8f;
            var padding = 20;
            var typeface = SKTypeface.Default;
            var font = new SKFont(typeface, fontSize);
            var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

            var lines = WrapText(text, PaperWidth - 40, font);
            var lineHeight = fontSize + lineSpacing;
            var totalHeight = (int)(lines.Count * lineHeight + padding * 2);

            var bmp = new SKBitmap(PaperWidth, totalHeight);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            for(int i = 0; i < lines.Count; i++) {
                var txt = lines[i];
                var w = font.MeasureText(txt);
                var x = (PaperWidth - w) / 2;
                var y = padding + i * lineHeight + fontSize;
                canvas.DrawText(txt, x, y, SKTextAlign.Left, font, paint);
            }

            return await SendBitmapAsync(peripheral, service.Uuid, characteristic!, bmp);
        }

        // ✅ Fixed QR Printing (No more cutoff!)
        public static async Task<bool> PrintQrAsync(IPeripheral peripheral, string content) {

            var result = await GetWritablePrinterAsync(peripheral);
            if(result == null || result.Value.characteristic == null)
                return false;

            var (characteristic, service) = result.Value;

            var qrGen = new QRCodeGenerator();
            var data = qrGen.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
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

        // ✅ Send Bitmap → ESC/POS data
        static async Task<bool> SendBitmapAsync(IPeripheral peripheral, string serviceUuid, BleCharacteristicInfo characteristic, SKBitmap bitmap) {

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
        static byte[] ToMono(SKBitmap bmp) {
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
        static byte[] BuildRaster(byte[] data, int widthPixels, int heightPixels) {
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

        static IEnumerable<byte[]> Chunk(byte[] data, int size) {
            for(int i = 0; i < data.Length; i += size)
                yield return data.Skip(i).Take(size).ToArray();
        }

        static List<string> WrapText(string text, float maxWidth, SKFont font) {
            var lines = new List<string>();
            foreach(var paragraph in text.Split('\n')) {
                var words = paragraph.Split(' ');
                var line = "";
                foreach(var word in words) {
                    var testLine = string.IsNullOrEmpty(line) ? word : line + " " + word;
                    if(font.MeasureText(testLine) > maxWidth) {
                        if(!string.IsNullOrEmpty(line))
                            lines.Add(line);
                        line = word;
                    }
                    else
                        line = testLine;
                }
                if(!string.IsNullOrEmpty(line))
                    lines.Add(line);
            }
            return lines;
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
}
