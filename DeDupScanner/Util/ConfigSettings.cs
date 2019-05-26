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

    public ConfigSettings(string category, string key)
    {
        Category = category;
        Key = key;
        Value = null;
    }

    public override string ToString()
    {
        string returnString = String.Format("[{0}] '{1}'", this.Category, this.Key);
        if (this.Value != null)
            returnString += String.Format(" = '{0}'", this.Value);
        return returnString;
    }
}

