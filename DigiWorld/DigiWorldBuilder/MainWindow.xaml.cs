using DigiWorldBuilder.Data;
using DigiWorldBuilder.Helpers;
using Microsoft.Win32;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

        public string ProjectFileName { get; set; }
        public bool needsSave { get; set; }

        private const string FreshWaterRef = "Fresh Water";
        private const string OceanWaterRef = "Ocean Water";

        public MainWindow()
        {
            InitializeComponent();
        }

        //Open File
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "PRJ Files(*.PRJ)| *.PRJ| All files(*.*) | *.*";
            openDialog.DefaultExt = ".prj";
            var openResult = openDialog.ShowDialog();
            if (openResult == true)
            {
                btnShowColor.IsEnabled = false;
                btnShowColor.IsChecked = false;
                btnShowWater.IsEnabled = false;
                btnShowWater.IsChecked = false;
                btnShowFreshWater.IsEnabled = false;
                btnShowFreshWater.IsChecked = false;
                btnShowOceanWater.IsEnabled = false;
                btnShowOceanWater.IsChecked = false;
                btnShowResource.IsEnabled = false;
                btnShowResource.IsChecked = false;
                btnDeleteResource.IsEnabled = false;

                var fileString = File.ReadAllText(openDialog.FileName);
                MetaData = JsonConvert.DeserializeObject<ProjectMetaData>(fileString);
                ProjectFileName = openDialog.FileName;
                ResourceBitmaps = new Dictionary<string, ExtendedResourceData>();
                if (!string.IsNullOrWhiteSpace(MetaData?.ColorMapFileName))
                {
                    colorBMP = ImageHelper.LoadBitmapFromFile(MetaData.ColorMapFileName);
                    checkerBoard = ImageHelper.CreateCheckerboard(1024, 1024, 32, SKColors.DarkBlue, SKColors.Black);
                    lblColorFile.Content = Path.GetFileName(MetaData.ColorMapFileName);
                    btnShowColor.IsEnabled = true;
                    btnShowColor.IsChecked = true;
                    btnResource.IsEnabled = true;
                }
                if (ResourceBitmaps == null)
                {
                    ResourceBitmaps = new Dictionary<string, ExtendedResourceData>();
                }
                if (MetaData?.ResourceMetaData != null && MetaData.ResourceMetaData.Count > 0)
                {
                    btnGenerate.IsEnabled = true;
                    cmbResources.Items.Clear();
                    foreach (var metaData in MetaData.ResourceMetaData)
                    {
                        SKColorChannel colorChannel = SKColorChannel.R;
                        switch (metaData.Value.ResourceColorChannel)
                        {
                            case ColorChannel.R:
                                colorChannel = SKColorChannel.R;
                                break;
                            case ColorChannel.G:
                                colorChannel = SKColorChannel.G;
                                break;
                            case ColorChannel.B:
                                colorChannel = SKColorChannel.B;
                                break;
                        }
                        SKColor color = new SKColor(metaData.Value.RepColor.Red, metaData.Value.RepColor.Green, metaData.Value.RepColor.Blue);
                        ExtendedResourceData extData = new ExtendedResourceData
                        {
                            RepColor = metaData.Value.RepColor,
                            SKRepColor = color,
                            Properties = metaData.Value.Properties,
                            ResourceColorChannel = metaData.Value.ResourceColorChannel,
                            SKResourceColorChannel = colorChannel,
                            ResourceFilename = metaData.Value.ResourceFilename,
                            ResourceName = metaData.Value.ResourceName
                        };
                        extData.OriginalImage = ImageHelper.LoadBitmapFromFile(metaData.Value.ResourceFilename);
                        extData.ConvertedImage = ImageHelper.CreateBitmapFromMask(extData.OriginalImage, extData.SKResourceColorChannel, extData.SKRepColor);
                        extData.IsPublished = true;
                        extData.IsVisible = true;
                        ResourceBitmaps.Add(extData.ResourceName, extData);
                        if (extData.ResourceName == FreshWaterRef)
                        {
                            btnShowWater.IsEnabled = true;
                            btnShowWater.IsChecked = true;
                            btnShowFreshWater.IsEnabled = true;
                            btnShowFreshWater.IsChecked = true;
                            btnLoadFresh.IsEnabled = true;
                            lblFreshWaterFile.Content = Path.GetFileName(extData.ResourceFilename);
                            cmbFreshChannel.IsEnabled = true;
                            cmbFreshChannel.Text = extData.ResourceColorChannel.ToString();

                        }
                        else if (extData.ResourceName == OceanWaterRef)
                        {
                            btnShowWater.IsEnabled = true;
                            btnShowWater.IsChecked = true;
                            btnShowOceanWater.IsEnabled = true;
                            btnShowOceanWater.IsChecked = true;
                            btnLoadOcean.IsEnabled = true;
                            lblOceanWaterFile.Content = Path.GetFileName(extData.ResourceFilename);
                            cmbOceanChannel.IsEnabled = true;
                            cmbOceanChannel.Text = extData.ResourceColorChannel.ToString();
                        }
                        else
                        {
                            cmbResources.Items.Add(extData.ResourceName);
                            cmbResources.IsEnabled = true;
                            btnShowResource.IsEnabled = true;
                            btnShowResource.IsChecked = true;
                            btnDeleteResource.IsEnabled = true;
                            cmbResources.SelectedIndex = cmbResources.Items.Count - 1;
                            
                        }
                    }
                }
                var combinedImage = CombineImages();
                iImage.Source = ImageHelper.GetBitmapFromSKBitmap(combinedImage);
                updateSaveStatus(false);
            }
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            var saveFile = JsonConvert.SerializeObject(MetaData);
            File.WriteAllText(ProjectFileName, saveFile);
            updateSaveStatus(false);
        }

        //Save As
        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var fileSaveDialog = new SaveFileDialog();
            fileSaveDialog.Filter = "PRJ Files(*.PRJ)| *.PRJ| All files(*.*) | *.*";
            fileSaveDialog.DefaultExt = ".prj";
            var saveResult = fileSaveDialog.ShowDialog();
            if (saveResult == true)
            {
                var saveFile = JsonConvert.SerializeObject(MetaData);
                File.WriteAllText(fileSaveDialog.FileName, saveFile);
                ProjectFileName = fileSaveDialog.FileName;
                this.Title = "DigiWorld Builder - " + ProjectFileName;
                updateSaveStatus(false);
            }
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
                updateSaveStatus(true);
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
                if (MetaData.ResourceMetaData == null)
                {
                    MetaData.ResourceMetaData = new Dictionary<string, ResourceMetaData>();
                }
                SKColor skrepColor = SKColors.Blue;
                BasicColor repColor = new BasicColor
                {
                    Red = skrepColor.Red,
                    Green = skrepColor.Green,
                    Blue = skrepColor.Blue
                };
                var metaData = new ResourceMetaData
                {
                    RepColor = repColor,
                    ResourceName = FreshWaterRef,
                    ResourceFilename = freshwaterResult.FileName
                };
                if (MetaData.ResourceMetaData.ContainsKey(FreshWaterRef))
                {
                    MetaData.ResourceMetaData[FreshWaterRef] = metaData;
                }
                else
                {
                    MetaData.ResourceMetaData.Add(FreshWaterRef, metaData);
                }
                lblFreshWaterFile.Content = Path.GetFileName(freshwaterResult.FileName);
                cmbFreshChannel.IsEnabled = true;
                if (ResourceBitmaps == null)
                {
                    ResourceBitmaps = new Dictionary<string, ExtendedResourceData>();
                }
                ExtendedResourceData newData = new ExtendedResourceData
                {
                    SKRepColor = skrepColor,
                    ResourceFilename = MetaData.ResourceMetaData[FreshWaterRef].ResourceFilename,
                    ResourceName = MetaData.ResourceMetaData[FreshWaterRef].ResourceName,
                    OriginalImage = freshwaterResult.Image
                };
                if (ResourceBitmaps.ContainsKey(newData.ResourceName))
                {
                    ResourceBitmaps[newData.ResourceName] = newData;
                }
                else
                {
                    ResourceBitmaps.Add(newData.ResourceName, newData);
                }
                updateSaveStatus(true);
            }
        }

        private void btnLoadOcean_Click(object sender, RoutedEventArgs e)
        {
            var oceanwaterResult = loadImageMap();
            if (oceanwaterResult != null)
            {
                if (MetaData.ResourceMetaData == null)
                {
                    MetaData.ResourceMetaData = new Dictionary<string, ResourceMetaData>();
                }
                SKColor skrepColor = SKColors.DarkBlue;
                BasicColor repColor = new BasicColor
                {
                    Red = skrepColor.Red,
                    Blue = skrepColor.Blue,
                    Green = skrepColor.Green
                };
                var metaData = new ResourceMetaData
                {
                    RepColor = repColor,
                    ResourceName = OceanWaterRef,
                    ResourceFilename = oceanwaterResult.FileName
                };
                if (MetaData.ResourceMetaData.ContainsKey(OceanWaterRef))
                {
                    MetaData.ResourceMetaData[OceanWaterRef] = metaData;
                }
                else
                {
                    MetaData.ResourceMetaData.Add(OceanWaterRef, metaData);
                }
                lblOceanWaterFile.Content = Path.GetFileName(oceanwaterResult.FileName);
                cmbOceanChannel.IsEnabled = true;
                if (ResourceBitmaps == null)
                {
                    ResourceBitmaps = new Dictionary<string, ExtendedResourceData>();

                }
                ExtendedResourceData newData = new ExtendedResourceData
                {
                    SKRepColor = skrepColor,
                    ResourceFilename = MetaData.ResourceMetaData[OceanWaterRef].ResourceFilename,
                    ResourceName = MetaData.ResourceMetaData[OceanWaterRef].ResourceName,
                    OriginalImage = oceanwaterResult.Image
                };
                if (ResourceBitmaps.ContainsKey(newData.ResourceName))
                {
                    ResourceBitmaps[newData.ResourceName] = newData;
                }
                else
                {
                    
                ResourceBitmaps.Add(newData.ResourceName, newData);
                }
                updateSaveStatus(true);
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
            data.SKResourceColorChannel = selectedChannel;
            data.ConvertedImage = ImageHelper.CreateBitmapFromMask(data.OriginalImage, data.SKResourceColorChannel, data.SKRepColor);
            data.IsPublished = true;
            data.IsVisible = true;
            btnShowFreshWater.IsEnabled = true;
            btnShowWater.IsChecked = true;
            btnShowFreshWater.IsChecked = true;
            SKBitmap compiled = CombineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiled);
            ResourceBitmaps[FreshWaterRef] = data;
            var metaData = MetaData.ResourceMetaData[FreshWaterRef];
            ColorChannel repChannel = ColorChannel.R;
            switch (selectedChannel)
            {
                case SKColorChannel.R:
                    repChannel = ColorChannel.R;
                    break;
                case SKColorChannel.G:
                    repChannel = ColorChannel.G;
                    break;
                case SKColorChannel.B:
                    repChannel = ColorChannel.B;
                    break;
            }
            metaData.ResourceColorChannel = repChannel;
            MetaData.ResourceMetaData[FreshWaterRef] = metaData;
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
            data.SKResourceColorChannel = selectedChannel;
            data.ConvertedImage = ImageHelper.CreateBitmapFromMask(data.OriginalImage, data.SKResourceColorChannel, data.SKRepColor);
            data.IsPublished = true;
            data.IsVisible = true;
            btnShowOceanWater.IsEnabled = true;
            btnShowWater.IsChecked = true;
            btnShowOceanWater.IsChecked = true;
            SKBitmap compiled = CombineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiled);
            ResourceBitmaps[OceanWaterRef] = data;
            var metaData = MetaData.ResourceMetaData[OceanWaterRef];
            ColorChannel repChannel = ColorChannel.R;
            switch (selectedChannel)
            {
                case SKColorChannel.R:
                    repChannel = ColorChannel.R;
                    break;
                case SKColorChannel.G:
                    repChannel = ColorChannel.G;
                    break;
                case SKColorChannel.B:
                    repChannel = ColorChannel.B;
                    break;
            }
            metaData.ResourceColorChannel = repChannel;
            MetaData.ResourceMetaData[OceanWaterRef] = metaData;
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
            data.SKResourceColorChannel = selectedChannel;
            data.ConvertedImage = ImageHelper.CreateBitmapFromMask(data.OriginalImage, data.SKResourceColorChannel, data.SKRepColor);
            ResourceBitmaps[txtResourceName.Text] = data;
            btnAddResource.IsEnabled = true;
            //MetaData.ResourceMetaData[txtResourceName.Text] = data;
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
                data.SKResourceColorChannel = selectedChannel;
                var selectedColor = cpResourceColor.SelectedColor;
                data.SKRepColor = new SKColor(selectedColor.Value.R, selectedColor.Value.G, selectedColor.Value.B);
                data.ConvertedImage = ImageHelper.CreateBitmapFromMask(data.OriginalImage, data.SKResourceColorChannel, data.SKRepColor);
                ResourceBitmaps[txtResourceName.Text] = data;
                //MetaData.ResourceMetaData[txtResourceName.Text] = data;
            }
        }

        private void btnCancelResource_Click(object sender, RoutedEventArgs e)
        {
            pnlNewResource.Visibility = Visibility.Collapsed;
            if (txtResourceName.IsEnabled == false)
            {

                ResourceBitmaps.Remove(txtResourceName.Text);
                MetaData.ResourceMetaData.Remove(txtResourceName.Text);
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
            if (MetaData.ResourceMetaData == null)
            {
                MetaData.ResourceMetaData = new Dictionary<string, ResourceMetaData>();
            }

                //MetaData.ResourceMetaData[txtResourceName.Text] = data;
                var resourceColorChannel = ColorChannel.R;
                switch (data.SKResourceColorChannel)
                {
                    case SKColorChannel.R:
                        resourceColorChannel = ColorChannel.R;
                        break;
                    case SKColorChannel.G:
                        resourceColorChannel = ColorChannel.G;
                        break;
                    case SKColorChannel.B:
                        resourceColorChannel = ColorChannel.B;
                        break;
                }
                var metaData = new ResourceMetaData
                {
                    RepColor = new BasicColor
                    {
                        Red = data.SKRepColor.Red,
                        Green = data.SKRepColor.Green,
                        Blue = data.SKRepColor.Blue,
                    },
                    ResourceColorChannel = resourceColorChannel,
                    ResourceFilename = data.ResourceFilename,
                    ResourceName = data.ResourceName,
                    Properties = data.Properties
                };
            if (MetaData.ResourceMetaData.ContainsKey(txtResourceName.Text))
            {
                MetaData.ResourceMetaData[txtResourceName.Text] = metaData;
            }
            else
            {
                MetaData.ResourceMetaData.Add(txtResourceName.Text, metaData);
            }
            
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
            updateSaveStatus(true);
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
                MetaData.ResourceMetaData.Remove(selectedItem);
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
            var resourceName = cmbResources.SelectedItem != null ? cmbResources.SelectedItem.ToString() : null;
            if (ResourceBitmaps != null && ResourceBitmaps.Count > 0 && !string.IsNullOrWhiteSpace(resourceName) && ResourceBitmaps.ContainsKey(resourceName))
            {
                var data = ResourceBitmaps[resourceName];
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

        private void updateSaveStatus(bool change = true)
        {
                mnuSaveAs.IsEnabled = true;
                if (!string.IsNullOrWhiteSpace(ProjectFileName) && change)
            {
                mnuSave.IsEnabled = true;
                needsSave = true;
            }
                else
            {
                mnuSave.IsEnabled = false;
                needsSave = false;
            }
        }


    }
}
