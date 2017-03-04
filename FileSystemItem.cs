using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace FileOrganizer
{
    public class FileSystemItem
    {
        #region Constructors
        /// <summary>
        /// Creates a new file system item
        /// </summary>
        /// <param name="path">The physical path to the file system item</param>
        public FileSystemItem(string path)
        {
            Path = path;
            Name = path;
            if (System.IO.Directory.Exists(path))
            {
                Type = FileSystemType.Directory;
            }
            else if (System.IO.File.Exists(path))
            {
                Type = FileSystemType.File;
            }
            else
            {
                throw new ArgumentException(path + " does not exist.");
            }
        }
        #endregion

        #region Public Properties
        public enum FileSystemType
        {
            File,
            Directory
        }

        public FileSystemType Type
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public string ExpandedPath
        {
            get
            {
                return System.IO.Path.GetFullPath(Environment.ExpandEnvironmentVariables(Path));
            }
        }

        public string Directory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(ExpandedPath);
            }
        }

        public string File
        {
            get
            {
                return System.IO.Path.GetFileName(ExpandedPath);
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Expands the path with the environment variables
        /// </summary>
        /// <param name="path">The path to expand</param>
        /// <returns>The expanded path</returns>
        public static string ExpandPath(string path)
        {
            return System.IO.Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
        }
        /// <summary>
        /// Detects whether the value is a valid file system item
        /// </summary>
        /// <param name="path">The path to the item</param>
        /// <returns>Whether the value is a valid file system item</returns>
        public static bool IsValidPath(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                return true;
            }
            else if (System.IO.File.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
