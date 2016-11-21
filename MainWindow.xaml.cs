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

using System.IO;
using System.Diagnostics;
using System.Threading;

namespace FileOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Populate the source directory tree, find all files in their original state
            GetDirectories(trVwFilesToOrganize, Settings.Default.FilesToOrganizeDirectory, string.Empty, string.Empty, FileUtilities.GetEveryFile);
            //Populate the destination directory tree, find the files matching the desitnation file pattern
            GetDirectories(trVwOrganizedFiles, Settings.Default.OrganizedFilesDirectory, Settings.Default.OrganizedFileNamePrefix, Settings.Default.OrganizedFileNameSuffix, FileUtilities.GetPatternedFile);
            //Set the move button to the right state
            btnMove.IsEnabled = IsMoveEnabled();
            //Configure a file watcher on the source directory to detect files moving in and out of the directory
            ConfigureFileSystemWatcher(Settings.Default.FilesToOrganizeDirectory, _filesToOrganizePathWatcher, this.trVwFilesToOrganizeRenamed, this.trVwFilesToOrganizeUpdated);
            //Configure a file watcher on the destination directory to detect files moving in and out of the directory
            ConfigureFileSystemWatcher(Settings.Default.OrganizedFilesDirectory, _organizedFilesPathWatcher, this.trVwOrganizedFilesRenamed, this.trVwOrganizedFileUpdated);
            //Delete any temporary files from previous runs
            FileUtilities.DeleteTempAppDataFiles("FileOrganizer");
            //Sets the date picker state
            CheckBox_Checked(null, null);
        }

        /// <summary>
        /// Populates a tree view from a directory
        /// </summary>
        /// <param name="treeView">The tree view to populate</param>
        /// <param name="directoryName">The root directory to get the structures from</param>
        /// <param name="fileNamePrefix">The file name prefix</param>
        /// <param name="fileNameSuffix">The file name suffix</param>
        /// <param name="fileProducer">The producer to get the list of files in a directory</param>
        private void GetDirectories(TreeView treeView, string directoryName, string fileNamePrefix, string fileNameSuffix, FileUtilities.GetFile fileProducer)
        {
            FileSystemTreeViewItem fileSystemTreeViewItem = new FileSystemTreeViewItem(directoryName, directoryName, fileNamePrefix, fileNameSuffix, fileProducer, this.SytlizeFileSystemTreeViewItem);
            treeView.Items.Add(fileSystemTreeViewItem);
            fileSystemTreeViewItem.Create();
            //Thread thread = new Thread(fileSystemTreeViewItem.Create);
            //thread.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Restores the previous windows width and height
            if (Settings.Default.MainWindowHeight != 0)
            {
                this.Height = Settings.Default.MainWindowHeight;
            }
            if(Settings.Default.MainWindowWidth != 0)
            {
                this.Width = Settings.Default.MainWindowWidth;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Saves the current windows width and height
            Settings.Default.MainWindowHeight = this.Height;
            Settings.Default.MainWindowWidth = this.Width;
            Settings.Default.Save();
        }

        private void trVwOrganizedFiles_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FileSystemTreeViewItem selectedItem = e.NewValue as FileSystemTreeViewItem;
            //If the user selected a file from the destination get the name and set the destination file name to what you selected
            if(selectedItem != null && selectedItem.FileSystemItem.Type == FileSystemItem.FileSystemType.File)
            {
                txtFileName.Text = System.IO.Path.GetFileNameWithoutExtension(selectedItem.FileSystemItem.Name);
            }
            //Set the move buttons state appropriately
            btnMove.IsEnabled = IsMoveEnabled();
        }

        private void trVwFilesToOrganize_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //Set the move buttons state appropriately
            btnMove.IsEnabled = IsMoveEnabled();
        }

        private void trVwFilesToOrganize_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Open a preview up on the item that the user double clicked
            FileSystemTreeViewItem sourceFile = trVwFilesToOrganize.SelectedItem as FileSystemTreeViewItem;
            if(sourceFile != null && sourceFile.FileSystemItem.Type == FileSystemItem.FileSystemType.File)
            {
                //Copy the file clicked to the application temporary directory. This allows the file to be previewed multiple times if the
                //application that opens the file gets a lock on it
                string destinationPath = string.Empty;
                if(!FileUtilities.CreateAppDataTemporaryFile("FileOrganizer", sourceFile.FileSystemItem.Path, false, MessageHandler, RespondMessageHandler, out destinationPath))
                {
                    return;
                }
                //Start a process to handle displaying the file
                Process.Start(destinationPath);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //Sets the state of the date picker appropriately
            if (dtPkrDate != null && chkBxDated.IsChecked != null)
            {
                dtPkrDate.IsEnabled = (bool)chkBxDated.IsChecked;
            }
        }

        private void SytlizeFileSystemTreeViewItem(FileSystemTreeViewItem fileSystemTreeViewItem)
        {
            //The FileSystemTreeViewItem figures out what the tree should look like this is in charge of making it look right
            if(fileSystemTreeViewItem != null)
            {
                //Because this is on a black background change the text to be white
                fileSystemTreeViewItem.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void MessageHandler(string message)
        {
            //Delegate to handle showing the message to the user
            MessageBox.Show(message);
        }

        private MessageBoxResult RespondMessageHandler(string message)
        {
            //Delegate to handle showing the message and getting the user response
            return MessageBox.Show(message, string.Empty, MessageBoxButton.YesNo);
        }

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMoveEnabled())
            {
                return;
            }
            FileSystemTreeViewItem sourceFile = trVwFilesToOrganize.SelectedItem as FileSystemTreeViewItem;
            FileSystemTreeViewItem destinationFile = trVwOrganizedFiles.SelectedItem as FileSystemTreeViewItem;

            string extension = string.Empty;
            string moveToDirectoryName = string.Empty;
            extension = System.IO.Path.GetExtension(sourceFile.FileSystemItem.Path);

            //The destination is the destination name that the user put in
            string newFileName = txtFileName.Text + extension;
            
            //Add the prefix and suffix defaults value to the new file name
            newFileName = FileUtilities.AddPrefixAndSuffixToFileName(newFileName, Settings.Default.OrganizedFileNamePrefix, Settings.Default.OrganizedFileNameSuffix);

            //Figure out what to do with the date
            bool isChecked = false;
            if(chkBxDated.IsChecked != null)
            {
                isChecked = (bool)chkBxDated.IsChecked;
            }
            if (isChecked)
            {
                DateTime theDate = DateTime.Now;
                if(dtPkrDate.SelectedDate != null)
                {
                    theDate = (DateTime)dtPkrDate.SelectedDate;
                }
                //Add a date to the new file name
                newFileName = FileUtilities.ReplaceDateFileNamePattern(newFileName, theDate, txtFileName.Text);
            }
            else
            {
                //No date add the default pattern to the file name
                newFileName = FileUtilities.ReplaceNoDateFileNamePattern(newFileName, txtFileName.Text);
            }
            //Figure out which directory to move it into
            if(destinationFile.FileSystemItem.Type == FileSystemItem.FileSystemType.Directory)
            {
                //It's a directory just use the value
                moveToDirectoryName = destinationFile.FileSystemItem.Path;
            }
            else
            {
                //Find the directory of the file that the user has selected
                moveToDirectoryName = System.IO.Path.GetDirectoryName(destinationFile.FileSystemItem.Path);
            }
            //Call the API to try to move the file
            if (FileUtilities.MoveFile(sourceFile.FileSystemItem.Path,
                System.IO.Path.Combine(moveToDirectoryName, newFileName),
                this.MessageHandler,
                this.RespondMessageHandler))
            {
                FileSystemEventArgs eventArgs = new FileSystemEventArgs(WatcherChangeTypes.Created, moveToDirectoryName, newFileName);
                trVwOrganizedFileUpdated(this, eventArgs);
            }
        }

        private void trVwFilesToOrganizeRenamed(object sender, RenamedEventArgs eventArguments)
        {
            //Handle a file being renamed in the source tree
            TreeViewRenamed(trVwFilesToOrganize, eventArguments);
        }

        private void trVwFilesToOrganizeUpdated(object sender, FileSystemEventArgs eventArguments)
        {
            //Handle a file being updated in the source tree
            TreeViewUpdated(trVwFilesToOrganize, eventArguments);
        }

        private void trVwOrganizedFilesRenamed(object sender, RenamedEventArgs eventArguments)
        {
            //Handle a file being renamed in the destination tree
            TreeViewRenamed(trVwOrganizedFiles, eventArguments);
        }

        private void trVwOrganizedFileUpdated(object sender, FileSystemEventArgs eventArguments)
        {
            //Handle a file being renamed in the destination tree
            TreeViewUpdated(trVwOrganizedFiles, eventArguments);
        }

        private void TreeViewRenamed(TreeView treeView, RenamedEventArgs eventArguments)
        {
            if(treeView == null)
            {
                return;
            }
            if (treeView.Items == null || treeView.Items.Count == 0)
            {
                return;
            }
            //Actually handle the rename action that happened to the file in the tree
            FileSystemTreeViewItem.FileWatcherRenameEventHandler(treeView.Items[0] as FileSystemTreeViewItem, eventArguments, this.SytlizeFileSystemTreeViewItem);
        }

        private void TreeViewUpdated(TreeView treeView, FileSystemEventArgs eventArguments)
        {
            if(treeView == null)
            {
                return;
            }
            if(treeView.Items == null || treeView.Items.Count == 0)
            {
                return;
            }
            //Actually handle the update action that happened to the file in the tree
            FileSystemTreeViewItem.FileWatcherEventHandler(treeView.Items[0] as FileSystemTreeViewItem, eventArguments, this.SytlizeFileSystemTreeViewItem);
        }

        private bool IsMoveEnabled()
        {
            //Move is only enabled if we have a source selected, a destination selected, and they've put in a name to move it to
            FileSystemTreeViewItem sourceFile = trVwFilesToOrganize.SelectedItem as FileSystemTreeViewItem;
            if(sourceFile == null || sourceFile.FileSystemItem.Type != FileSystemItem.FileSystemType.File)
            {
                return false;
            }
            FileSystemTreeViewItem destinationFile = trVwOrganizedFiles.SelectedItem as FileSystemTreeViewItem;
            if(destinationFile == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(txtFileName.Text))
            {
                return false;
            }
            return true;
        }

        private void ConfigureFileSystemWatcher(string path, FileSystemWatcher fileSystemWatcher, RenamedEventHandler renamedHandler, FileSystemEventHandler fileSystemHandler)
        {
            //Set the file system watcher
            fileSystemWatcher = new FileSystemWatcher();
            //The path to watch
            fileSystemWatcher.Path = path;
            //All the event handlers
            fileSystemWatcher.Renamed += renamedHandler;
            fileSystemWatcher.Created += fileSystemHandler;
            fileSystemWatcher.Deleted += fileSystemHandler;
            //Only enable the watcher if the directory is a real directory
            fileSystemWatcher.EnableRaisingEvents = Directory.Exists(fileSystemWatcher.Path);
        }

        private void txtDestinationFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnMove.IsEnabled = IsMoveEnabled();
        }

        private FileSystemWatcher _filesToOrganizePathWatcher = null;
        private FileSystemWatcher _organizedFilesPathWatcher = null;
    }
}
