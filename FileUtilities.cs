using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

using System.IO;
using System.Text.RegularExpressions;

namespace FileOrganizer
{
    public class FileUtilities
    {
        #region Constants
        public const string Delimiter = "%";
        public const string Year = "Y";
        public const string Month = "M";
        public const string Day = "D";
        public const string Page = "P";
        #endregion

        //Delegate to retrieve the appropriate files for a directory
        public delegate List<FileSystemItem> GetFilesNames(string directoryName, string fileNamePrefix, string fileNameSuffix);
        //Delegate to retrieve the appropriate file
        public delegate bool GetFile(string path, string fileNamePrefix, string fileNameSuffix, out FileSystemItem fileSystemItem);
        //Delegate to handle displaying a message to the user
        public delegate void Message(string message);
        //Delegate to handle displaying a message and getting a response from the user
        public delegate MessageBoxResult RespondToMessage(string message);

        public static string YearPlaceHolder
        {
            get
            {
                return Delimiter + Year;
            }
        }

        public static string MonthPlaceHolder
        {
            get
            {
                return Delimiter + Month;
            }
        }

        public static string DayPlaceHolder
        {
            get
            {
                return Delimiter + Day;
            }
        }

        public static string PagePlaceHolder
        {
            get
            {
                return Delimiter + Page;
            }
        }

        #region Public Methods
        /// <summary>
        /// Returns the general pattern for a specific file
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="fileNamePrefix">The prefix of the file</param>
        /// <param name="fileNameSuffix">The suffix for the file</param>
        /// <returns>The general pattern for a specific file</returns>
        public static string AddPrefixAndSuffixToFileName(string path, string fileNamePrefix, string fileNameSuffix)
        {
            string fileNameNoExtension = Path.GetFileNameWithoutExtension(path);
            string directoryName = Path.GetDirectoryName(path);
            string extension = Path.GetExtension(path);
            return Path.Combine(directoryName, fileNamePrefix + fileNameNoExtension + fileNameSuffix  + extension);
        }

        /// <summary>
        /// Replaces a general pattern file name with a specific date
        /// </summary>
        /// <param name="pattern">The pattern for the file name</param>
        /// <param name="dateTime">The date to substitute into the pattern</param>
        /// <param name="pageNumber">An optional page number to use</param>
        /// <returns>The actual file name to use</returns>
        public static string ReplaceDateFileNamePattern(string pattern, DateTime dateTime, string pageNumber = "")
        {
            return ReplaceFileNamePattern(pattern, dateTime.Year.ToString(), dateTime.Month.ToString().PadLeft(2, '0'), dateTime.Day.ToString().PadLeft(2, '0'), pageNumber);
        }

        /// <summary>
        /// Replaces a general pattern file name with the no date
        /// </summary>
        /// <param name="pattern">The pattern for the file name</param>
        /// <param name="pageNumber">An optional page number to use</param>
        /// <returns>The actual file name to use</returns>
        public static string ReplaceNoDateFileNamePattern(string pattern, string pageNumber = "")
        {
            return ReplaceFileNamePattern(pattern, "XXXX", "XX", "XX", pageNumber);
        }

        /// <summary>
        /// Gets a pattern to use to see if a files name matches the pattern
        /// </summary>
        /// <param name="pattern">The string pattern of the file</param>
        /// <returns>A regular expression to use for matching</returns>
        public static Regex GetDatedFileNamePattern(string pattern)
       {
            return new Regex(ReplaceFileNamePattern(pattern, "[0-9]{4}", "[0-9]{2}", "[0-9]{2}", "[0-9]{1,}"));
        }

        /// <summary>
        /// Gets a pattern to use to see if a files name matches the pattern
        /// </summary>
        /// <param name="pattern">The string pattern of the file</param>
        /// <returns>A regular expression to use for matching</returns>
        public static Regex GetNoDateFileNamePattern(string pattern)
        {
            return new Regex(ReplaceFileNamePattern(pattern, "XXXX", "XX", "XX", "[0-9]{1,}"));
        }

