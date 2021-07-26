# ezocher / FileUtilities

Project / Folder | Description & Status
-----------------|---------------------
CopyUniqueFiles | A unique file copier that compares against base File List.tsv file(s). Can split found files into categories based on file extensions. Creates new File List.tsv files with just unique files found. Can be used without base File Lists to remove duplicate files from a volume/directory. Uses config files from project below for settings and for file/directory exclusion lists. Super hacky WinForms + Console UI - **Working, in active development**
DeDupScanner | A multi-threaded file scanner which computes and stores unique file signatures/fingerprints (SHA1 hash of entire file contents). Produces tab separated text files of data about files and directories scanned. Data files can be used with Excel to analyze or de-dup within a set of volumes or can be used as baselines by CopyUniqueFiles. Uses config files from project below for settings and for file/directory exclusion lists. Super hacky WinForms + Console UI - **Superceded by CopyUniqueFiles**
DeDupScanner/Util | ConfigFileUtil.cs and ConfigSettings.cs - A simple config file parser meeting the needs of the apps in this project - Documented-ish in FileUtilities/Docs/ConfigFiles.md; Sample/test files in FileUtilities/Config - **Working**
ExplorationsSpecialFolders | Project for file system framework tests; Lists special folders on Windows - **Inactive**
PDFValidator | Scans headers of all PDFs in a drive/folder, reports PDF versions of good headers, detects (badly) corrupted PDFs - **Working** _(Someday TBD: Re-do this algorithm in Powershell)_


## TBD

* Build and test Blake3.NET (https://github.com/xoofx/Blake3.NET) and compare with existing SHA1-based implementation of CUF and DDS.