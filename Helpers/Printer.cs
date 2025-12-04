namespace SnapLabel.Helpers {
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

        // Threshold used to binarize images (0-255). Increase for "darker" output.
        // Typical good values for thermal printers: 180 - 220
        const int BinarizeThreshold = 200;

        // BLE chunk size for writes (many BLE devices use 20 bytes).
        const int BleChunkSize = 20;

        #region 🖨️ Print Text
        // --------------------------------------------------------------------
        // Handles styled text rendering (with tags for alignment and style)
        // Converts the text into a bitmap and sends it to the printer.
        // --------------------------------------------------------------------

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
                SKFontStyle weightStyle = SKFontStyle.Normal;
                if(bold && italic)
                    weightStyle = SKFontStyle.BoldItalic;
                else if(bold)
                    weightStyle = SKFontStyle.Bold;
                else if(italic)
                    weightStyle = SKFontStyle.Italic;

                using var tf = SKTypeface.FromFamilyName(null, weightStyle);
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
        // Generates and prints a centered QR code for the given image bytes.
        // The bytes are expected to be an encoded image (PNG/JPEG/etc).
        // --------------------------------------------------------------------

        /// <summary>
        /// Generates and prints a centered QR code for the given image bytes.
        /// </summary>
        public static async Task<bool> PrintQrAsync(IPeripheral peripheral, byte[] qrBytes) {
            var result = await GetWritablePrinterAsync(peripheral);
            if(result == null || result.Value.characteristic == null)
                return false;

            var (characteristic, service) = result.Value;

            // Decode incoming bytes to SKBitmap
            using var stream = new MemoryStream(qrBytes);
            using var original = SKBitmap.Decode(stream);
            if(original == null)
                return false;

            // Step 1: Binarize the decoded bitmap to remove anti-aliasing
            using var bin = Binarize(original, BinarizeThreshold);

            // Step 2: Resize to fit printable width (leave some side margin)
            int maxQrWidth = PaperWidth - 40; // leave 20px each side
            int qrSize = Math.Min(bin.Width, maxQrWidth);

            // Replace this line:
            // var resized = bin.Resize(new SKImageInfo(qrSize, qrSize), new SKSamplingOptions(SKFilterQuality.None));

            // With the following line using SKSamplingOptions with SKFilterMode.Nearest:
            var resized = bin.Resize(new SKImageInfo(qrSize, qrSize), new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None));
            if(resized == null)
                return false;

            // Step 3: Create final centered bitmap for printing
            int paddingTop = 10;
            int finalHeight = resized.Height + paddingTop + 10;
            var finalBmp = new SKBitmap(PaperWidth, finalHeight);
            using(var canvas = new SKCanvas(finalBmp)) {
                canvas.Clear(SKColors.White);
                int x = (PaperWidth - resized.Width) / 2;
                canvas.DrawBitmap(resized, x, paddingTop);
            }

            // Step 4: Send to printer
            return await SendBitmapAsync(peripheral, service.Uuid, characteristic!, finalBmp);
        }

        /// <summary>
        /// Returns a binarized copy of the source bitmap (black/white only).
        /// This removes anti-aliasing and forces crisp 1-bit shapes.
        /// </summary>
        private static SKBitmap Binarize(SKBitmap src, int threshold) {
            var result = new SKBitmap(src.Width, src.Height, src.ColorType, src.AlphaType);

            for(int y = 0; y < src.Height; y++) {
                for(int x = 0; x < src.Width; x++) {
                    var c = src.GetPixel(x, y);
                    // compute luminance using integer-friendly math
                    int gray = (int)(c.Red * 0.3 + c.Green * 0.59 + c.Blue * 0.11);
                    result.SetPixel(x, y, (gray < threshold) ? SKColors.Black : SKColors.White);
                }
            }

            return result;
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

            // Convert to white background and center horizontally
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

            // Reset/init printer
            var escInit = new byte[] { 0x1B, 0x40 }; // ESC @

            // Convert to 1-bit monochrome array
            var mono = ToMono(bitmap);

            // Build ESC/POS raster data header
            var raster = BuildRaster(mono, bitmap.Width, bitmap.Height);

            var data = escInit.Concat(raster).ToArray();

            var withResponse = characteristic.Properties.HasFlag(CharacteristicProperties.Write);

            // BLE writes: BleChunkSize-byte chunks
            foreach(var chunk in Chunk(data, BleChunkSize)) {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await peripheral.WriteCharacteristicAsync(serviceUuid, characteristic.Uuid, chunk, withResponse, cts.Token);
            }

            return true;
        }

        /// <summary>
        /// Converts an image to monochrome 1-bit pixel data (black or white).
        /// Rows are packed left-to-right, 8 pixels per byte, MSB first.
        /// Uses a stricter threshold to favor black for slightly dark pixels.
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
                            // Use a higher threshold to force more black pixels (reduces gray speckling)
                            if(gray < BinarizeThreshold)
                                b |= (byte)(1 << (7 - bit)); // Black pixel = 1
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

            // m (mode) byte: 0x00 is normal. Some firmwares accept other values (e.g., double density).
            // If you need more darkness you can experiment with different mode values,
            // but 0x00 is the most compatible.
            byte mode = 0x00;

            return new byte[]
            {
                0x1D, 0x76, 0x30, mode, // GS v 0 m
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
            var printerService = services.FirstOrDefault(s => s.Uuid.StartsWith("0000ff", StringComparison.OrdinalIgnoreCase));
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
}
