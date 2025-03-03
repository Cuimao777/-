using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using static System.IO.Path;

namespace DesktopOrganizer
{
    public class DropZone : INotifyPropertyChanged
    {
        private string _name;
        private string _path;
        private int _fileCount;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public int FileCount
        {
            get => _fileCount;
            set
            {
                _fileCount = value;
                OnPropertyChanged(nameof(FileCount));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<DropZone> _dropZones = new ObservableCollection<DropZone>();
        public ObservableCollection<DropZone> DropZones
        {
            get => _dropZones;
            set
            {
                _dropZones = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DropZones)));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // 创建默认存储目录
            string baseDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OrganizedFiles");
            Directory.CreateDirectory(baseDir);

            // 初始化默认区域
            for (int i = 1; i <= 4; i++)
            {
                string areaPath = System.IO.Path.Combine(baseDir, $"Area{i}");
                Directory.CreateDirectory(areaPath);
                
                DropZones.Add(new DropZone
                {
                    Name = $"区域{i}",
                    Path = areaPath,
                    FileCount = Directory.GetFiles(areaPath).Length
                });
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Opacity = e.NewValue;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void EditName_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var dropZone = menuItem?.Tag as DropZone;
            if (dropZone == null) return;

            var dialog = new InputDialog("修改名称", "请输入新的名称：", dropZone.Name);
            if (dialog.ShowDialog() == true)
            {
                dropZone.Name = dialog.ResponseText;
            }
        }

        private void CustomizePath_Click(object sender, RoutedEventArgs e)
        {
            DropZone dropZone = null;
            if (sender is MenuItem menuItem)
            {
                dropZone = menuItem.Tag as DropZone;
            }
            else if (e.Source is FrameworkElement element)
            {
                dropZone = element.DataContext as DropZone;
            }

            if (dropZone == null) return;

            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "选择目标文件夹",
                    UseDescriptionForTitle = true,
                    SelectedPath = dropZone.Path // 设置初始路径为当前路径
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var oldPath = dropZone.Path;
                    var newPath = dialog.SelectedPath;

                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }

                    // 移动现有文件到新路径
                    if (Directory.Exists(oldPath))
                    {
                        foreach (var file in Directory.GetFiles(oldPath))
                        {
                            try
                            {
                                var fileName = System.IO.Path.GetFileName(file);
                                var destFile = System.IO.Path.Combine(newPath, fileName);
                                if (File.Exists(destFile))
                                {
                                    var result = MessageBox.Show($"文件 {fileName} 已存在于目标文件夹中，是否覆盖？",
                                        "文件已存在", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                    if (result == MessageBoxResult.No) continue;
                                }
                                File.Move(file, destFile, true);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"移动文件时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }

                    dropZone.Path = newPath;
                    dropZone.FileCount = Directory.GetFiles(newPath).Length;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置路径时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddNewZone_Click(object sender, RoutedEventArgs e)
        {
            if (DropZones.Count < 8)
            {
                var newIndex = DropZones.Count + 1;
                DropZones.Add(new DropZone 
                { 
                    Name = $"区域{newIndex}",
                    Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OrganizedFiles", $"Area{newIndex}"),
                    FileCount = 0
                });
                Directory.CreateDirectory(DropZones.Last().Path);
            }
        }

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            var border = sender as Border;
            var targetPath = border?.Tag?.ToString();
            
            if (!string.IsNullOrEmpty(targetPath) && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    try
                    {
                        var destFile = System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(file));
                        File.Move(file, destFile);
                        var dropZone = DropZones.FirstOrDefault(d => d.Path == targetPath);
                        if (dropZone != null) dropZone.FileCount++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"文件移动失败：{ex.Message}");
                    }
                }
            }
        }
        private void DropZone_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var border = sender as Border;
                if (border?.DataContext is DropZone dropZone)
                {
                    var menuItem = new MenuItem { Tag = dropZone };
                    CustomizePath_Click(menuItem, new RoutedEventArgs());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理右键菜单时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
