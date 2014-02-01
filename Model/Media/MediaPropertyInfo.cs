using System;
using System.Data;
using System.Configuration;
using Exo.Exoget.Model.Search;

public class MediaPropertyInfo
{
    private readonly IndexPropertyType type;
    private readonly string value;

    public MediaPropertyInfo(IndexPropertyType type, string value)
	{
        this.type = type;
        this.value = value;
	}

    public IndexPropertyType Type
    {
        get { return type; }
    }

    public string Value
    {
        get { return value; }
    }
}
