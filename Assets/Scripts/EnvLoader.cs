using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class EnvLoader
{
    private static Dictionary<string, string> envVars = new Dictionary<string, string>();

    public static void LoadEnv()
    {
        string filePath = Path.Combine(Application.dataPath, "../.env");

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                {
                    continue;
                }

                string[] parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    envVars[key] = value;
                }
            }
        }
        else
        {
            Debug.LogError("[EnvLoader-Error] .env not found");
        }
    }

    public static string Get(string key)
    {
        if (envVars.ContainsKey(key))
        {
            return envVars[key];
        }
        return null;
    }
}