using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Perfectoo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black;

            Thread logThread = new Thread(() => LogActivity("Starting log thread", ""));
            logThread.Start();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
            string cDrivePath = "C:\\";

            Console.WriteLine("Choose any action: ");
            Console.WriteLine("1. Sort files");
            Console.WriteLine("2. Find duplicates");
            Console.WriteLine("3. Exit");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.WriteLine("Choose the path to sort: ");
                    Console.WriteLine("1. Desktop Path: " + desktopPath);
                    Console.WriteLine("2. Downloads Path: " + downloadsPath);
                    Console.WriteLine("3. C Drive Path: " + cDrivePath);
                    string sortChoice = Console.ReadLine();
                    string sortPath = "";
                    switch (sortChoice)
                    {
                        case "1":
                            sortPath = desktopPath;
                            break;
                        case "2":
                            sortPath = downloadsPath;
                            break;
                        case "3":
                            sortPath = cDrivePath;
                            break;
                        default:
                            Console.WriteLine("Invalid choice!");
                            return;
                    }
                    ProcessFiles(sortPath);
                    break;
                case "2":
                    Console.WriteLine("Choose the path to search for duplicates: ");
                    Console.WriteLine("1. Desktop Path: " + desktopPath);
                    Console.WriteLine("2. Downloads Path: " + downloadsPath);
                    Console.WriteLine("3. C Drive Path: " + cDrivePath);
                    string duplicateChoice = Console.ReadLine();
                    string duplicatePath = "";
                    switch (duplicateChoice)
                    {
                        case "1":
                            duplicatePath = desktopPath;
                            break;
                        case "2":
                            duplicatePath = downloadsPath;
                            break;
                        case "3":
                            duplicatePath = cDrivePath;
                            break;
                        default:
                            Console.WriteLine("Invalid choice!");
                            return;
                    }
                    Thread duplicateThread = new Thread(() => FindDuplicates(duplicatePath));
                    duplicateThread.Start();
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine("Invalid choice!");
                    break;
            }
        }

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
                    LogActivity($"Unrecognized file extension: {fileName} ({extension})", basePath);
                }
            }
            Console.Beep();
            Console.WriteLine($"Files organized successfully in {basePath}");
        }

        static void LogActivity(string message, string basePath)
        {
            string logFilePath = "activity_log.txt";
            string logMessage = $"[{DateTime.Now.ToString()}] {message} {basePath} ";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }

        static void FindDuplicates(string basePath)
        {
            try
            {
                List<string> duplicateFiles = new List<string>();

                string[] allFiles = Directory.GetFiles(basePath);

                var duplicateFileNames = allFiles.GroupBy(Path.GetFileName)
                                                 .Where(g => g.Count() > 1)
                                                 .Select(g => g.Key);

                foreach (var fileName in duplicateFileNames)
                {
                    foreach (var filePath in allFiles)
                    {
                        if (Path.GetFileName(filePath) == fileName || Path.GetFileNameWithoutExtension(filePath) == fileName + "(1)")
                        {
                            duplicateFiles.Add(filePath);
                            break;
                        }
                    }
                }

                foreach (var file in duplicateFiles)
                {
                    Console.Beep();

                    LogActivity($"Duplicate file found: {file}", basePath);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error while searching for duplicates: {ex.Message}", basePath);
            }
        }

    }
}
