using System;


struct ConfigSettings
{
    public string Category { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }

    public ConfigSettings(string category, string key, string value)
    {
        Category = category;
        Key = key;
        Value = value;
    }

    public ConfigSettings(string category, string value)
    {
        Category = category;
        Key = null;
        Value = value;
    }

    public override string ToString()
    {
        string returnString = String.Format("[{0}] ", this.Category);
        if (this.Key != null)
            returnString += String.Format("'{0}' = ", this.Key);
        returnString += String.Format("'{0}'", Value);
        return returnString;
    }
}

