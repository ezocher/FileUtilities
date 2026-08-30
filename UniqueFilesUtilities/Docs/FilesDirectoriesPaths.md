# UniqueFilesUtilities: Directories, Files, and Paths
### Terminology and Operation

## Five Categories of DF&P

1. App configuration and settings ("AppConfig" directory)
2. Unique file DBs ("Base File DBs" -or- "Photos File DBs" directories)
3. Reports ("New Reports" directory)
4. Scan target volume or directory (source)
5. Copy/Move destination directory (dest)

## App Root Directory

The first three categories above are all stored in subdirectories of the app root directory, which is in the same location on every pc.

This location is a sub-directory of the current user's One Drive root directory named "Files and Storage". E.g. "C:\Users\username\OneDrive\Files and Storage" 

This location is hard-coded within AppSettings.cs as it is needed to open the core AppSettings.txt file so that all other settings can be loaded.

## 1 - App configuration and settings

All of the apps' configuration and settings files (*.txt) are stored in the "AppConfig" subdirectory of the app root directory.

The core configuration and settings are read from "AppSettings.txt" which includes all of the subsequent file names and locations.

In the Visual Studio solution the Config folder contains all of the .txt files and a Powershell script that copies them from the git repo to the AppConfig directory.

## 2 - Unique file DBs

The databases of previously scanned unique files are stored in one of two subdirectories of the app root directory:
* "Base File DBs" for the FingerprintDBMaker and UniqueFileCopier apps
* "Photos File DBs" for the PhotoCollector app

The databases are stored in .tsv files and the unique entries are all loaded into an in-memory database for detecting unique files in the scan target.

## 3 - Reports and new DB's

As the scan runs, three new reports are generated into .tsv files: excluded/skipped files, duplicate files, and unique files.

These reports are all created in the "New Reports" subdirectory of the app root directory. If there are existing reports with the same name a (#) will be added to subsequent report names, no reports are ever overwritten.

The database of newly found unique files is also generated as a .tsv file and stored in "New Reports". Currently the "*-File DB.tsv" files must be manually copied to one of the Base File DBs folders to be used in the baseline for future scans.

### Base Name
When the scan target volume or directory is selected, this determines the default "base name" for the target of the scan. Base name is name of directory e.g. "Music" or machine + drive name e.g. "MyLaptop-Drive C". This base name can be changed to anything desired before the scan is started.

The base name is used as the prefix of the names of the reports and new DB that are generated during the scan.

In the Unique File Copier app, the base name is also used as the suffix name of the destination directory for the copy operation (see 5 below).

## 4 -  Scan target volume or directory (source)

The source root directory (string sourceRootDir)is the root of the volume or directory that is selected for scanning. 

## 5 - Copy/Move destination directory (dest)

If the selected destination volume is the system volume (typically C:), then we can't write to the root of the volume and instead create the destination root directory in the current user's root directory (E.g. C:\Users\username).

### Unique File Copier Destination Directory
For the Unique File Copier app, the destination root directory is named based on the base name described above, prefixed by "unq-".

If the base name is "Thumb drive 1" for example, the destination root directory would be "unq-Thumb drive 1", with the path being "D:\unq-Thumb drive 1" or "C:\Users\username\unq-Thumb drive 1" in this example.

### Unique File Copier Copied Files Subdirectories
Unique files found in the scan target are copied to this destination root directory.

If the option to copy files into categories is selected then the files are copied into subdirectories of the destination root directory based on their extension's category as defined in the FileCategoriesByExtension.txt file. Within each category directory, the the directory structure of the source files is preserved. This subdirectory structure is created in the category directories as needed.

If the files are being copied without categorization, then they are copied into the destination root directory, preserving the directory structure of the source files. The subdirectory structure is created in the destination root directory as needed.

### Photo Collector Destination Directory
For the Photo Collector app, there are two destination root directories, one for Photos and one for Videos.

For example, the destination root directory for Photos will be "D:\Collected Photos" or "C:\Users\username\Collected Photos" on the system volume.

### Photo Collector Copied Files Subdirectories
Unique photos and videos found in the scan target are checked for the year they were taken using metadata in the files or for year names found in the file's path.

The year taken (or "Unknown Year" if it can't be determined) is used to create a subdirectory of the destination root directory for each year. The unique photos and videos are copied into these year directories, preserving the directory structure of the source files. The subdirectory structure is created in the year directories as needed.