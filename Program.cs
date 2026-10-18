using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

if (args.Length != 4)
{
    Console.WriteLine("Usage: dotnet run -- <source> <replica> <interval> <logFile>");
    return;
}


string source = Path.GetFullPath(args[0]);
string replica = Path.GetFullPath(args[1]);
string logFile = Path.GetFullPath(args[3]);

if (!Directory.Exists(source))
{
    Console.WriteLine("Error: Source directory does not exist.");
    return;
}

if (IsSameOrNested(source, replica) ||
    IsSameOrNested(replica, source))
{
    Console.WriteLine("Error: Source and replica must be separate directories.");
    return;
}

if (IsSameOrNested(source, logFile) ||
    IsSameOrNested(replica, logFile))
{
    Console.WriteLine("Error: Log file must be outside source and replica.");
    return;
}

if (!int.TryParse(args[2], out int interval) || interval <= 0)
{
    Console.WriteLine("Error: Interval must be a positive integer that corresponds to the time in seconds between each synchronization.");
    return;
}

string? logDirectory = Path.GetDirectoryName(logFile);

if (logDirectory != null)
{
    Directory.CreateDirectory(logDirectory);
}

while (true)
{
    try
    {
        Log("Starting synchronization...", logFile);

        Synchronize(source, replica, logFile);

        Log("Synchronization completed.", logFile);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Synchronization failed: {ex.Message}");
        break;
    }

    Console.WriteLine($"Waiting {interval} seconds...");

    Thread.Sleep(TimeSpan.FromSeconds(interval));
}


static void Synchronize(string source, string replica, string logFile)
{
    if (!Directory.Exists(replica))
    {
        Directory.CreateDirectory(replica);
        Log($"Directory created: {replica}", logFile);
    }

    string[] files = Directory.GetFiles(
        source,
        "*",
        SearchOption.AllDirectories
    );


    string[] sourceDirectories = Directory.GetDirectories(
        source,
        "*",
        SearchOption.AllDirectories
    );

    Array.Sort(sourceDirectories, (a, b) =>
    a.Length.CompareTo(b.Length)
    );

    foreach (string directory in sourceDirectories)
    {
        string relativePath = Path.GetRelativePath(source, directory);

        string destinationDirectory = Path.Combine(replica, relativePath);

        if (File.Exists(destinationDirectory))
        {
            File.Delete(destinationDirectory);
            Log($"File deleted: {destinationDirectory}. It should be a directory.", logFile);
        }

        if (!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
            Log($"Directory created: {destinationDirectory}", logFile);
        }
    }

    foreach (string file in files)
    {
        string relativePath = Path.GetRelativePath(source, file);

        string destination = Path.Combine(replica, relativePath);

        if (Directory.Exists(destination))
        {
            Directory.Delete(destination, true);
            Log($"Directory deleted: {destination}. It should be a file.", logFile);
        }

        string? destinationFolder = Path.GetDirectoryName(destination);

        if (destinationFolder != null)
        {
            Directory.CreateDirectory(destinationFolder);
        }

        if (!File.Exists(destination))
        {
            File.Copy(file, destination);
            Log($"Created: {destination}", logFile);
        }
        else if (!FilesAreEqual(file, destination))
        {
            File.Copy(file, destination, true);
            Log($"Updated: {destination}", logFile);
        }
        else
        {
            Console.WriteLine($"Unchanged: {destination}");
        }

    }


    string[] replicaFiles = Directory.GetFiles(
        replica,
        "*",
        SearchOption.AllDirectories
    );

    foreach (string replicaFile in replicaFiles)
    {
        string relativePath = Path.GetRelativePath(replica, replicaFile);

        string sourceFile = Path.Combine(source, relativePath);

        if (!File.Exists(sourceFile))
        {
            File.Delete(replicaFile);
            Log($"Deleted: {replicaFile}", logFile);
        }
    }


    string[] replicaDirectories = Directory.GetDirectories(
        replica,
        "*",
        SearchOption.AllDirectories
    );

    Array.Sort(replicaDirectories, (a, b) =>
        b.Length.CompareTo(a.Length)
    );

    foreach (string directory in replicaDirectories)
    {
        string relativePath = Path.GetRelativePath(replica, directory);

        string sourceDirectory = Path.Combine(source, relativePath);

        if (!Directory.Exists(sourceDirectory))
        {
            Directory.Delete(directory, true);
            Log($"Directory deleted: {directory}", logFile);
        }
    }
}

static bool FilesAreEqual(string path1, string path2)
{
    byte[] hash1;
    byte[] hash2;

    using (FileStream stream1 = File.OpenRead(path1))
    {
        hash1 = SHA256.HashData(stream1);
    }

    using (FileStream stream2 = File.OpenRead(path2))
    {
        hash2 = SHA256.HashData(stream2);
    }

    return hash1.SequenceEqual(hash2);
}

static void Log(string message, string logFile)
{
    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    string logMessage = $"[{timestamp}] {message}";

    Console.WriteLine(logMessage);

    File.AppendAllText(logFile, logMessage + Environment.NewLine);
}

static bool IsSameOrNested(string parent, string path)
{
    string relativePath = Path.GetRelativePath(parent, path);

    return relativePath == "." ||
           (!Path.IsPathRooted(relativePath) &&
            relativePath != ".." &&
            !relativePath.StartsWith(".." + Path.DirectorySeparatorChar));
}




