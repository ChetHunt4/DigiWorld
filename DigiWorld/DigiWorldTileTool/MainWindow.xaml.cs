using DigiWorldTileTool.Helpers;
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


namespace DigiWorldTileTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SKBitmap _mask { get; set; }
        private SKBitmap _map { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoadMask_Click(object sender, RoutedEventArgs e)
        {
            var fileLoad = new OpenFileDialog();
            fileLoad.Filter = "PNG Files(*.PNG)| *.PNG| All files(*.*) | *.*";
            fileLoad.DefaultExt = ".png";
            var fileResult = fileLoad.ShowDialog();
            if (fileResult == true)
            {
                _mask = ImageHelper.LoadBitmapFromFile(fileLoad.FileName);
                imgMask.Source = ImageHelper.GetBitmapFromSKBitmap(_mask);
                cmbMaskColorChannel.IsEnabled = true;

            }
            updatePreview();
        }

        private void btnLoadMap_Click(object sender, RoutedEventArgs e)
        {
            var fileLoad = new OpenFileDialog();
            fileLoad.Filter = "PNG Files(*.PNG)| *.PNG| All files(*.*) | *.*";
            fileLoad.DefaultExt = ".png";
            var fileResult = fileLoad.ShowDialog();
            if (fileResult == true)
            {
                _map = ImageHelper.LoadBitmapFromFile(fileLoad.FileName);
                imgMap.Source = ImageHelper.GetBitmapFromSKBitmap(_map);
            }
            updatePreview();
        }

        private void cmbMaskColorChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updatePreview();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PNG Files(*.PNG)| *.PNG| All files(*.*) | *.*";
            saveDialog.DefaultExt = ".png";
            var saveResult = saveDialog.ShowDialog();
            if (saveResult == true)
            {
                var filename = saveDialog.FileName;
                var path = Path.GetDirectoryName(filename);
                var ext = Path.GetExtension(filename);
                var file = Path.GetFileName(filename);
                SKColorChannel colorChannel = GetColorChannelFromComboBox();
                var bitmaps = ImageHelper.CreateBitmapTilesFromMask(_mask, colorChannel, _map);
                for (int i = 0; i < bitmaps.Count; i++)
                {
                    var suffix = "";
                    switch (i)
                    {
                        case 0:
                            suffix = "UL";
                            break;
                        case 1: 
                            suffix = "UM";
                            break;
                        case 2:
                            suffix = "UR";
                            break;
                        case 3:
                            suffix = "ML";
                            break;
                        case 4:
                            suffix = "MM";
                            break;
                        case 5:
                            suffix = "MR";
                            break;
                        case 6:
                            suffix = "LL";
                            break;
                        case 7:
                            suffix = "LM";
                            break;
                        case 8:
                            suffix = "LR";
                            break;
                    }
                    var fullPath = Path.Combine(path, file + "_" + suffix + ext);
                    ImageHelper.SaveImageToFile(fullPath, bitmaps[i], SKEncodedImageFormat.Png);
                }
            }
        }

        private void updatePreview()
        {
            if (_map != null && _mask != null && cmbMaskColorChannel.SelectionBoxItem != null)
            {
                SKColorChannel colorChannel = GetColorChannelFromComboBox();
                SKBitmap preview = ImageHelper.CreateBitmapPreviewFromMask(_mask, colorChannel, _map);
                imgPreview.Source = ImageHelper.GetBitmapFromSKBitmap(preview);
            }
        }

        private SKColorChannel GetColorChannelFromComboBox()
        {
            SKColorChannel colorChannel = SKColorChannel.R;
            var selectedIndex = cmbMaskColorChannel.SelectedIndex;
            var selectedItem = cmbMaskColorChannel.Items[selectedIndex] as ComboBoxItem;
            var color = selectedItem.Content as string;
            switch (color)
            {
                case "R":
                    colorChannel = SKColorChannel.R;
                    break;
                case "G":
                    colorChannel = SKColorChannel.G;
                    break;
                case "B":
                    colorChannel = SKColorChannel.B;
                    break;
            }
            return colorChannel;
        }


    }
}
