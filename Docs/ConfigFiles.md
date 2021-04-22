# Simple Config Files

## Format

Each line in a config file can specify a Value, or a Key = Value pair. Every setting of either type exists in a named Category.

### Categories

* Categories are set or changed with lines of the form:

```
	[CategoryName]
```

* Each setting after a category declaration will be in that category until the category is changed

* The default category is ""

### Values

* Values are set with lines of the form:

```
	MyValue
```

* Or of the form:

```
	MyValue1, MyValue2, MyValue3, ...
```

* Key = Value pairs are set with lines of the form:

```
	MyKey = MyValue
```

### Comments and Whitespace

* Comments or blank lines are ignored
  * Comments start with # and can appear anywhere on a line, the remainder of the line will be ignored
  * The # can be escaped with \\
* Whitespace at the beginning and end of category names, keys, and values is ignored
* Whitespace within category names, keys, and values is preserved

### Environment Variables

* Environment variables can appear anywhere within a category name, key, or value and are of the form:

```
	%windir%
```

* They will be replaced with their current values, or with "" if the specified environment variable does not exist

### Special Folders

* Special Folders from the .NET Environment.SpecialFolder enum can appear anywhere within a category name, key, or value and are of the form:

```
	$UserProfile$
```

* They will be replaced with their current values, or with "" if the specified special folder does not exist

## ConfigSettings struct

* Each value or key-value pair in a config file is loaded into one of these structs
* They contain a Category, Key, and Value
* Key will be null in the case of a value-only setting

## ConfigFileUtil

### LoadConfigFile(path)

This static method loads and parses the config file with the specified path and returns an array of ConfigSettings structs with all of the valid values and key-value pairs read from the file.

If there is an exception while reading the file a message is printed to the console and an exception is thrown.
