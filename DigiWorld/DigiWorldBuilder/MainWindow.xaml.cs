using DigiWorldBuilder.Data;
using DigiWorldBuilder.Helpers;
using Microsoft.Win32;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DigiWorldBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SKBitmap colorBMP { get; set; }
        public SKBitmap checkerBoard { get; set; }
        public ProjectMetaData MetaData { get; set; }
        public Dictionary<string, ExtendedResourceData> ResourceBitmaps { get; set; }

        private const string FreshWaterRef = "Fresh Water";
        private const string OceanWaterRef = "Ocean Water";

        public MainWindow()
        {
            InitializeComponent();
        }

        //Quit
        private void menuItem_QuitClick(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        //load color map
        private void btnLoadColor_Click(object sender, RoutedEventArgs e)
        {
            var colormapResult = loadImageMap();
            if (colormapResult != null)
            {
                colorBMP = colormapResult.Image;
            
            if (MetaData == null)
            {
                MetaData = new ProjectMetaData();
            }

                //iImage.Source = ImageHelper.GetBitmapFromSKBitmap(colorBMP);
                MetaData.ColorMapFileName = colormapResult.FileName;
                lblColorFile.Content = Path.GetFileName(MetaData.ColorMapFileName);
                btnShowColor.IsEnabled = true;
                btnShowColor.IsChecked = true;
                btnLoadFresh.IsEnabled = true;
                btnLoadOcean.IsEnabled = true;
                checkerBoard = ImageHelper.CreateCheckerboard(1024, 1024, 32, SKColors.DarkBlue, SKColors.Black) ;
                SKBitmap bmp = CombineImages();
                iImage.Source = ImageHelper.GetBitmapFromSKBitmap(bmp);
                btnResource.IsEnabled = true;
            }
        }

        private void btnResource_Click(object sender, RoutedEventArgs e)
        {
            pnlNewResource.Visibility = Visibility.Visible;
        }

        private void btnLoadFresh_Click(object sender, RoutedEventArgs e)
        {
            var freshwaterResult = loadImageMap();
            if (freshwaterResult != null)
            {
                MetaData.FreshWaterMetaData = new ResourceMetaData();
                MetaData.FreshWaterMetaData.ResourceName = FreshWaterRef;
                MetaData.FreshWaterMetaData.ResourceFilename = freshwaterResult.FileName;
                lblFreshWaterFile.Content = Path.GetFileName(freshwaterResult.FileName);
                cmbFreshChannel.IsEnabled = true;
                if (ResourceBitmaps == null)
                {
                    ResourceBitmaps = new Dictionary<string, ExtendedResourceData>();
                }
                ExtendedResourceData newData = new ExtendedResourceData
                {
                    RepColor = SKColors.Blue,
                    ResourceFilename = MetaData.FreshWaterMetaData.ResourceFilename,
                    ResourceName = MetaData.FreshWaterMetaData.ResourceName,
                    originalImage = freshwaterResult.Image
                };
                ResourceBitmaps.Add(newData.ResourceName, newData);
            }
        }

        private void btnLoadOcean_Click(object sender, RoutedEventArgs e)
        {
            var oceanwaterResult = loadImageMap();
            if (oceanwaterResult != null)
            {
                MetaData.OceanWaterMetaData = new ResourceMetaData();
                MetaData.OceanWaterMetaData.ResourceName = OceanWaterRef;
                MetaData.OceanWaterMetaData.ResourceFilename = oceanwaterResult.FileName;
                lblOceanWaterFile.Content = Path.GetFileName(oceanwaterResult.FileName);
                cmbOceanChannel.IsEnabled = true;
                if (ResourceBitmaps == null)
                {
                    ResourceBitmaps = new Dictionary<string, ExtendedResourceData>();

                }
                ExtendedResourceData newData = new ExtendedResourceData
                {
                    RepColor = SKColors.DarkBlue,
                    ResourceFilename = MetaData.OceanWaterMetaData.ResourceFilename,
                    ResourceName = MetaData.OceanWaterMetaData.ResourceName,
                    originalImage = oceanwaterResult.Image
                };
                ResourceBitmaps.Add(newData.ResourceName, newData);
            }
        }

        private void btnLoadResourceMap_Click(object sender, RoutedEventArgs e)
        {
            var resourceResult = loadImageMap();
            if (resourceResult != null)
            {
                var selectedColor = cpResourceColor.SelectedColor;
                if (!selectedColor.HasValue)
                {
                    selectedColor = Colors.White;
                }
                var data = new ExtendedResourceData
                {
                    ResourceName = txtResourceName.Text,
                    ResourceFilename = resourceResult.FileName,
                    RepColor = new SKColor(selectedColor.Value.R, selectedColor.Value.G, selectedColor.Value.B),
                    originalImage = resourceResult.Image
                };
                lblResourceFilename.Content = Path.GetFileName(resourceResult.FileName);
            }
        }

        private void cmbFreshChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SKColorChannel selectedChannel = SKColorChannel.R;
            switch (cmbFreshChannel.SelectedItem.ToString())
            {
                case "R":
                    selectedChannel = SKColorChannel.R;
                    break;
                case "G":
                    selectedChannel = SKColorChannel.G;
                    break;
                case "B":
                    selectedChannel = SKColorChannel.B;
                    break;
            }
            btnShowWater.IsEnabled = true;
            ExtendedResourceData data = ResourceBitmaps[FreshWaterRef];
            data.ResourceColorChannel = selectedChannel;
            data.convertedImage = ImageHelper.CreateBitmapFromMask(data.originalImage, data.ResourceColorChannel, data.RepColor);
            btnShowFreshWater.IsEnabled = true;
            btnShowWater.IsChecked = true;
            btnShowFreshWater.IsChecked = true;
            SKBitmap compiled = CombineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiled);
            ResourceBitmaps[FreshWaterRef] = data;
        }

        private void cmbOceanChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SKColorChannel selectedChannel = SKColorChannel.R;
            switch (cmbOceanChannel.SelectedItem.ToString())
            {
                case "R":
                    selectedChannel = SKColorChannel.R;
                    break;
                case "G":
                    selectedChannel = SKColorChannel.G;
                    break;
                case "B":
                    selectedChannel = SKColorChannel.B;
                    break;
            }
            btnShowWater.IsEnabled = true;
            ExtendedResourceData data = ResourceBitmaps[OceanWaterRef];
            data.ResourceColorChannel = selectedChannel;
            data.convertedImage = ImageHelper.CreateBitmapFromMask(data.originalImage, data.ResourceColorChannel, data.RepColor);
            btnShowOceanWater.IsEnabled = true;
            btnShowWater.IsChecked = true;
            btnShowOceanWater.IsChecked = true;
            SKBitmap compiled = CombineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiled);
            ResourceBitmaps[OceanWaterRef] = data;
        }

        private void btnShowColor_Click(object sender, RoutedEventArgs e)
        {
            var compiledImage = CombineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiledImage);
        }

        private SKBitmap CombineImages()
        {
            List<SKBitmap> bitmaps = new List<SKBitmap>();
            if (btnShowColor.IsChecked == true)
            {
                //iImage.Source = ImageHelper.GetBitmapFromSKBitmap(colorBMP);
                bitmaps.Add(colorBMP);
            }
            else
            {
                bitmaps.Add(checkerBoard);
                //iImage.Source = ImageHelper.GetBitmapFromSKBitmap(checkerBoard);
            }
            if (btnShowWater.IsEnabled == true)
            {
                if (btnShowFreshWater.IsChecked == true)
                {
                    bitmaps.Add(ResourceBitmaps[FreshWaterRef].convertedImage);
                }
                if (btnShowOceanWater.IsChecked == true)
                {
                    bitmaps.Add(ResourceBitmaps[OceanWaterRef].convertedImage);
                }
            }
            SKBitmap compiledImage = ImageHelper.CombineBitmaps(bitmaps, 1024, 1024);
            return compiledImage;
        }

        private GraphicData loadImageMap()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG Files(*.PNG)| *.PNG| All files(*.*) | *.*";
            openFileDialog.DefaultExt = ".png";
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                var map = ImageHelper.LoadBitmapFromFile(openFileDialog.FileName);

                return new GraphicData
                {
                    Image = map,
                    FileName = openFileDialog.FileName,
                };
            }
            else { return null; }
        }

        private void txtResourceName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtResourceName.Text))
            {
                btnLoadResourceMap.IsEnabled = true;
            }
            else
            {
                btnLoadResourceMap.IsEnabled = false;
            }
        }
    }
}