        /// <summary>
        /// Gets the directory for a specific application
        /// </summary>
        /// <param name="applicationName">The name of the application</param>
        /// <returns>The name of the directory to use</returns>
        public static string AppDataDirectoryName(string applicationName)
        {
            string applicationDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);
            if (!Directory.Exists(applicationDataFolder))
            {
                Directory.CreateDirectory(applicationDataFolder);
            }
            return applicationDataFolder;
        }

        /// <summary>
        /// Deletes all temporary files under an application directory
        /// </summary>
        /// <param name="applicationName">The name of the application</param>
        public static void DeleteTempAppDataFiles(string applicationName)
        {
            string applicationDataFolder = AppDataDirectoryName(applicationName);
            foreach(string fileName in Directory.GetFiles(applicationDataFolder))
            {
                try
                {
                    File.Delete(Path.Combine(applicationDataFolder, fileName));
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Creates a directory under the application directory
        /// </summary>
        /// <param name="applicationName">The name of the application</param>
        /// <param name="path">The path to the original file</param>
        /// <param name="confirmDestinationDelete">Whether or not to ask the user prior to the file being deleted</param>
        /// <param name="messageHandler">A delegate to invoke if a message needs to be displayed</param>
        /// <param name="messageRespondHandler">A delegate to invoke if a message needs to be displayed and responded to</param>
        /// <param name="destinationPath">The path to the destination file created</param>
        /// <returns>Whether or not the destination file was created</returns>
        public static bool CreateAppDataTemporaryFile(string applicationName, string path, bool confirmDestinationDelete, Message messageHandler, RespondToMessage messageRespondHandler, out string destinationPath)
        {
            destinationPath = string.Empty;
            string appDataDirectoryName = AppDataDirectoryName(applicationName);
            string fileName = Path.GetFileName(path);
            destinationPath = Path.Combine(appDataDirectoryName, fileName);
            return CopyFile(path, destinationPath, confirmDestinationDelete, messageHandler, messageRespondHandler);
        }

        /// <summary>
        /// Gets a list of files matching a specific pattern
        /// </summary>
        /// <param name="directoryName">The directory to look under</param>
        /// <param name="fileNamePrefix">The prefix of the file name</param>
        /// <param name="fileNameSuffix">The prefix of the file name</param>
        /// <returns>A list of files matching a specific pattern</returns>
        public static List<FileSystemItem> GetPatternedFileNames(string directoryName, string fileNamePrefix, string fileNameSuffix)
        {
            List<FileSystemItem> files = new List<FileSystemItem>();
            if (string.IsNullOrEmpty(directoryName) || !Directory.Exists(directoryName))
            {
                return files;
            }
            foreach (string path in Directory.GetFiles(directoryName))
            {
                FileSystemItem file = null;
                if (GetPatternedFile(path, fileNamePrefix, fileNameSuffix, out file))
                {
                    files.Add(file);
                }
            }
            return files;
        }

        /// <summary>
        /// Tests whether the file matches the pattern and then returns the it's representation
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="fileNamePrefix">The prefix for the file</param>
        /// <param name="fileNameSuffix">The suffix for the file</param>
        /// <param name="fileSystemItem">The file system item</param>
        /// <returns>Whether or not the file matches the pattern we're looking for</returns>
        public static bool GetPatternedFile(string path, string fileNamePrefix, string fileNameSuffix, out FileSystemItem fileSystemItem)
        {
            fileSystemItem = null;
            string fileName = Path.GetFileName(path);
            string extension = "[.](.*?)";
            Regex datedFileNamePattern = FileUtilities.GetDatedFileNamePattern(Regex.Escape(fileNamePrefix) + "(.*?)" + Regex.Escape(fileNameSuffix) + extension);
            Regex noDateFileNamePattern = FileUtilities.GetNoDateFileNamePattern(Regex.Escape(fileNamePrefix) + "(.*?)" + Regex.Escape(fileNameSuffix) + extension);
            Regex matchedPattern = null;
            if (datedFileNamePattern.IsMatch(fileName))
            {
                matchedPattern = datedFileNamePattern;
            }
            if (noDateFileNamePattern.IsMatch(fileName))
            {
                matchedPattern = noDateFileNamePattern;
            }
            if (matchedPattern == null)
            {
                return false;
            }
            else
            {
                Match fileNameMatch = matchedPattern.Match(fileName);
                fileSystemItem = new FileSystemItem(path);
                fileSystemItem.Name = fileNameMatch.Groups[1].Value;
                return true;
            }
        }

        /// <summary>
        /// Gets a list of all files in a directory
        /// </summary>
        /// <param name="directoryName">The name of the directory</param>
        /// <param name="fileNamePrefix">The prefix of the file name</param>
        /// <param name="fileNameSuffix">The suffix of the file name</param>
        /// <returns>A list of all files in a directory</returns>
        public static List<FileSystemItem> GetAllFileNames(string directoryName, string fileNamePrefix, string fileNameSuffix)
        {
            List<FileSystemItem> files = new List<FileSystemItem>();
            if (string.IsNullOrEmpty(directoryName) || !Directory.Exists(directoryName))
            {
                return files;
            }
            foreach (string fileName in Directory.GetFiles(directoryName))
            {
                FileSystemItem file = null;
                if (GetEveryFile(Path.Combine(directoryName, fileName), fileNamePrefix, fileNameSuffix, out file))
                {
                    files.Add(file);
                }
            }
            return files;
        }

        /// <summary>
        /// Gets the files for a directory
        /// </summary>
        /// <param name="directoryName">The directory name</param>
        /// <param name="fileNamePrefix">The name prefixing the file</param>
        /// <param name="fileNameSuffix">The name sufixing the file</param>
        /// <param name="fileProducer">The delegate to get the file</param>
        /// <returns>A list of files</returns>
        public static List<FileSystemItem> GetFiles(string directoryName, string fileNamePrefix, string fileNameSuffix, GetFile fileProducer)
        {
            List<FileSystemItem> files = new List<FileSystemItem>();
            if (string.IsNullOrEmpty(directoryName) || !Directory.Exists(directoryName))
            {
                return files;
            }
            foreach (string fileName in Directory.GetFiles(directoryName))
            {
                FileSystemItem file = null;
                if(fileProducer(Path.Combine(directoryName, fileName), fileNamePrefix, fileNameSuffix, out file))
                {
                    bool found = false;
                    foreach(FileSystemItem checkFile in files)
                    {
                        if(checkFile.Name == file.Name)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        files.Add(file);
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// Gets the file representation of the file
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="fileNamePrefix">The prefix for the file</param>
        /// <param name="fileNameSuffix">The suffix for the file</param>
        /// <param name="fileSystemItem">The file system item</param>
        /// <returns>Returns whether or not the path represents a file</returns>
        public static bool GetEveryFile(string path, string fileNamePrefix, string fileNameSuffix, out FileSystemItem fileSystemItem)
        {
            fileSystemItem = null;
            if (!File.Exists(path))
            {
                return false;
            }
            fileSystemItem = new FileSystemItem(path);
            fileSystemItem.Name = Path.GetFileName(path);
            fileSystemItem.Path = path;
            fileSystemItem.Type = FileSystemItem.FileSystemType.File;
            return true;
        }

        /// <summary>
        /// Moves a file between one directory and another
        /// </summary>
        /// <param name="sourcePath">The source path</param>
        /// <param name="destinationPath">The destination path</param>
        /// <param name="messageHandler">The delegate to use to display messages</param>
        /// <param name="messageRespondHandler">The delegate to use to display message and get a response</param>
        /// <returns>Whether the file is moved or not</returns>
        public static bool MoveFile(string sourcePath, string destinationPath, Message messageHandler, RespondToMessage messageRespondHandler)
        {
            if (!File.Exists(sourcePath))
            {
                if (messageHandler != null)
                {
                    messageHandler("The file " + sourcePath + " does not exist.");
                }
                return false;
            }
            if (File.Exists(destinationPath))
            {
                if(messageRespondHandler == null)
                {
                    return false;
                }
                MessageBoxResult result = messageRespondHandler("The file " + destinationPath + " exists.  Do you want to delete it prior to the move?");
                if (!MessageBoxResponseIsAffirmitive(result))
                {
                    return false;
                }
                try
                {
                    File.Delete(destinationPath);
                }
                catch (Exception exception)
                {
                    messageHandler("Failed to delete file " + destinationPath + ". " + exception.Message);
                    return false;
                }
            }
            try
            {
                File.Move(sourcePath, destinationPath);
                return true;
            }
            catch (Exception exception)
            {
                messageHandler("Failed to move file " + sourcePath + " to " + destinationPath + ". " + exception.Message);
                return false;
            }
        }

        /// <summary>
        /// Copies a file from one place to another
        /// </summary>
        /// <param name="sourcePath">The source path</param>
        /// <param name="destinationPath">The destination path</param>
        /// <param name="confirmDestinationDelete">Whether to confirm deleting an existing file or not</param>
        /// <param name="messageHandler">The delegate to use to display messages</param>
        /// <param name="messageRespondHandler">The delegate to use to display message and get a response</param>
        /// <returns>Whether or not the file was copied or not</returns>
        public static bool CopyFile(string sourcePath, string destinationPath, bool confirmDestinationDelete, Message messageHandler, RespondToMessage messageRespondHandler)
        {
            if (!File.Exists(sourcePath))
            {
                messageHandler("The file" + sourcePath + " does not exist.");
                return false;
            }
            if (File.Exists(destinationPath))
            {
                if (confirmDestinationDelete)
                {
                    if(messageRespondHandler == null)
                    {
                        messageHandler("The file " + destinationPath + " already exists.");
                        return false;
                    }
                    MessageBoxResult result = messageRespondHandler("The file " + destinationPath + " already exists. Delete it prior to the move?");
                    if (!MessageBoxResponseIsAffirmitive(result))
                    {
                        return false;
                    }
                }
                try
                {
                    File.Delete(destinationPath);
                }
                catch (Exception exception)
                {
                    if(messageRespondHandler != null)
                    {
                        messageHandler("Failed to delete the file: " + destinationPath + ". " + exception.Message);
                    }
                    return false;
                }
            }
            try
            {
                File.Copy(sourcePath, destinationPath);
            }
            catch(Exception exception)
            {
                if(messageHandler != null)
                {
                    messageHandler("Failed to copy the file: " + sourcePath + " to " + destinationPath + ". " + exception.Message);
                }
                return false;
            }
            return true;
        }
        #endregion

        #region Private Methods
        private static string ReplaceFileNamePattern(string pattern, string year, string month, string day, string pageNumber)
        {
            string replacedDateTimePattern = pattern;
            replacedDateTimePattern = replacedDateTimePattern.Replace(YearPlaceHolder, year);
            replacedDateTimePattern = replacedDateTimePattern.Replace(MonthPlaceHolder, month);
            replacedDateTimePattern = replacedDateTimePattern.Replace(DayPlaceHolder, day);
            replacedDateTimePattern = replacedDateTimePattern.Replace(PagePlaceHolder, pageNumber);
            return replacedDateTimePattern;
        }

        private static bool MessageBoxResponseIsAffirmitive(MessageBoxResult result)
        {
            return result == MessageBoxResult.Yes || result == MessageBoxResult.OK;
        }
        #endregion
    }
}
