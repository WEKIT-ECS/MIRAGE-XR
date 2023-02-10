using System;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

/// <summary>
/// Utilities for handling ZIP files
/// </summary>
public static class ZipUtilities
{
    /// <summary>
    /// Extracts the ZIP file from the stream and puts its contents into the outFolder
    /// </summary>
    /// <param name="stream">The stream that reads the ZIP file</param>
    /// <param name="outFolder">The folder to which the content is extracted</param>
    /// <returns></returns>
    public static async Task ExtractZipFileAsync(Stream stream, string outFolder)
    {
        using (var zipFile = new ZipFile(stream))
        {
            foreach (ZipEntry zipEntry in zipFile)
            {
                if (!zipEntry.IsFile) continue;

                var entryFileName = CheckFileForIllegalCharacters(zipEntry.Name);
                var fullZipToPath = Path.Combine(outFolder, entryFileName);
                var directoryName = Path.GetDirectoryName(fullZipToPath);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                }
                using (var zipStream = zipFile.GetInputStream(zipEntry))
                using (Stream fileStream = File.Create(fullZipToPath))
                {
                    await zipStream.CopyToAsync(fileStream);
                }
            }
        }
    }

    /// <summary>
    /// Compresses the given folder into a ZIP file
    /// </summary>
    /// <param name="path">The path to the folder which should be compressed</param>
    /// <param name="zipStream">The stream which writes the resulting ZIP file</param>
    /// <param name="compressionLevel">The amount of compression. Should be between 0 and 9.</param>
    /// <returns></returns>
    public static async Task CompressFolderAsync(string path, ZipOutputStream zipStream, int compressionLevel = 5)
    {
        const string slash = "\\";
        if (compressionLevel > 9) compressionLevel = 9;
        if (compressionLevel < 0) compressionLevel = 0;
        zipStream.SetLevel(compressionLevel);

        var folderName = CheckFileForIllegalCharacters(Path.GetDirectoryName(path));

        if (Directory.Exists(path) && folderName != null)
        {
            var folderOffset = folderName.Length + (folderName.EndsWith(slash) ? 0 : 1);
            await CompressFolderRecursivelyAsync(path, zipStream, folderOffset);
        }
        else
        {
            throw new ArgumentException();
        }
    }

    // recursively compresses a folder structure
    private static async Task CompressFolderRecursivelyAsync(string path, ZipOutputStream zipStream, int folderOffset)
    {
        var files = Directory.GetFiles(path);
        foreach (var filename in files)
        {
            var fi = new FileInfo(filename);
            var entryName = filename.Substring(folderOffset);
            entryName = ZipEntry.CleanName(entryName);

            var newEntry = new ZipEntry(entryName)
            {
                DateTime = fi.LastWriteTime,
                Size = fi.Length
            };

            zipStream.PutNextEntry(newEntry);
            using (var fileStream = File.OpenRead(filename))
            {
                await fileStream.CopyToAsync(zipStream);
            }
            zipStream.CloseEntry();
        }

        var folders = Directory.GetDirectories(path);
        foreach (var folder in folders)
        {
            await CompressFolderRecursivelyAsync(folder, zipStream, folderOffset);
        }
    }

    /// <summary>
    /// Adds a given file to a compressed zip output stream
    /// </summary>
    /// <param name="zipStream">The stream that writes the resulting zip folder</param>
    /// <param name="sourceFileName">The path to the file that should be included in the zip file</param>
    /// <param name="entryName">The name under which the file should be added to the zip file</param>
    /// <returns></returns>
    public static async Task AddFileToZipStreamAsync(ZipOutputStream zipStream, string sourceFileName, string entryName)
    {
        var fileInfo = new FileInfo(sourceFileName);
        var entry = new ZipEntry(entryName)
        {
            DateTime = fileInfo.LastWriteTime,
            Size = fileInfo.Length
        };
        zipStream.PutNextEntry(entry);
        using (var fileStream = File.OpenRead(sourceFileName))
        {
            await fileStream.CopyToAsync(zipStream);
        }
        zipStream.CloseEntry();
    }

    /// <returns>The inputed file name without illegal characters</returns>
    /// <summary>
    /// Checks a given string for the illegal file name characters \ : * ? " < > |
    /// </summary>
    /// <param name="file">The file name to be checked</param>
    public static string CheckFileForIllegalCharacters(string file)
    {
        string characters = "\\:*?\"<>|";

        foreach (char c in characters.ToCharArray())
        {
            if (file.Contains(c))
            {
                file = file.Replace(c.ToString(), string.Empty);
            }
        }

        return file;
    }
}