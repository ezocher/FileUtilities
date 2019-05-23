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
}

