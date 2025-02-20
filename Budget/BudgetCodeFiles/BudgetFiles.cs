// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================
namespace Budget
{

    /// <summary>
    /// BudgetFiles class is used to manage the files used in the Budget project
    /// It provides methods to verify file paths for reading and writing.
    /// </summary>
    public class BudgetFiles
    {
        private static String DefaultSavePath = @"Budget\";
        private static String DefaultAppData = @"%USERPROFILE%\AppData\Local\";

        // ====================================================================
        // verify that the name of the file exists, or set the default file, and 
        // is it readable?
        // throws System.IO.FileNotFoundException if file does not exist
        // ====================================================================
        /// <summary>
        ///     Verifies that the file exists and is readable. If no file path is provided, a default is set.
        /// </summary>
        /// <param name="FilePath">The path of the file to verify. If null, a default path will be used.</param>
        /// <param name="DefaultFileName">The default file name to use if no path is provided.</param>
        /// <returns>The verified file path if it exists and is readable.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified file does not exist.</exception>
        /// <example>
        /// Example usage:
        /// <code>
        /// String filePath = BudgetFiles.VerifyReadFromFileName(".\BudgetCodeFiles\filename", "filename");
        /// Console.WriteLine("Verified file path: " + filePath);
        /// </code>
        /// </example>
        public static String VerifyReadFromFileName(String FilePath, String DefaultFileName)
        {

            // ---------------------------------------------------------------
            // if file path is not defined, use the default one in AppData
            // ---------------------------------------------------------------
            if (FilePath == null)
            {
                FilePath = Environment.ExpandEnvironmentVariables(DefaultAppData + DefaultSavePath + DefaultFileName);
            }

            // ---------------------------------------------------------------
            // does FilePath exist?
            // ---------------------------------------------------------------
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException("ReadFromFileException: FilePath (" + FilePath + ") does not exist");
            }

            // ----------------------------------------------------------------
            // valid path
            // ----------------------------------------------------------------
            return FilePath;

        }

        // ====================================================================
        // verify that the name of the file exists, or set the default file, and 
        // is it writable
        // ====================================================================
        /// <summary>
        ///     Verifies that the file path is valid and writable. 
        ///     Creates necessary directories if missing.
        /// </summary>
        /// <param name="FilePath">The path of the file to verify.  If null, a default path will be used.</param>
        /// <param name="DefaultFileName">The default file name to use if no path is provided.</param>
        /// <returns>The verified file path if it is writable.</returns>
        /// <exception cref="Exception">Thrown if the directory does not exist or the file is read-only.</exception>
        /// <example>
        /// Example usage:
        /// <code>
        /// String filePath = BudgetFiles.VerifyWriteToFileName(".\Budget\filename", "filename");
        /// Console.WriteLine("Verified writable file path: " + filePath);
        /// </code>
        /// </example>
        public static String VerifyWriteToFileName(String FilePath, String DefaultFileName)
        {
            // ---------------------------------------------------------------
            // if the directory for the path was not specified, then use standard application data
            // directory
            // ---------------------------------------------------------------
            if (FilePath == null)
            {
                // create the default appdata directory if it does not already exist
                String tmp = Environment.ExpandEnvironmentVariables(DefaultAppData);
                if (!Directory.Exists(tmp))
                {
                    Directory.CreateDirectory(tmp);
                }

                // create the default Budget directory in the appdirectory if it does not already exist
                tmp = Environment.ExpandEnvironmentVariables(DefaultAppData + DefaultSavePath);
                if (!Directory.Exists(tmp))
                {
                    Directory.CreateDirectory(tmp);
                }

                FilePath = Environment.ExpandEnvironmentVariables(DefaultAppData + DefaultSavePath + DefaultFileName);
            }

            // ---------------------------------------------------------------
            // does directory where you want to save the file exist?
            // ... this is possible if the user is specifying the file path
            // ---------------------------------------------------------------
            String folder = Path.GetDirectoryName(FilePath);
            String delme = Path.GetFullPath(FilePath);
            if (!Directory.Exists(folder))
            {
                throw new Exception("SaveToFileException: FilePath (" + FilePath + ") does not exist");
            }

            // ---------------------------------------------------------------
            // can we write to it?
            // ---------------------------------------------------------------
            if (File.Exists(FilePath))
            {
                FileAttributes fileAttr = File.GetAttributes(FilePath);
                if ((fileAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    throw new Exception("SaveToFileException:  FilePath(" + FilePath + ") is read only");
                }
            }

            // ---------------------------------------------------------------
            // valid file path
            // ---------------------------------------------------------------
            return FilePath;

        }



    }
}
