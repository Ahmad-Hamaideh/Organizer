using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using PdfFileWriter;
using iTextSharp.text.pdf;
using iTextSharp.text;
using static iTin.Core.Interop.Shared.Linux.Constants.PROC;
using System.Net.NetworkInformation;


namespace Perfectoo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread logThread = new Thread(() => LogActivity("Starting log thread ", ""));
            logThread.Start();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string cDrivePath = "C:\\";
            while (true)
            {
                Console.WriteLine("Choose any action: \n ");
                Console.WriteLine("1. Sort files \n ");
                Console.WriteLine("2. Find duplicates \n ");
                Console.WriteLine("3. Convert images to PDF \n ");
                Console.WriteLine("4. Exit \n");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        ChooseAndProcess("Sort", desktopPath, downloadsPath, cDrivePath, ProcessFiles);
                        break;
                    case "2":
                        ChooseAndProcess("Search for duplicates", desktopPath, downloadsPath, cDrivePath, FindDuplicates);
                        break;
                    case "3":
                        ChooseAndProcess("Convert images to PDF", desktopPath, downloadsPath, cDrivePath, ConvertImagesToPdf);
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid choice!");
                        Console.Beep();
                        break;
                }
            }
        }

        static void ChooseAndProcess(string action, string desktopPath, string downloadsPath, string cDrivePath, Action<string> processAction)
        {
            Console.WriteLine($"Choose the path to {action}: ");
            Console.WriteLine($"1. Desktop Path: {desktopPath}");
            Console.WriteLine($"2. Downloads Path: {downloadsPath}");
            Console.WriteLine($"3. C Drive Path: {cDrivePath}");

            string choice = Console.ReadLine();
            Console.WriteLine();

            string selectedPath = "";

            switch (choice)
            {
                case "1":
                    selectedPath = desktopPath;  
                    break;
                case "2":
                    selectedPath = downloadsPath;
                    break;
                case "3":
                    selectedPath = cDrivePath;
                    break;
                default:
                    Console.WriteLine("Invalid choice!");
                    return;
            }

            processAction(selectedPath);
        }
    
        //-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_
        static void ProcessFiles(string basePath)
        {
            string todayFolder = Path.Combine(basePath, DateTime.Today.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(todayFolder);

            string textFolder = Path.Combine(todayFolder, "Text");
            string pdfFolder = Path.Combine(todayFolder, "PDF");
            string videoFolder = Path.Combine(todayFolder, "Videos");
            string imageFolder = Path.Combine(todayFolder, "Images");
            string excelFolder = Path.Combine(todayFolder, "excelFolder");
            Directory.CreateDirectory(textFolder);
            Directory.CreateDirectory(pdfFolder);
            Directory.CreateDirectory(videoFolder);
            Directory.CreateDirectory(imageFolder);
            Directory.CreateDirectory(excelFolder);

            foreach (string filePath in Directory.GetFiles(basePath))
            {
                string fileName = Path.GetFileName(filePath);
                string extension = Path.GetExtension(fileName).ToLower();

                if (extension == ".txt" || extension == ".doc" || extension == ".docx")
                {
                    File.Move(filePath, Path.Combine(textFolder, fileName));
                }
                else if (extension == ".pdf")
                {
                    File.Move(filePath, Path.Combine(pdfFolder, fileName));
                }
                else if (extension == ".mp4" || extension == ".avi" || extension == ".mkv")
                {
                    File.Move(filePath, Path.Combine(videoFolder, fileName));
                }
                else if (extension == ".jpg" || extension == ".png" || extension == ".gif")
                {
                    File.Move(filePath, Path.Combine(imageFolder, fileName));
                }
                else if (extension == ".xls" || extension == ".xlsx" || extension == ".csv")
                {
                    File.Move(filePath, Path.Combine(excelFolder, fileName));
                }
                else
                {
                    LogActivity($"Unrecognized file extension: {fileName} ({extension}) ", basePath);
                }
            }
            Console.Beep();
            Console.WriteLine($"Files organized successfully in {basePath}");
        }
        //-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ Convert Images ToPdf
        static void ConvertImagesToPdf(string basePath)
        {
            try
            {
                string[] imageFiles = Directory.GetFiles(basePath, "*.jpg")
                                               .Concat(Directory.GetFiles(basePath, "*.jpeg"))
                                               .Concat(Directory.GetFiles(basePath, "*.png"))
                                               .Concat(Directory.GetFiles(basePath, "*.gif"))
                                               .ToArray();

                if (imageFiles.Length == 0)
                {
                    Console.WriteLine("No images found in the directory.");
                    return;
                }

                Console.WriteLine("Available images:\n");
                for (int i = 0; i < imageFiles.Length; i++)
                {
                    Console.WriteLine($"[{i + 1}]. {Path.GetFileName(imageFiles[i])}");
                }

                Console.WriteLine("Enter the numbers of images to convert (separated by commas), or type 'change ' to select images from another directory: ");
                string input = Console.ReadLine().Trim();

                if (input.ToLower() == "change")
                {
                    Console.WriteLine("Enter the path of the new directory: ");

                    string newDirectory = Console.ReadLine().Trim();
                    ConvertImagesToPdf(newDirectory);
                    return;
                }

                string[] selectedImageIndexes = input.Split(',');

                List<string> selectedImages = new List<string>();
                foreach (var indexStr in selectedImageIndexes)
                {
                    if (int.TryParse(indexStr.Trim(), out int index) && index >= 1 && index <= imageFiles.Length)
                    {
                        selectedImages.Add(imageFiles[index - 1]);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid image number: {indexStr}");
                    }
                }

                if (selectedImages.Count == 0)
                {
                    Console.WriteLine("No valid images selected.");
                    return;
                }

                string pdfFileName = "converted_images.pdf";
                string pdfPath = Path.Combine(basePath, pdfFileName);

                Document document = new Document();
                using (var stream = new FileStream(pdfPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    PdfWriter.GetInstance(document, stream);
                    document.Open();
                    document.AddTitle(DateTime.Now.ToString());

                    foreach (var imagePath in selectedImages)
                    {
                        using (var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var image = iTextSharp.text.Image.GetInstance(imageStream);

                            float width = document.PageSize.Width * 0.5f; 
                            float height = image.Height * (width / image.Width);

                            image.ScaleToFit(width, height);

                            image.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                            document.Add(image);

                            document.Add(new Paragraph(" "));
                        }
                    }
                    document.Close();
                }

                Console.WriteLine($"Images converted to PDF: {pdfPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting images to PDF: {ex.Message}");
            }
        }


        static bool IsImageFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp";
        }
        //-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_

        static void LogActivity(string message, string basePath)
        {
            string logFilePath = "activity_log.txt";
            string logMessage = $"[{DateTime.Now.ToString()}] {message} {basePath} ";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }        
        //-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_
        static void FindDuplicates(string basePath)
        {
            try
            {
                List<string> duplicateFiles = new List<string>();

                string[] allFiles = Directory.GetFiles(basePath);

                string[] one = { };
                string todayFolder = Path.Combine(basePath, DateTime.Today.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(todayFolder);

                string duplicateFolder = Path.Combine(todayFolder, "duplicateFile");
                Directory.CreateDirectory(duplicateFolder);

                foreach (var f in allFiles)
                {
                    string[] filecont = System.IO.File.ReadAllLines(f);

                    if (filecont.SequenceEqual(one))
                    {
                        string fileName = Path.GetFileName(f);
                        string destinationPath = Path.Combine(duplicateFolder, fileName);

                        if (!File.Exists(destinationPath))
                        {
                            File.Move(f, destinationPath);
                            LogActivity($"Duplicate file found: {f}", basePath);
                        }
                    }

                    one = filecont;
                }

                Console.Beep();

                foreach (var file in duplicateFiles)
                {
                    Console.Beep();

                    LogActivity($"Duplicate file found: {file}  ", basePath);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error while searching for duplicates: {ex.Message}  ", basePath);
            }
        }
    }
}
