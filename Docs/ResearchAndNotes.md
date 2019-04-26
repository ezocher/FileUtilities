# Research and Notes

## Known Files DB Maker
* Keep DeDupScanner for intra-volume cleanup and cross-base-volumes cleanup
* Clone into new project KnownFilesDBMaker
* KnowFilesDBMaker scans (or re-scans) volumes and creates Known Files DB files for use by Unique File Collector
	* Can run a Rescan on any volume when needed for new/changed files, files copied in outside of this system
	* Rescan replaces previous DB
* Known DB Schema:
	* Volume/Media/Machine (first one in scan priority order)
	* Full Path
      * Only store full paths since Path.GetFileName() .GetExtension() etc. are cheap
	* Which dates? None for now
	* Length
	* Checksum (Dictionary key)
	* Cloud (Generalize to where) bit array
      * OneDrive and Dropbox for now
	* Number of copies in this volume
      * During scan, duplicates found just increment this counter
* "In memory DB": Dictionary<string, file-record>
  * Key is checksum in a string
* Fully scan to in memory DB and then serialize
* Known DB serialized format:
	* Tab delimited text
    * Excel maximum rows per worksheet: 1,048,576 rows

## Skip lists and Extension lists
* Need to be much better, e.g. exclude User/AppData etc.
* gitignore format for skip lists: https://git-scm.com/docs/gitignore
* And a similar format?? for file categories by extension lists?

## Unique File Collector
* Load previously made DB Files that define Base File Collection in "priority order" into in-memory DB
* Scan new volume and compare to DB
  * Unique files copied in existing directory structure into: Found Photos, found Music/Podcasts/Sounds, found Office/Documents, found Code, found PDFs, found Web, found Adobe
  * Destination volume for found files can be specified (or in config file)
* Add found files to in-memory DB
* Create and serialize found files DB in Known DB format
* These can be added to Base File Collection for running more scans without having to rescan anything
* On another computer (e.g. a laptop) Unique File Collector can be run with C: as the new volume being scanned
	* Found files can be copied to a USB stick/drive to later move to the machine housing the Base File Collection


