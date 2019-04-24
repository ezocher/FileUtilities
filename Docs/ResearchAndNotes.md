# Research and Notes

## DeDupScanner -> Known DB Maker
* Only store full paths since Path.GetFileName() .GetExtension() are cheap
* Known DB Schema:
	* Volume/Media/Machine
	* Full Path
	* Which dates?
	* Length
	* Checksum
	* Cloud (Generalize to where) bit array
		* Need known hierarchy of locations to determine which to store in Full Path
* Rescan? (Need to have for new/changed files, copies in outside of this system)

## Unique file copier
* Unique files copied in existing directory structure into: Found Photos, found Music, found ...
* Check dup directories first??

* Add new files to Known DB


## Skip lists and Extension lists
* gitignore format for skip lists: https://git-scm.com/docs/gitignore
* And a similar format?? for file categories by extension lists?
