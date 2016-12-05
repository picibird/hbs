
using System;
using System.Drawing;
using System.Globalization;
using picibits.app.bitmap;
using picibits.app.services;
using picibits.core;
using ZXing;
using ZXing.Common;

namespace picibird.hbs.wpf.qrcode
{
    public class QRCodes : IQRCodes
    {

        public IBitmapImage Encode(string contents, int width, int height,
            string background = "#FFFBFEFF", string foreground = "#FF2D6178")
        {
            Color backgroundColor = FromHex(background);
            Color foregroundColor = FromHex(foreground);
            BarcodeWriter writer = new BarcodeWriter()
            {
                Renderer = new ZXing.Rendering.BitmapRenderer()
                {
                    Background = backgroundColor,
                    Foreground = foregroundColor
                }
            };
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = new EncodingOptions { Margin = 0, Width = width, Height = height, PureBarcode = true };
            Bitmap bmp = writer.Write(contents);
            IBitmapFactory fact = Pici.Services.Get<IBitmapFactory>();
            return fact.FromBitmap(bmp);
        }

        private static Color FromHex(string argbHEX)
        {
            int argb = Int32.Parse(argbHEX.Replace("#", ""), NumberStyles.HexNumber);
            return Color.FromArgb(argb);
        }

    }
}

