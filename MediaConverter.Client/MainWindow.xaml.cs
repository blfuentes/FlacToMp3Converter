using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace MediaConverter.Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private string _output;
    public required string Output
    {
        get { return _output; }
        set
        {
            if (_output != value)
            {
                _output = value;
                OnPropertyChanged(nameof(Output));
            }
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog openFolderDialog = new();
        if (openFolderDialog.ShowDialog().GetValueOrDefault())
        {
            this.lblOriginFolderName.Content = openFolderDialog.FolderName;
        }
    }

    private void ChooseTargetFolder_Click(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog openFolderDialog = new();
        if (openFolderDialog.ShowDialog().GetValueOrDefault())
        {
            this.lblTargetFolderName.Content = openFolderDialog.FolderName;
        }
    }

    private void ConvertMusic_Click(object sender, RoutedEventArgs e)
    {

        // Call your DLL method here
        Dispatcher.BeginInvoke(new Action(() =>
        {
            lblStatus.Content = "TRABAJANDO...";
            DualWriter dualWriter = new(Console.Out);
            dualWriter.OutputUpdated += (output) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Output = output;
                    txbOutput.ScrollToEnd();
                }), System.Windows.Threading.DispatcherPriority.Background);

            };
            Console.SetOut(dualWriter);

            string folderContent = this.lblOriginFolderName.Content.ToString();
            string targetFolder = this.lblTargetFolderName.Content.ToString();

            Task.Run(() =>
            {
                ConversorService.Run(folderContent, targetFolder, "flac", "mp3");
                ConversorService.Run(folderContent, targetFolder, "m4a", "mp3");
                ConversorService.Run(folderContent, targetFolder, "mp4", "mp3");
                ConversorService.Run(folderContent, targetFolder, "opus", "mp3");
            }).ContinueWith( _ =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    lblStatus.Content = "TERMINÓ!";

                    if(chkDeleteOrigin.IsChecked.GetValueOrDefault())
                    {
                        if(MessageBoxResult.Yes == MessageBox.Show("¿Desea eliminar los archivos originales?", "Eliminar archivos", MessageBoxButton.YesNo))
                        {
                            var allNewFolders = System.IO.Directory.GetDirectories(targetFolder, "*", System.IO.SearchOption.AllDirectories);
                            foreach (var folder in allNewFolders)
                            {
                                DirectoryInfo directoryInfo = new(folder);
                                var deletePath = System.IO.Path.Join(folderContent, directoryInfo.Name);
                                Directory.Delete(deletePath, true);
                            }
                        }
                    }

                    if(chkOpenTarget.IsChecked.GetValueOrDefault() &&
                        MessageBoxResult.Yes == MessageBox.Show("¿Abrir carpeta con MP3?", "Abrir carpeta", MessageBoxButton.YesNo))
                    {
                        var startInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = targetFolder,
                            UseShellExecute = true,
                            Verb = "open"
                        };
                        System.Diagnostics.Process.Start(startInfo);
                    }

                }), System.Windows.Threading.DispatcherPriority.Background);
            });
        }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

    }
}