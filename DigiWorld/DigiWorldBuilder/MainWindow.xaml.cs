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
                checkerBoard = ImageHelper.CreateCheckerboard(1024, 1024, 32, SKColors.DarkBlue, SKColors.Black);
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
                    OriginalImage = freshwaterResult.Image
                };
                ResourceBitmaps.Add(newData.ResourceName, newData);
                mnuSaveAs.IsEnabled = true;
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
                    OriginalImage = oceanwaterResult.Image
                };
                ResourceBitmaps.Add(newData.ResourceName, newData);
            }
        }

        private void btnLoadResourceMap_Click(object sender, RoutedEventArgs e)
        {
            var resourceResult = loadImageMap();
            if (resourceResult != null)
            {
                //var selectedColor = cpResourceColor.SelectedColor;
                //if (!selectedColor.HasValue)
                //{
                //    selectedColor = Colors.White;
                //}
                var data = new ExtendedResourceData
                {
                    ResourceName = txtResourceName.Text,
                    ResourceFilename = resourceResult.FileName,
                    //RepColor = new SKColor(selectedColor.Value.R, selectedColor.Value.G, selectedColor.Value.B),
                    OriginalImage = resourceResult.Image
                };

                if (ResourceBitmaps == null)
                {
                    ResourceBitmaps = new Dictionary<string, ExtendedResourceData>();
                }
                if (ResourceBitmaps.ContainsKey(txtResourceName.Text))
                {
                    var overwriteResult = MessageBox.Show("There is already a resource by this name. Would you like to overwrite it?", "Overwrite resource?", MessageBoxButton.YesNoCancel);
                    if (overwriteResult == MessageBoxResult.Yes)
                    {
                        ResourceBitmaps.Remove(txtResourceName.Text);
                    }
                    else
                    {
                        txtResourceName.IsEnabled = true;
                        btnLoadResourceMap.IsEnabled = false;
                        cmbResourceChannel.IsEnabled = false;
                        cpResourceColor.IsEnabled = false;
                        return;
                    }
                }
                lblResourceFilename.Content = Path.GetFileName(resourceResult.FileName);
                cmbResourceChannel.IsEnabled = true;
                cpResourceColor.SelectedColor = Colors.White;
                cpResourceColor.IsEnabled = true;
                ResourceBitmaps.Add(data.ResourceName, data);
                txtResourceName.IsEnabled = false;
            }
        }

        private void cmbFreshChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SKColorChannel selectedChannel = SKColorChannel.R;
            switch (cmbFreshChannel.SelectedValue.ToString())
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
            data.ConvertedImage = ImageHelper.CreateBitmapFromMask(data.OriginalImage, data.ResourceColorChannel, data.RepColor);
            data.IsPublished = true;
            data.IsVisible = true;
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
            switch (cmbOceanChannel.SelectedValue.ToString())
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
            data.ConvertedImage = ImageHelper.CreateBitmapFromMask(data.OriginalImage, data.ResourceColorChannel, data.RepColor);
            data.IsPublished = true;
            data.IsVisible = true;
            btnShowOceanWater.IsEnabled = true;
            btnShowWater.IsChecked = true;
            btnShowOceanWater.IsChecked = true;
            SKBitmap compiled = CombineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiled);
            ResourceBitmaps[OceanWaterRef] = data;
        }


        private void cmbResourceChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SKColorChannel selectedChannel = SKColorChannel.R;
            var selectedItem = cmbResourceChannel.SelectedValue.ToString();
            switch (selectedItem)
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
            ExtendedResourceData data = ResourceBitmaps[txtResourceName.Text];
            data.ResourceColorChannel = selectedChannel;
            data.ConvertedImage = ImageHelper.CreateBitmapFromMask(data.OriginalImage, data.ResourceColorChannel, data.RepColor);
            ResourceBitmaps[txtResourceName.Text] = data;
            btnAddResource.IsEnabled = true;
        }

        private void cpResourceColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var resourceName = txtResourceName.Text;
            if (!string.IsNullOrWhiteSpace(resourceName) && ResourceBitmaps != null && ResourceBitmaps.Count > 0 && ResourceBitmaps.ContainsKey(resourceName))
            {
                var data = ResourceBitmaps[resourceName];
                SKColorChannel selectedChannel = SKColorChannel.R;
                var selectedItem = cmbResourceChannel.SelectedValue.ToString();
                switch (selectedItem)
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
                data.ResourceColorChannel = selectedChannel;
                var selectedColor = cpResourceColor.SelectedColor;
                data.RepColor = new SKColor(selectedColor.Value.R, selectedColor.Value.G, selectedColor.Value.B);
                data.ConvertedImage = ImageHelper.CreateBitmapFromMask(data.OriginalImage, data.ResourceColorChannel, data.RepColor);
                ResourceBitmaps[txtResourceName.Text] = data;
            }
        }

        private void btnCancelResource_Click(object sender, RoutedEventArgs e)
        {
            pnlNewResource.Visibility = Visibility.Collapsed;
            if (txtResourceName.IsEnabled == false)
            {

                ResourceBitmaps.Remove(txtResourceName.Text);
            }
                txtResourceName.Text = "";
                txtResourceName.IsEnabled = true;
            btnLoadResourceMap.IsEnabled = false;
            //cpResourceColor.SelectedColor = Colors.White;
            cpResourceColor.IsEnabled = false;
            cmbResourceChannel.IsEnabled = false;
            lblResourceFilename.Content = "Filename...";
        }

        private void btnAddResource_Click(object sender, RoutedEventArgs e)
        {
            var data = ResourceBitmaps[txtResourceName.Text];
            data.IsPublished = true;
            data.IsVisible = true;
            pnlNewResource.Visibility= Visibility.Collapsed;

            cpResourceColor.IsEnabled = false;
            cmbResourceChannel.IsEnabled = false;
            btnLoadResourceMap.IsEnabled = false;
            ResourceBitmaps[txtResourceName.Text] = data;
            //cmbResources.Items.Add(txtResourceName.Text);
            var resourceKeys = ResourceBitmaps.Keys;
            cmbResources.Items.Clear();
            foreach (var key in resourceKeys)
            {
                cmbResources.Items.Add(key);
            }
            if (cmbResources.IsEnabled == false)
            {
                cmbResources.IsEnabled = true;

            }
            btnShowResource.IsEnabled = true;
            btnShowResource.IsChecked = true;
            btnDeleteResource.IsEnabled = true;
            cmbResources.SelectedIndex = cmbResources.Items.Count - 1;
            //cpResourceColor.SelectedColor = Colors.White;
            txtResourceName.Text = "";
            txtResourceName.IsEnabled = true;
            lblResourceFilename.Content = "Filename...";
            var compiledImage = CombineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiledImage);
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            if (btnShowWater.IsChecked == false)
            {
                btnShowFreshWater.IsEnabled = false;
                btnShowOceanWater.IsEnabled = false;
                if (ResourceBitmaps != null && ResourceBitmaps.Count > 0)
                {
                    if (ResourceBitmaps.ContainsKey(FreshWaterRef))
                    {
                        var freshwaterdata = ResourceBitmaps[FreshWaterRef];
                        freshwaterdata.IsVisible = false;
                        ResourceBitmaps[FreshWaterRef] = freshwaterdata;
                    }
                    if (ResourceBitmaps.ContainsKey(OceanWaterRef))
                    {
                        var oceanwaterdata = ResourceBitmaps[OceanWaterRef];
                        oceanwaterdata.IsVisible = false;
                        ResourceBitmaps[OceanWaterRef] = oceanwaterdata;
                    }
                }
            }
            else
            {
                if (ResourceBitmaps != null && ResourceBitmaps.Count > 0)
                {
                    if (ResourceBitmaps.ContainsKey(FreshWaterRef))
                    {
                        btnShowFreshWater.IsEnabled = true;


                        var freshwaterdata = ResourceBitmaps[FreshWaterRef];
                        if (btnShowFreshWater.IsChecked == true)
                        {

                            freshwaterdata.IsVisible = true;

                        }
                        else
                        {
                            freshwaterdata.IsVisible = false;
                        }
                        ResourceBitmaps[FreshWaterRef] = freshwaterdata;
                    }
                    if (ResourceBitmaps.ContainsKey(OceanWaterRef))
                    {
                        btnShowOceanWater.IsEnabled = true;
                        var oceanwaterdata = ResourceBitmaps[OceanWaterRef];
                        if (btnShowOceanWater.IsChecked == true)
                        {
                            oceanwaterdata.IsVisible = true;
                        }
                        else
                        {
                            oceanwaterdata.IsVisible = false;
                        }
                        ResourceBitmaps[OceanWaterRef] = oceanwaterdata;
                    }
                }
            }
            var compiledImage = CombineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiledImage);
        }

        private void btnShowResource_Click(object sender, RoutedEventArgs e)
        {
            string selectedItem = cmbResources.SelectedItem.ToString();
            var data = ResourceBitmaps[selectedItem];
            data.IsVisible = btnShowResource.IsChecked == true;
            ResourceBitmaps[selectedItem] = data;
            var image = CombineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(image);
        }

        private void btnDeleteResource_Click(object sender, RoutedEventArgs e)
        {
            var messageBox = MessageBox.Show("Are you sure you want to delete this resource?", "Delete resource", MessageBoxButton.YesNoCancel);
            if (messageBox == MessageBoxResult.Yes)
            {
                var selectedItem = cmbResources.SelectedItem.ToString();
                ResourceBitmaps.Remove(selectedItem);
                cmbResources.Items.Clear();
                if (ResourceBitmaps.Count > 0)
                {
                    var resourceList = ResourceBitmaps.Keys.ToList();
                    //cmbResources.Items.Add(resourceList);
                    foreach (var item in resourceList)
                    {
                        cmbResources.Items.Add(item);
                    }
                    cmbResources.SelectedIndex = 0;
                    cmbResources.Text = resourceList[0];
                    var newSelectedItem = resourceList[0];
                    var data = ResourceBitmaps[newSelectedItem];
                    if (data.IsVisible == true)
                    {
                        btnShowResource.IsChecked = true;
                    }
                    else
                    {
                        btnShowResource.IsChecked = false;
                    }
                }
                else
                {
                    btnShowResource.IsEnabled = false;
                    btnDeleteResource.IsEnabled = false;
                }
                var image = CombineImages();
                iImage.Source = ImageHelper.GetBitmapFromSKBitmap(image);
            }
        }

        private void cmbResources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResourceBitmaps != null && ResourceBitmaps.Count > 0 && !string.IsNullOrWhiteSpace(cmbResources.Text) && ResourceBitmaps.ContainsKey(cmbResources.Text))
            {
                var data = ResourceBitmaps[cmbResources.Text];
                if (data.IsVisible == true)
                {
                    btnShowResource.IsChecked = true;
                }
                else
                {
                    btnShowResource.IsChecked = false;
                }
            }
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

        private SKBitmap CombineImages()
        {
            List<SKBitmap> bitmaps = new List<SKBitmap>();
            if (btnShowColor.IsChecked == true)
            {
            //    //iImage.Source = ImageHelper.GetBitmapFromSKBitmap(colorBMP);
                bitmaps.Add(colorBMP);
            }
            else
            {
                bitmaps.Add(checkerBoard);
            //    //iImage.Source = ImageHelper.GetBitmapFromSKBitmap(checkerBoard);
            }

            if (ResourceBitmaps != null && ResourceBitmaps.Count > 0)
            {
                foreach (var bitmap in ResourceBitmaps)
                {
                    if (bitmap.Value.IsVisible && bitmap.Value.IsPublished)
                    {
                        bitmaps.Add(bitmap.Value.ConvertedImage);
                    }
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
    }
}
