using GdPicture14;
using System;
using System.IO;
using System.Net;

class Program
{
    static void Main()
    {
        string ftpServer = "ftp://192.168.0.136";
        string ftpUsername = "Administrator";
        string ftpPassword = "Scm$@123!@#1";
        string localFilePath = @"D:\Nazer_Hussain\a_Reading_Files\Yazaki Reading\Yazaki Supplier Docs\YAZAKI EUROPE LIMITED.PDF";

        // Get the directory path from the local file path
        string directoryPath = Path.GetDirectoryName(localFilePath).Replace("D:\\","");

        // Create the directory structure on the FTP server
        CreateDirectoryStructure(ftpServer, ftpUsername, ftpPassword, directoryPath);

        // Upload the file to the FTP server
        UploadFile(ftpServer, ftpUsername, ftpPassword, localFilePath);

        GetFileFromFtpTORead(localFilePath);
        Console.WriteLine("check txt file");
    }
    static void GetFileFromFtpTORead(string localFilePath)
    {
        string sourceFilePath = localFilePath;
        string ocrDataPath = @"D:\GdPicture.NET 14\Redist\OCR";
        var Products = new List<Dictionary<string, string>>();
        try
        {
            string sourceFileName = Path.GetFileName(sourceFilePath);
            Console.WriteLine($"Source PDF file name: {sourceFileName}");
            // Create an instance of GdPicturePDF
            using (GdPicturePDF gdpicturePDF = new GdPicturePDF())
            {
                // Load the source document
                if (gdpicturePDF.LoadFromFile(sourceFilePath) == GdPictureStatus.OK)
                {
                    // Determine the number of pages
                    int pageCount = gdpicturePDF.GetPageCount();

                    // Loop through the pages of the source document
                    List<string> lines = new List<string>();
                    System.IO.StreamWriter outputFile = new System.IO.StreamWriter(Path.ChangeExtension(localFilePath, ".txt"));
                    for (int i = 1; i <= pageCount; i++)
                    {
                        // Select a page and run the OCR process on it
                        gdpicturePDF.SelectPage(i);
                        gdpicturePDF.OcrPage("eng", ocrDataPath, "", 300);
                        string pageText = gdpicturePDF.GetPageText();
                        outputFile.WriteLine(pageText);
                    }

                    outputFile.Close();

                }
                else
                {
                    Console.WriteLine("Failed to load the source PDF.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing page: {ex.Message}");
        }
    }
    static void CreateDirectoryStructure(string ftpServer, string ftpUsername, string ftpPassword, string directoryPath)
    {
        // Split the directory path into individual directories
        string[] directories = directoryPath.Split(Path.DirectorySeparatorChar);

        // Create directories one by one
        string currentDir = ftpServer;
        foreach (string dir in directories)
        {
            currentDir = currentDir + "/" + dir;

            // Check if directory exists
            if (DirectoryExists(currentDir, ftpUsername, ftpPassword))
            {
                Console.WriteLine("Directory already exists: " + currentDir);
                continue;
            }

            // Create the directory
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(currentDir));
            ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

            try
            {
                using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
                {
                    Console.WriteLine("Created directory: " + currentDir);
                }
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                Console.WriteLine("Error creating directory " + currentDir + ": " + ex.Message);
                response.Close();
            }
        }
    }
    static bool DirectoryExists(string directoryPath, string ftpUsername, string ftpPassword)
    {
        FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(directoryPath));
        ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
        ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

        try
        {
            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
            {
                return true;
            }
        }
        catch (WebException)
        {
            return false;
        }
    }

    static void UploadFile(string ftpServer, string ftpUsername, string ftpPassword, string localFilePath)
    {
        // Get the file name from the local file path
        string fileName = Path.GetFileName(localFilePath);

        // Create FTP request
        FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpServer + "/"+ localFilePath.Replace("D:\\",""));
        ftpRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
        ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

        try
        {
            // Read the file and upload it to the FTP server
            using (FileStream fs = File.OpenRead(localFilePath))
            using (Stream ftpStream = ftpRequest.GetRequestStream())
            {
                fs.CopyTo(ftpStream);
            }

            Console.WriteLine("File uploaded successfully.");
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error uploading file: " + ex.Message);
        }
    }
}
