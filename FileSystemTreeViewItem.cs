using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;

namespace FileOrganizer
{
    public class FileSystemTreeViewItem : TreeViewItem
    {
        //Delegate invoked when something is added to the tree
        public delegate void ItemAdded(FileSystemTreeViewItem fileSystemTreeViewItem);

        /// <summary>
        /// Creates a new file system tree item
        /// </summary>
        /// <param name="path">The path to the item</param>
        /// <param name="displayPath">The value to display to the user</param>
        /// <param name="fileNamePrefix">The prefix to use for files</param>
        /// <param name="fileNameSuffix">The suffix to use for files</param>
        /// <param name="fileProducer">The delegate used to find files</param>
        /// <param name="itemAddedHandler">The delegate to invoke when a item is added to the tree</param>
        public FileSystemTreeViewItem(string path, string displayPath, string fileNamePrefix, string fileNameSuffix, FileUtilities.GetFile fileProducer, ItemAdded itemAddedHandler)
        {
            FileSystemItem = new FileSystemItem(path);
            FileSystemItem.Name = displayPath;
            FileNamePrefix = fileNamePrefix;
            FileNameSuffix = fileNameSuffix;
            FileProducer = fileProducer;
            ItemAddedHandler = itemAddedHandler;
            Header = FileSystemItem.Name;
        }

        public string FileNamePrefix
        {
            get;
            set;
        }

        public string FileNameSuffix
        {
            get;
            set;
        }

        public FileSystemItem FileSystemItem
        {
            get;
            set;
        }

        public FileUtilities.GetFile FileProducer
        {
            get;
            set;
        }

        public ItemAdded ItemAddedHandler
        {
            get;
            set;
        }

        /// <summary>
        /// Create the tree structure
        /// </summary>
        public void Create()
        {
            //Just to be safe clear the values we have already
            Action action = () =>
            {
                Items.Clear();
            };
            //Application.Current.Dispatcher.BeginInvoke(action);
            //Invoke the delegate that we add this to a tree
            action = () =>
            {
                if (ItemAddedHandler != null)
                {
                    ItemAddedHandler(this);
                }
            };
            Application.Current.Dispatcher.BeginInvoke(action);
            //Now create all the item's children
            Create(FileSystemItem.ExpandedPath);
        }

