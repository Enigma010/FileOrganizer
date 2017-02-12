using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileOrganizer
{
    public class ApplicationData
    {
        #region Constants
        public const string ConfigurationDirectoryName = "Config";
        public const string DataDirectoryName = "Data";
        public const string TempDirectoryName = "Temp";
        #endregion

        #region Public Properties
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the directory for a specific application
        /// </summary>
        /// <returns>The directory for a specific application</returns>
        public string AppDirectoryName
        {
            get
            {
                string directoryName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Name);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                return directoryName;
            }
        }

        /// <summary>
        /// Gets the data directory for a specific application
        /// </summary>
        /// <returns>The data directory for a specific application</returns>
        public string AppDataDirectoryName
        {
            get
            {
                string directoryName = Path.Combine(AppDirectoryName, DataDirectoryName);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                return directoryName;
            }
        }

        /// <summary>
        /// Gets the configuration directory for a specific application
        /// </summary>
        /// <returns>The configuration directory for a specific application</returns>
        public string AppConfigDirectoryName
        {
            get
            {
                string directoryName = Path.Combine(AppDirectoryName, ConfigurationDirectoryName);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                return directoryName;
            }
        }

        /// <summary>
        /// Gets the temporary directory for a specific application
        /// </summary>
        /// <returns>The temporary directory for a specific application</returns>
        public string AppTempDirectoryName
        {
            get
            {
                string directoryName = Path.Combine(AppDirectoryName, TempDirectoryName);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                return directoryName;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Deletes all temporary files under an application directory
        /// </summary>
        /// <returns>Whether all the files were deleted from the temporary directory</returns>
        public bool DeleteTempAppDataFiles()
        {
            string directoryName = AppTempDirectoryName;
            bool allDeleted = true;
            foreach (string fileName in Directory.GetFiles(directoryName))
            {
                try
                {
                    File.Delete(Path.Combine(directoryName, fileName));
                }
                catch
                {
                    allDeleted = false;
                }
            }
            return allDeleted;
        }
        #endregion
    }
}
