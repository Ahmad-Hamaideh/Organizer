
## 🛠 Organizing files

 The program automatically creates new folders with today’s date in the path chosen by the user (from the office path, the downloads folder, or the C drive). Files in the specified path are moved to new folders based on the file extension.

* Technology Used:

Programming Language: C# was used for the project.
Programming Libraries and Tools: .NET Framework libraries were utilized for developing the application.
Additional Libraries: .NET Framework libraries were used for file and folder handling, as well as for operations related to date and time.
Threads:

* Threads were employed in this project to execute concurrent operations such as logging activities and searching for duplicate files without blocking the user interface.
The main thread was utilized for running the user interface and initiating the file organization process.
A separate thread was employed for searching for duplicate files, enabling this operation to be executed independently and efficiently.
By leveraging these technologies and effectively managing threads, the application can achieve its specified goals with efficiency an


