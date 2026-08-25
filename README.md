# ezocher / FileUtilities

Project / Folder | Description & Status
-----------------|---------------------
Config | Configuration files (.txt) for file/directory exclusion lists and file extension exclusion list and extension map to file categories. Format documented-ish in Docs/ConfigFiles.md
CopyUniqueFiles | [TBD: Rename] Main project -- .sln can build three different apps: * File Fingerprint Database Maker, * Unique File Copier, and * Photo Collector and Organizer. Apps share config files from project below for settings and for file/directory exclusion lists. Super hacky WinForms + Console UI - **Working, in active development**
CopyUniqueFiles/Util | ConfigFileUtil.cs and ConfigSettings.cs - A simple config file parser meeting the needs of the apps in this project - Documented-ish in Docs/ConfigFiles.md; Sample/test files in Config - **Working**
DeDupScanner | **Superceded by CopyUniqueFiles**
ExplorationsSpecialFolders | Project for file system framework tests; Lists special folders on Windows - **Inactive**
PDFValidator | Scans headers of all PDFs in a drive/folder, reports PDF versions of good headers, detects (badly) corrupted PDFs - **Working** _(Someday TBD: Re-do this algorithm in Powershell)_

## CopyUniqueFiles Notes

* A multi-threaded file scanner which computes and stores unique file signatures/fingerprints (SHA1 hash of entire file contents). 
* Produces tab separated text files of data about files and directories scanned. Data files can be used with Excel to analyze or de-dup within a set of volumes or can be used as baselines by CopyUniqueFiles. 
* Uses config files from Config directory for file/directory exclusion lists.

## Next

* Run single threaded photo scanner to test new code and review directory naming possibilities and file collecting strategy and identify any missing file extensions
* Implement real multi-threaded scanner/collector with a "Would've" report but don't move any files
* Implement saving newly created DBs in the correct places for all three apps

## Use Photo Collector

* With "finished" collector: start by moving intact special project folders and "best of" folders such as Photo Books and Calendars and then build starting DB (exclude "best of" collections from DB so that they get copied as part of their original full folders)

## TBD Someday

* Build and test Blake3.NET (https://github.com/xoofx/Blake3.NET) and compare with existing SHA1-based implementation of CUF and DDS.