        /// <summary>
        /// Handle adding an item as appropriate
        /// </summary>
        /// <param name="path">The path of the item</param>
        /// <param name="displayPath">The path to display to the user</param>
        public void Add(string path, string displayPath)
        {
            FileSystemItem file = null;
            if (Directory.Exists(path))
            {
                file = new FileSystemItem(path);
                file.Name = Path.GetFileName(path);
            }
            else if(FileProducer != null && !FileProducer(path, FileNamePrefix, FileNameSuffix, out file))
            {
                return;
            }
            //Add a new item to this tree
            Action action = () =>
            {
                FileSystemTreeViewItem addedTreeViewItem = new FileSystemTreeViewItem(file.ExpandedPath, file.Name, FileNamePrefix, FileNameSuffix, FileProducer, ItemAddedHandler);
                if (addedTreeViewItem.FileSystemItem.Type != FileSystemItem.FileSystemType.Unknown)
                {
                    bool found = false;
                    foreach (object item in Items)
                    {
                        FileSystemTreeViewItem checkFileSystemTreeViewItem = item as FileSystemTreeViewItem;
                        if (checkFileSystemTreeViewItem == null)
                        {
                            continue;
                        }
                        if (checkFileSystemTreeViewItem.FileSystemItem.Name == addedTreeViewItem.FileSystemItem.Name)
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        Items.Add(addedTreeViewItem);
                        if (ItemAddedHandler != null)
                        {
                            ItemAddedHandler(addedTreeViewItem);
                        }
                    }
                };
            };
            Application.Current.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Handle adding an item as appropriate
        /// </summary>
        /// <param name="addedTreeViewItem">The item to add</param>
        public void Add(FileSystemTreeViewItem addedTreeViewItem)
        {
            //Add a new item to this tree
            Action action = () =>
            {
                Items.Add(addedTreeViewItem);
                if (ItemAddedHandler != null)
                {
                    ItemAddedHandler(addedTreeViewItem);
                }
            };
            Application.Current.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Delete a item from the tree
        /// </summary>
        /// <param name="path">The path to the item</param>
        public void Delete(string path)
        {
            bool found = false;
            FileSystemTreeViewItem foundTreeViewItem = null;
            //Search through the existing items to try to find the one we want to delete
            foreach (TreeViewItem item in Items)
            {
                FileSystemTreeViewItem childFileSystemTreeViewItem = item as FileSystemTreeViewItem;
                if (childFileSystemTreeViewItem != null)
                {
                    if (childFileSystemTreeViewItem.FileSystemItem.ExpandedPath == FileSystemItem.ExpandPath(path))
                    {
                        foundTreeViewItem = childFileSystemTreeViewItem;
                        found = true;
                        break;
                    }
                }
            }
            if (found)
            {
                //We found a item time to delete it
                Action action = () =>
                {
                    Items.Remove(foundTreeViewItem);
                };
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }

        /// <summary>
        /// Handle a file system event
        /// </summary>
        /// <param name="fileSystemTreeViewItem">The tree the event occurred on</param>
        /// <param name="eventArguments">The arguments that happened</param>
        /// <param name="itemAddedHandler">The delegate to invoke if a item is added to the tree</param>
        public static void FileWatcherEventHandler(FileSystemTreeViewItem fileSystemTreeViewItem, FileSystemEventArgs eventArguments, ItemAdded itemAddedHandler)
        {
            if (fileSystemTreeViewItem == null)
            {
                return;
            }
            bool correctDepth = false;

            string fileSystemDirectory = fileSystemTreeViewItem.FileSystemItem.ExpandedPath;
            string eventParentDirectoryName = System.IO.Path.GetDirectoryName(eventArguments.FullPath);

            correctDepth = (fileSystemDirectory == eventParentDirectoryName);
            //Figure out if the change happened where we are or if we should look through all our children and see if they need to do something
            if (!correctDepth)
            {
                Action action = () =>
                {
                    //We're not in the right spot, look through all our children and ask them to review the change
                    foreach (TreeViewItem childTreeViewItem in fileSystemTreeViewItem.Items)
                    {
                        FileSystemTreeViewItem childFileSystemTreeViewItem = childTreeViewItem as FileSystemTreeViewItem;
                        if (childFileSystemTreeViewItem != null)
                        {
                            FileWatcherEventHandler(childFileSystemTreeViewItem, eventArguments, itemAddedHandler);
                        }
                    }
                };
                Application.Current.Dispatcher.BeginInvoke(action);
            }
            else
            {
                //We're in the right spot, either delete or add the item as necessary
                if (eventArguments.ChangeType == WatcherChangeTypes.Deleted)
                {
                    fileSystemTreeViewItem.Delete(eventArguments.FullPath);
                }
                else if (eventArguments.ChangeType == WatcherChangeTypes.Created)
                {
                    fileSystemTreeViewItem.Add(eventArguments.FullPath, System.IO.Path.GetFileName(eventArguments.FullPath));
                }
            }
        }

        /// <summary>
        /// Handle a rename event
        /// </summary>
        /// <param name="fileSystemTreeViewItem">The tree view</param>
        /// <param name="eventArguments">The event that occurred</param>
        /// <param name="itemAddedHandler">The delegate to invoke when a item is added</param>
        public static void FileWatcherRenameEventHandler(FileSystemTreeViewItem fileSystemTreeViewItem, RenamedEventArgs eventArguments, ItemAdded itemAddedHandler)
        {
            //Change a rename to a delete and add action run those actions
            FileSystemEventArgs addedEventArgs = new FileSystemEventArgs(WatcherChangeTypes.Created, System.IO.Path.GetDirectoryName(eventArguments.FullPath), System.IO.Path.GetFileName(eventArguments.FullPath));
            FileSystemEventArgs deletedEventArgs = new FileSystemEventArgs(WatcherChangeTypes.Deleted, System.IO.Path.GetDirectoryName(eventArguments.OldFullPath), System.IO.Path.GetFileName(eventArguments.OldFullPath));
            FileWatcherEventHandler(fileSystemTreeViewItem, addedEventArgs, itemAddedHandler);
            FileWatcherEventHandler(fileSystemTreeViewItem, deletedEventArgs, itemAddedHandler);
        }

        /// <summary>
        /// Creates all the children beneath the path
        /// </summary>
        /// <param name="path">The path</param>
        private void Create(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return;
            }
            //Go through all the subdirectories in the path, doing this first makes it so the directories are at the top of the tree
            foreach (string subDirectory in Directory.GetDirectories(path))
            {
                Action action = () =>
                {
                    FileSystemTreeViewItem subDirectoryTreeItem = new FileSystemTreeViewItem(subDirectory, System.IO.Path.GetFileName(subDirectory), FileNamePrefix, FileNameSuffix, FileProducer, ItemAddedHandler);
                    if (subDirectoryTreeItem.FileSystemItem.Type != FileSystemItem.FileSystemType.Unknown)
                    {
                        //Add a node to the current tree
                        Add(subDirectoryTreeItem);
                        //Call create to add all children under this node recursively
                        subDirectoryTreeItem.Create();
                    }
                };
                Application.Current.Dispatcher.BeginInvoke(action);
            }
            //Invoke the delegate to return the files appropriate to this tree
            List<FileSystemItem> files = FileUtilities.GetFiles(FileSystemItem.ExpandedPath, FileNamePrefix, FileNameSuffix, FileProducer);
            //Go through all the files
            foreach (FileSystemItem file in files)
            {
                //Add files to the tree
                Action action = () =>
                {
                    FileSystemTreeViewItem fileTreeItem = new FileSystemTreeViewItem(file.ExpandedPath, file.Name, FileNamePrefix, FileNameSuffix, FileProducer, ItemAddedHandler);
                    if (fileTreeItem.FileSystemItem.Type != FileSystemItem.FileSystemType.Unknown)
                    {
                        Add(fileTreeItem);
                        fileTreeItem.Create();
                    }
                };
                Application.Current.Dispatcher.BeginInvoke(action);
            }
        }
    }
}
