using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SummitKit.IO;

public interface ISerializable<T> where T : class, ISerializable<T>
{
    [JsonIgnore]
    public JsonSerializerOptions JsonOptions => new JsonSerializerOptions
    {
        WriteIndented = true
    };
    [JsonIgnore]
    public string Namespace { get; }
    [JsonIgnore]
    public string FileName { get; }
    [JsonIgnore]
    public string FilePath => (Namespace is null ? FileName : Path.Combine(JsonUtil.ToAppdata(Namespace), FileName)) + ".json";

    void Load()
    {
        if (!File.Exists(FilePath))
            return;

        string json = File.ReadAllText(FilePath);
        var loaded = JsonSerializer.Deserialize<T>(json, JsonOptions);
        if (loaded == null) return;

        // Copy all public writable properties from loaded instance
        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanWrite) continue;
            var value = prop.GetValue(loaded);
            prop.SetValue(this, value);
        }

        OnLoad();
    }
    void Save()
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        string json = JsonSerializer.Serialize(this, GetType(), JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    void OnLoad();
}