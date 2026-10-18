# FOLDER SYNCHRONIZER

A folder synchronization tool written in C# using .NET 8

The application synchronizes two folders in one direction, from source to replica, at a specified time interval.

## FEATURES

- Recursive synchronization of files and subfolders.
- Detection of modified files using SHA-256.
- Automatic creation, updating and deletion of files and folders.
- Logging to both the console and a log file.
- Configurable paths and synchronization interval through command line arguments.

## USAGE

Requires .NET 8 SDK.

Run the application from the project directory:

dotnet run -- <source> <replica> <interval> <logFile>

For example:

dotnet run -- source replica 5 sync.log

This synchronizes the 'source' folder with 'replica' every 5 seconds. The replica directory is created if it doesn't exist.

Press 'Ctrl+C' to stop the program.

## NOTES

The source folder is left unchanged. Files and folders that no longer exist in the source are removed from the replica.

The application uses standard .NET libraries without external synchronization tools. It includes basic path validation and error handling, but does not explicitly handle symbolic links or files modified during synchronization.