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

### Scan target name 
The prefixes of the report file names can be set

## 4 -  Scan target volume or directory (source)


## 5 - Copy/Move destination directory (dest)



