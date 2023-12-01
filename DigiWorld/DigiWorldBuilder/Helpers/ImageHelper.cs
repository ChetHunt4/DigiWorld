using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DigiWorldBuilder.Helpers
{
    public static class ImageHelper
    {
        public static SKBitmap LoadBitmapFromFile(string filePath)
        {
            //SKImageInfo imageInfo = new SKImageInfo();
            using (SKStream stream = new SKFileStream(filePath))
            {
                //using (SKImage skImage = SKImage.FromEncodedData(stream))
                //{
                    SKBitmap skBitmap = SKBitmap.Decode(stream);
                    return skBitmap;
                //}
            }
        }

        public static BitmapSource BitmapImageFromByteArray(byte[] imageBytes)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new System.IO.MemoryStream(imageBytes);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public static BitmapSource GetBitmapFromSKBitmap(SKBitmap bitmap)
        {
            if (bitmap != null)
            {
                SKImageInfo imageInfo = new SKImageInfo(bitmap.Width, bitmap.Height);
                using (SKSurface surface = SKSurface.Create(imageInfo))
                {
                    SKCanvas canvas = surface.Canvas;
                    using (SKPaint paint = new SKPaint())
                    {
                        canvas.DrawBitmap(bitmap, 0, 0);
                    }
                    using (SKImage datImage = surface.Snapshot())
                    using (SKData data = datImage.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        byte[] imageBytes = data.ToArray();
                        BitmapSource bmsource = BitmapImageFromByteArray(imageBytes);

                        return bmsource;
                    }
                }
            }
            return null;
        }

        public static SKBitmap CreateCheckerboard(int width, int height, int tileSize, SKColor color1, SKColor color2)
        {

            int numTilesX = width / tileSize;
            int numTilesY = height / tileSize;
            int patternSizeX = numTilesX * tileSize;
            int patternSizeY = numTilesY * tileSize;

            SKBitmap baseTile = new SKBitmap(tileSize, tileSize);
            using (var surface = SKSurface.Create(new SKImageInfo(tileSize, tileSize)))
            {
                SKCanvas tileCanvas = surface.Canvas;
                SKPaint checkerPaint = new SKPaint();
                tileCanvas.Clear(color1);
                checkerPaint.Color = color2;
                tileCanvas.DrawRect(new SKRect(tileSize / 2f, 0, tileSize, tileSize / 2f), checkerPaint);
                tileCanvas.DrawRect(new SKRect(0, tileSize / 2f, tileSize / 2f, tileSize), checkerPaint);

                baseTile = SKBitmap.FromImage(surface.Snapshot());
            }

            SKShader shader = SKShader.CreateBitmap(baseTile, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);

            SKPaint paint = new SKPaint
            {
                Shader = shader
            };

            using (var surface = SKSurface.Create(new SKImageInfo(patternSizeX, patternSizeY)))
            {
                SKCanvas canvas = surface.Canvas;

                canvas.DrawRect(new SKRect(0, 0, patternSizeX, patternSizeY), paint);

                return SKBitmap.FromImage(surface.Snapshot());
            }
        }

        public static SKBitmap CombineBitmaps(List<SKBitmap> bitmaps, int width, int height)
        {
            if (bitmaps == null || bitmaps.Count <= 0 || width <= 0 || height <= 0)
            {
                return null;
            }
            using (SKSurface surf = SKSurface.Create(new SKImageInfo(width, height)))
            using (SKCanvas canvas = surf.Canvas)
            {
                canvas.Clear();
                SKPaint paint = new SKPaint
                {
                    BlendMode = SKBlendMode.SrcOver
                };
                //bitmaps.Reverse();
                foreach (SKBitmap bitmap in bitmaps)
                {
                    canvas.DrawBitmap(bitmap, 0, 0, paint);
                }
                return SKBitmap.FromImage(surf.Snapshot());
            }

        }

        public static SKBitmap CreateBitmapFromMask(SKBitmap mask, SKColorChannel colorChannel, SKColor color)
        {
            SKBitmap newBitmap = new SKBitmap(mask.Width, mask.Height);
            for (int y = 0; y < mask.Height; y++)
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    var pixelAlpha = mask.GetPixel(x, y);
                    byte value = 0;
                    switch (colorChannel)
                    {
                        case SKColorChannel.R:
                            value = pixelAlpha.Red;
                            break;
                        case SKColorChannel.G:
                            value = pixelAlpha.Green;
                            break;
                        case SKColorChannel.B:
                            value = pixelAlpha.Blue;
                            break;
                    }
                    newBitmap.SetPixel(x, y, new SKColor(color.Red, color.Green, color.Blue, value));
                }
            }
            return newBitmap;
        }
    }
}
