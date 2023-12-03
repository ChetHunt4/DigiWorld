using DigiWorldBuilder.Data;
using DigiWorldBuilder.Helpers;
using DigiWorldLib.World;
using Microsoft.Win32;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private const string filenameRef = "Filename...";

        private const byte pixelThresholdValue = 100;

        public MainWindow()
        {
            InitializeComponent();
            Closing += onClosing;
        }

        //New File
        private void MenuItemNew_Click(object sender, RoutedEventArgs e)
        {
            if (needsSave)
            {
                var saveResult = MessageBox.Show("You have unsaved changes. Are you sure you want to create a new file?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
                if (saveResult != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            reset();
        }

        //Open File
        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            if (needsSave)
            {
                var saveResult = MessageBox.Show("You have unsaved changes. Are you sure you want to open this file without saving the file first?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
                if (saveResult != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "PRJ Files(*.PRJ)| *.PRJ| All files(*.*) | *.*";
            openDialog.DefaultExt = ".prj";
            var openResult = openDialog.ShowDialog();
            if (openResult == true)
            {

                reset();

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
                        cmbResources.Items.Add(extData.ResourceName);
                        cmbResources.IsEnabled = true;
                        btnShowResource.IsEnabled = true;
                        btnShowResource.IsChecked = true;
                        btnDeleteResource.IsEnabled = true;
                        cmbResources.SelectedIndex = cmbResources.Items.Count - 1;
                        txtPropertyKey.IsEnabled = true;
                        txtPropertyValue.IsEnabled = true;
                    }
                }
                var combinedImage = combineImages();
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
                MetaData.ColorMapFileName = colormapResult.FileName;
                lblColorFile.Content = Path.GetFileName(MetaData.ColorMapFileName);
                btnShowColor.IsEnabled = true;
                btnShowColor.IsChecked = true;
                checkerBoard = ImageHelper.CreateCheckerboard(1024, 1024, 32, SKColors.DarkBlue, SKColors.Black);
                SKBitmap bmp = combineImages();
                iImage.Source = ImageHelper.GetBitmapFromSKBitmap(bmp);
                btnResource.IsEnabled = true;
                updateSaveStatus(true);
            }
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "JSON Files(*.JSON)| *.JSON| All files(*.*) | *.*";
            saveDialog.DefaultExt = ".json";
            var saveResult = saveDialog.ShowDialog();
            if (saveResult == true)
            {
                TileWorld tileWorld = new TileWorld
                {
                    ColormapFileLocation = MetaData.ColorMapFileName,
                    Width = 1024,
                    Height = 1024,
                    TileSize = 32,
                    SubtileFileLocations = new List<string>()
                };
                tileWorld.SubtileFileLocations = new List<string>();
                //List<SubTile> subTiles = new List<SubTile>();
                for (int y = 0; y < tileWorld.Height / tileWorld.TileSize; y++)
                {
                    for (int x = 0; x < tileWorld.Width / tileWorld.TileSize; x++)
                    {
                        SubTile subTile = new SubTile
                        {
                            X = x * tileWorld.TileSize,
                            Y = y * tileWorld.TileSize,
                            Resources = new List<TileResource>()
                        };
                        if (MetaData.ResourceMetaData != null && MetaData.ResourceMetaData.Count > 0)
                        {
                            foreach (var resource in MetaData.ResourceMetaData)
                            {
                                TileResource newResource = new TileResource
                                {
                                    ResourceName = resource.Key,
                                    Properties = resource.Value.Properties,
                                    ResourceRepColor = resource.Value.RepColor,
                                    ResourceLocations = new List<System.Numerics.Vector2>()
                                };
                                for (int resY = y * tileWorld.TileSize; resY < y * tileWorld.TileSize + tileWorld.TileSize; resY++)
                                {
                                    for (int resX = x * tileWorld.TileSize; resX < x * tileWorld.TileSize + tileWorld.TileSize; resX++)
                                    {
                                        var pixelValue = ResourceBitmaps[resource.Key].OriginalImage.GetPixel(resX, resY);
                                        switch (resource.Value.ResourceColorChannel)
                                        {
                                            case ColorChannel.R:
                                                if (pixelValue.Red >= pixelThresholdValue)
                                                {
                                                    newResource.ResourceLocations.Add(new System.Numerics.Vector2(resX, resY));
                                                }
                                                break;
                                            case ColorChannel.G:
                                                if (pixelValue.Green >= pixelThresholdValue)
                                                {
                                                    newResource.ResourceLocations.Add(new System.Numerics.Vector2(resX, resY));
                                                }
                                                break;
                                            case ColorChannel.B:
                                                if (pixelValue.Blue >= pixelThresholdValue)
                                                {
                                                    newResource.ResourceLocations.Add(new System.Numerics.Vector2(resX, resY));
                                                }
                                                break;
                                        }
                                    }
                                }
                                subTile.Resources.Add(newResource);

                            }
                        }
                        string subfilename = Path.Combine(Path.GetDirectoryName(saveDialog.FileName), Path.GetFileNameWithoutExtension(saveDialog.FileName) + "_" + x + "_" + y + ".json");
                        string subtileData = JsonConvert.SerializeObject(subTile);
                        File.WriteAllText(subfilename, subtileData);
                        tileWorld.SubtileFileLocations.Add(subfilename);
                    }
                }
                var tileworldSerialized = JsonConvert.SerializeObject(tileWorld);
                File.WriteAllText(saveDialog.FileName, tileworldSerialized);
                MessageBox.Show("Export to Json was successful", "Save successful");
                updateSaveStatus(true);
            }
        }

        private void btnResource_Click(object sender, RoutedEventArgs e)
        {
            pnlNewResource.Visibility = Visibility.Visible;
        }

        private void btnLoadResourceMap_Click(object sender, RoutedEventArgs e)
        {
            var resourceResult = loadImageMap();
            if (resourceResult != null)
            {
                var data = new ExtendedResourceData
                {
                    ResourceName = txtResourceName.Text,
                    ResourceFilename = resourceResult.FileName,
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
                data.SKResourceColorChannel = selectedChannel;
                var selectedColor = cpResourceColor.SelectedColor;
                data.SKRepColor = new SKColor(selectedColor.Value.R, selectedColor.Value.G, selectedColor.Value.B);
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
            resetResourceControls();
        }

        private void btnAddResource_Click(object sender, RoutedEventArgs e)
        {
            var data = ResourceBitmaps[txtResourceName.Text];
            data.IsPublished = true;
            data.IsVisible = true;
            data.ConvertedImage = ImageHelper.CreateBitmapFromMask(data.OriginalImage, data.SKResourceColorChannel, data.SKRepColor);
            pnlNewResource.Visibility = Visibility.Collapsed;

            ResourceBitmaps[txtResourceName.Text] = data;
            if (MetaData.ResourceMetaData == null)
            {
                MetaData.ResourceMetaData = new Dictionary<string, ResourceMetaData>();
            }
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
            updateResourceViewControls();
            resetResourceControls();
            btnShowResource.IsEnabled = true;
            btnShowResource.IsChecked = true;
            btnDeleteResource.IsEnabled = true;
            txtPropertyKey.IsEnabled = true;
            txtPropertyValue.IsEnabled = true;
            cmbResources.SelectedIndex = cmbResources.Items.Count - 1;
            //cpResourceColor.SelectedColor = Colors.White;
            txtResourceName.Text = "";
            txtResourceName.IsEnabled = true;
            lblResourceFilename.Content = "Filename...";
            var compiledImage = combineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiledImage);
            updateSaveStatus(true);
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            var compiledImage = combineImages();
            iImage.Source = ImageHelper.GetBitmapFromSKBitmap(compiledImage);
        }

        private void btnShowResource_Click(object sender, RoutedEventArgs e)
        {
            string selectedItem = cmbResources.SelectedItem.ToString();
            var data = ResourceBitmaps[selectedItem];
            data.IsVisible = btnShowResource.IsChecked == true;
            ResourceBitmaps[selectedItem] = data;
            var image = combineImages();
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
                updateResourceViewControls();
                if (cmbResources.IsEnabled = true && cmbResources.Items.Count > 0)
                {
                    cmbResources.SelectedIndex = 0;
                }
                var image = combineImages();
                iImage.Source = ImageHelper.GetBitmapFromSKBitmap(image);
                updateSaveStatus(true);
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


        private void txtProperty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPropertyKey.Text) || string.IsNullOrWhiteSpace(txtPropertyValue.Text))
            {
                btnAddProperty.IsEnabled = false;
            }
            else
            {
                btnAddProperty.IsEnabled = true;
            }
        }

        private void onClosing(object sender, CancelEventArgs e)
        {
            if (needsSave)
            {
                var saveResult = MessageBox.Show("You have unsaved changes. Are you sure you want to quit?", "Unsaved changes", MessageBoxButton.YesNoCancel);
                if (saveResult != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        private SKBitmap combineImages()
        {
            List<SKBitmap> bitmaps = new List<SKBitmap>();
            if (btnShowColor.IsChecked == true)
            {
                bitmaps.Add(colorBMP);
            }
            else
            {
                bitmaps.Add(checkerBoard);
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
            if (change)
            {
                if (!String.IsNullOrWhiteSpace(ProjectFileName))
                {
                    mnuSave.IsEnabled = true;
                }
                needsSave = true;
            }
            else
            {
                mnuSave.IsEnabled = false;
                needsSave = false;
            }
        }

        //Clear all graphics and reset control states
        private void reset()
        {
            //Clear data
            if (MetaData != null && MetaData.ResourceMetaData != null && MetaData.ResourceMetaData.Count > 0)
            {
                MetaData.ResourceMetaData.Clear();
            }
            if (MetaData == null)
            {
                MetaData = new ProjectMetaData();
            }
            MetaData.ColorMapFileName = "";
            if (ResourceBitmaps == null)
            {
                ResourceBitmaps = new Dictionary<string, ExtendedResourceData>();
            }
            ResourceBitmaps.Clear();
            //Disable save menu items
            mnuSave.IsEnabled = false;
            mnuSaveAs.IsEnabled = false;

            //reset controls
            btnLoadColor.IsEnabled = true;
            lblColorFile.Content = filenameRef;
            btnResource.IsEnabled = false;
            resetResourceControls();
            btnShowColor.IsEnabled = false;
            btnShowColor.IsChecked = false;
            btnShowResource.IsEnabled = false;
            btnShowResource.IsChecked = false;
            btnGenerate.IsEnabled = false;
            cmbResources.IsEnabled = false;
            cmbResources.Items.Clear();
            btnDeleteResource.IsEnabled = false;
            txtPropertyKey.IsEnabled = false;
            txtPropertyValue.IsEnabled = false;
            btnAddProperty.IsEnabled = false;
            iImage.Source = null;
            updateSaveStatus(false);
            mnuSaveAs.IsEnabled = false;
        }

        private void resetResourceControls()
        {
            pnlNewResource.Visibility = Visibility.Collapsed;
            txtResourceName.Text = "";
            btnLoadResourceMap.IsEnabled = false;
            lblResourceFilename.Content = filenameRef;
            cmbResourceChannel.IsEnabled = false;
            cpResourceColor.IsEnabled = false;
            btnAddResource.IsEnabled = false;
            btnCancelResource.IsEnabled = false;
        }

        private void updateResourceViewControls()
        {
            cmbResources.Items.Clear();
            if (ResourceBitmaps == null || ResourceBitmaps.Count == 0)
            {
                cmbResources.IsEnabled = false;
            }
            else
            {
                var resourceKeys = ResourceBitmaps.Keys;

                foreach (var key in resourceKeys)
                {
                    cmbResources.Items.Add(key);
                }
                if (cmbResources.IsEnabled == false)
                {
                    cmbResources.IsEnabled = true;

                }
            }
        }
    }
}
