using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static SkiaSharp.SKImageFilter;

namespace DigiWorldTileTool.Helpers
{
    public static class ImageHelper
    {
        public static SKBitmap LoadBitmapFromFile(string filePath)
        {
            using (SKStream stream = new SKFileStream(filePath))
            {
                SKBitmap skBitmap = SKBitmap.Decode(stream);
                return skBitmap;
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

        public static SKBitmap TileBitmap(SKBitmap bitmap, int x, int y)
        {
            //SKBitmap newBitmap = new SKBitmap(bitmap.Width * x, bitmap.Height * y);
            SKImageInfo imageInfo = new SKImageInfo(bitmap.Width * x, bitmap.Height * y);
            using (SKSurface surface = SKSurface.Create(imageInfo))
            {
                SKCanvas canvas = surface.Canvas;
                using (SKPaint paint = new SKPaint())
                {
                    for (int yPos = 0; yPos < y; yPos++)
                    {
                        for (int xPos = 0; xPos < x; xPos++)
                        {
                            canvas.DrawBitmap(bitmap, bitmap.Width * xPos, bitmap.Height * yPos);
                        }
                    }
                }
                return SKBitmap.FromImage(surface.Snapshot());
            }
        }

        public static SKBitmap CreateBitmapPreviewFromMask(SKBitmap mask, SKColorChannel colorChannel, SKBitmap map)
        {
            SKBitmap newBitmap = new SKBitmap(mask.Width, mask.Height);
            SKBitmap mapBitmap = map.Resize(new SKImageInfo(mask.Width / 3, mask.Height / 3), SKFilterQuality.High);
            SKBitmap tiledBitmap = TileBitmap(mapBitmap, 3, 3);
            for (int y = 0; y < mask.Height; y++)
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    var pixelAlpha = mask.GetPixel(x, y);
                    var pixelColor = tiledBitmap.GetPixel(x, y);
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
                    newBitmap.SetPixel(x, y, new SKColor(pixelColor.Red, pixelColor.Green, pixelColor.Blue, value));
                }
            }
            return newBitmap;
        }

        public static SKBitmap CreateBitmapTileFromMask(SKBitmap mask, SKColorChannel colorChannel, SKBitmap map, int x, int y)
        {
            int width = mask.Width / 3;
            int height = mask.Height / 3;
            SKRectI sampleRect = new SKRectI(width * x, height * y, width * (x+1), height * (y + 1));
            SKRectI destrect = new SKRectI(0, 0, width, height);
            SKBitmap croppedMask = new SKBitmap(width, height);
            // Create an SKCanvas for drawing onto the new bitmap
            using (SKCanvas canvas = new SKCanvas(croppedMask))
            {
                // Create an SKPaint to define drawing properties
                using (SKPaint paint = new SKPaint())
                {
                    // Draw the specified region of the source bitmap onto the new bitmap
                    canvas.DrawBitmap(mask, sampleRect, destrect, paint);
                }
            }
            SKBitmap mapBitmap = map.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
            SKBitmap completeBitmap = new SKBitmap(width, height);
            for (int yPos = 0; yPos < height; yPos++)
            {
                for (int xPos = 0; xPos < width; xPos++)
                {
                    var pixelAlpha = croppedMask.GetPixel(xPos, yPos);
                    var pixelColor = mapBitmap.GetPixel(xPos, yPos);
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
                    completeBitmap.SetPixel(xPos, yPos, new SKColor(pixelColor.Red, pixelColor.Green, pixelColor.Blue, value));
                }
            }
            return completeBitmap;
        }

        public static List<SKBitmap> CreateBitmapTilesFromMask(SKBitmap mask, SKColorChannel colorChannel, SKBitmap map)
        {
            List<SKBitmap> bitmapList = new List<SKBitmap>();
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    SKBitmap currentTile = CreateBitmapTileFromMask(mask, colorChannel, map, x, y);
                    bitmapList.Add(currentTile);
                }
            }
            return bitmapList;
        }

        public static bool SaveImageToFile(string filename, SKBitmap bitmap, SKEncodedImageFormat format)
        {
            try
            {
                using (var image = SKImage.FromBitmap(bitmap))
                {
                    using (var data = image.Encode(format, 100))
                    {
                        using (var stream = new System.IO.FileStream(filename, FileMode.Create))
                        {
                            data.SaveTo(stream);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


    }
}
