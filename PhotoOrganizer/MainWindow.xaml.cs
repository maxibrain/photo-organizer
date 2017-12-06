using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using PhotoOrganizer.ViewModel;

namespace PhotoOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();
        }

        private MainViewModel ViewModel
        {
            get => DataContext as MainViewModel;
            set => DataContext = value;
        }

        private void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = e.AllowedEffects & DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private async void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop) ?? new string[0];
                await ViewModel.AddPhotosAsync(fileNames);
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var array = ((FrameworkElement) sender).FindResource("SelectedItems") as ArrayList;
            if (array != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    array.Remove(item);
                }
                array.AddRange(e.AddedItems);
                ImageSource source = null;
                PreviewError.Text = string.Empty;
                if (array.Count > 0)
                {
                    var photos = array.OfType<FolderItemViewModel>().Where(x => x.IsPhoto).ToArray();
                    if (photos.Length > 0)
                    {
                        try
                        {
                            source = new BitmapImage(new Uri(photos.Last().OriginalPath));
                        }
                        catch (Exception exception)
                        {
                            PreviewError.Text = exception.Message;
                        }
                    }
                }
                Preview.Source = source;
            }
        }

        private async void Copy_OnClick(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await ViewModel.CopyAsync(fbd.SelectedPath);
            }
        }

        private void RenameButton_OnClick(object sender, RoutedEventArgs e)
        {
            /*
            var button = (Button) sender;
            var item = (FolderItemViewModel) button.DataContext;
            var mb = new Xceed.Wpf.Toolkit.MessageBox();
            
            ViewModel.Project.ActiveFolder.Rename(item, );
            */
        }

        private void AdjustTakenDateMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
