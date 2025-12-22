using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SummitKit.IO;

public static class JsonUtil
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true
    };


    public static string ToAppdata(string name)
    {
        string configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            name
        );
        Directory.CreateDirectory(configDir);

        return configDir;
    }

    public static void WriteFile<T>(string path, T val) where T : class
    {
        if (val == null) throw new ArgumentNullException(nameof(val));

        var jsonString = JsonSerializer.Serialize(val, DefaultOptions);

        try
        {
            File.WriteAllText(path, jsonString);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Failed to write file {path}: {ex.Message}");
        }
    }

    public static T? ReadFile<T>(string path) where T : class
    {
        if (!File.Exists(path)) return null;

        var jsonString = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(jsonString, DefaultOptions);
    }
}