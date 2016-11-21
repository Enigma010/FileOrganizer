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

        public string Directory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Path);
            }
        }

        public string File
        {
            get
            {
                return System.IO.Path.GetFileName(Path);
            }
        }
        #endregion

        #region Public Methods
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
