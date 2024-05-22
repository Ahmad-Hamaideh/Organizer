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
using System.Media;
using System.Reflection;


namespace Perfectoo
{
    using System;
    using System;
    using System.IO;
    using System.Linq;



    internal class Program
    {
        static void Main(string[] args)
        {
            Thread logThread = new Thread(() => LogActivity("Starting log thread ", ""));
            logThread.Start();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string cDrivePath = "C:\\";

            Dictionary<string, string> paths = new Dictionary<string, string>
            {
                 { "1", desktopPath },
                 { "2", downloadsPath },
                 { "3", cDrivePath }
            };

            DriveInfo[] drives = DriveInfo.GetDrives();
            for (int i = 0; i < drives.Length; i++)
            {
                paths.Add((i + 4).ToString(), drives[i].RootDirectory.FullName);
            }


            while (true)
            {
                Console.WriteLine("Choose any action: \n ");
                Console.WriteLine("1. Sort files \n ");
                Console.WriteLine("2. Find duplicates \n ");
                Console.WriteLine("3. Convert images to PDF \n ");
                Console.WriteLine("4. Exit \n");
                Console.WriteLine("---------------------------------------------------------------------------------------------------------------------\n");

                string choice = Console.ReadLine();
                Console.WriteLine("\n");

                switch (choice)
                {
                    case "1":
                        ChooseAndProcess("Sort", paths, ProcessFiles);
                        break;
                    case "2":
                        ChooseAndProcess("Search for duplicates", paths, FindDuplicates);
                        break;
                    case "3":
                        ChooseAndProcess("Convert images to PDF", paths, ConvertImagesToPdf);
                        break;
                    case "4":
                        System.Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice!");
                        Console.Beep();
                        break;
                }
            }
        }




        static void ChooseAndProcess(string action, Dictionary<string, string> paths, Action<string, bool> processAction)
        {
            Console.WriteLine($"Choose the path to {action}: \n");

            int index = 1;
            foreach (var kvp in paths)
            {
                Console.WriteLine($"{index}. {kvp.Key}: {kvp.Value}\n");
                index++;
            }
            Console.WriteLine("---------------------------------------------------------------------------------------------------------------------\n");

            string choice = Console.ReadLine();
            Console.WriteLine("\n");

            string selectedPath = "";

            if (paths.ContainsKey(choice))
            {
                selectedPath = paths[choice];
            }
            else
            {
                Console.WriteLine("Invalid choice!");
                return;
            }

            Console.WriteLine("Choose how to process:\n");
            Console.WriteLine(" 1 - Process files within a specific folder\n");
            Console.WriteLine(" 2 - Process all files within the selected path\n");
            Console.WriteLine("---------------------------------------------------------------------------------------------------------------------\n");

            string processChoice = Console.ReadLine();
            Console.WriteLine("\n");

            bool processAll = false;
            switch (processChoice)
            {
                case "1":
                    Console.WriteLine("Enter the folder name to process:");
                    ListFolders(selectedPath, processAction);
                    break;
                case "2":
                    processAll = true;
                    processAction(selectedPath, processAll);
                    break;
                default:
                    Console.WriteLine("Invalid choice!");
                    return;
            }
        }

        static void ListFolders(string path, Action<string, bool> processAction)
        {
            try
            {
                Console.WriteLine($"Folders in {path}:");
                string[] folders = Directory.GetDirectories(path);
                if (folders.Length == 0)
                {
                    Console.WriteLine("No folders found.");
                    Console.WriteLine("Press any key to go back.");
                    Console.ReadKey();
                    Console.WriteLine("Going back...\n");
                    return;
                }

                for (int i = 0; i < folders.Length; i++)
                {
                    Console.WriteLine($"[{i + 1}]. {Path.GetFileName(folders[i])}");
                }

                Console.WriteLine("Enter the folder number to process (0 to go back):");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Invalid input!");
                    return;
                }

                if (int.TryParse(input, out int folderIndex) && folderIndex >= 1 && folderIndex <= folders.Length)
                {
                    string selectedFolder = folders[folderIndex - 1];
                    Console.WriteLine($"Selected folder: {selectedFolder}\n");
                    ListFolders(selectedFolder, processAction);
                }
                else if (folderIndex == 0)
                {
                    Console.WriteLine("Going back...\n");
                    return;
                }
                else
                {
                    Console.WriteLine("Invalid folder selection!");
                    return;
                }
                processAction(path, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        //-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_
        static void ProcessFiles(string basePath, bool processAll)
        {
            string todayFolder = Path.Combine(basePath, DateTime.Today.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(todayFolder);

            string textFolder = Path.Combine(todayFolder, "Text");
            string pdfFolder = Path.Combine(todayFolder, "PDF");
            string videoFolder = Path.Combine(todayFolder, "Videos");
            string imageFolder = Path.Combine(todayFolder, "Images");
            string excelFolder = Path.Combine(todayFolder, "Excel");
            Directory.CreateDirectory(textFolder);
            Directory.CreateDirectory(pdfFolder);
            Directory.CreateDirectory(videoFolder);
            Directory.CreateDirectory(imageFolder);
            Directory.CreateDirectory(excelFolder);

            Dictionary<string, string> extensionToFolderMap = new Dictionary<string, string>
            {
                { ".txt", textFolder },
                { ".doc", textFolder },
                { ".docx", textFolder },
                { ".pdf", pdfFolder },
                { ".mp4", videoFolder },
                { ".avi", videoFolder },
                { ".mkv", videoFolder },
                { ".jpg", imageFolder },
                { ".png", imageFolder },
                { ".gif", imageFolder },
                { ".xls", excelFolder },
                { ".xlsx", excelFolder },
                { ".csv", excelFolder }
            };

            foreach (string filePath in Directory.GetFiles(basePath))
            {
                string fileName = Path.GetFileName(filePath);
                string extension = Path.GetExtension(fileName).ToLower();

                if (extensionToFolderMap.TryGetValue(extension, out string destinationFolder))
                {
                    string destinationPath = Path.Combine(destinationFolder, fileName);

                    if (File.Exists(destinationPath))
                    {
                        string newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.Ticks}{extension}";
                        destinationPath = Path.Combine(destinationFolder, newFileName);
                    }

                    File.Move(filePath, destinationPath);
                }
                else
                {
                    LogActivity($"Unrecognized file extension: {fileName} ({extension})", basePath);
                }
            }

            Console.Beep();
            Console.WriteLine($"Files organized successfully in {basePath}\n");
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------- \n ");
        }
        //-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_ Convert Images ToPdf
        static void ConvertImagesToPdf(string basePath, bool processAll)
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
                    ConvertImagesToPdf(newDirectory, true);
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


        static bool IsImageFile(string filePath, bool processAll)
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
        static void FindDuplicates(string basePath, bool processAll)
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
