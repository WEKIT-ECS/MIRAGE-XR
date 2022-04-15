using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ConfigEditor
{

    public string configFileDir = "Assets/MirageXR/Resources";
    public string configFileName = "MirageXRConfig";
    public string termsOfUseDefault = "TermsOfUseDefault";
    public string termsOfUseUser = "TermsOfUseUser";
    private string configFileExt = ".txt"; //Resources.load accept only txt or json as textasset

    public string ConfigFilePath()
    {
        return Path.Combine(configFileDir, configFileName + configFileExt);
    }


    public string TermsOfUseDefaultFilePath()
    {
        return Path.Combine(configFileDir, termsOfUseDefault + configFileExt);
    }

    public string TermsOfUseUserFilePath()
    {
        return Path.Combine(configFileDir, termsOfUseUser + configFileExt);
    }

    public void WriteConfigFile(List<string> configArray)
    {
        string path = ConfigFilePath();

        using (StreamWriter outputFile = new StreamWriter(path))
        {
            foreach (string line in configArray)
                outputFile.WriteLine(line);
        }

    }


    public List<string> ReadConfigFile()
    {
        string path = ConfigFilePath();

        string[] lines = File.ReadAllLines(path);
        return lines.ToList();
    }


    public void EditLine(string configKey, string value)
    {
        List<string> newConfigInfo = new List<string>();

        List<string> lines = ReadConfigFile();

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith(configKey))
            {
                newConfigInfo.Add(configKey + ":" + value);
            }
            else
            {
                newConfigInfo.Add(lines[i]);
            }
        }

        WriteConfigFile(newConfigInfo);
    }


    public string GetValue(string text)
    {
        string[] value = text.Split(':');
        if (value.Length == 0)
            return "";

        return value[1];
    }


    public string ColorToString(Color color)
    {
        return '#' + ColorUtility.ToHtmlStringRGBA(color);
    }


    public Color StringToColor(string str)
    {
        if (ColorUtility.TryParseHtmlString(str, out Color newCol))
        {
            return newCol;
        }
        else
        {
            return Color.black;
        }
    }
}
