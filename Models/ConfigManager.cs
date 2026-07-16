using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Demo.Models;

/// <summary>
/// Loads defaults, deep-merges user overrides, saves back.
/// </summary>
public class ConfigManager
{
    private JObject _data = new();
    private string? _filePath;

    public ConfigNode Root => new(_data);

    public void LoadDefaults(string path)
    {
        _data = JObject.Parse(File.ReadAllText(path));
    }

    public void LoadUser(string path)
    {
        var overlay = JObject.Parse(File.ReadAllText(path));
        DeepMerge(_data, overlay);
        _filePath = path;
    }

    public void Save(string? path = null)
    {
        path ??= _filePath ?? throw new InvalidOperationException("No save path");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, _data.ToString(Formatting.Indented));
        _filePath = path;
    }

    public string Get(string dottedKey, string fallback = "")
    {
        var parts = dottedKey.Split('.');
        JToken? node = _data;
        foreach (var p in parts)
        {
            node = node?[p];
            if (node == null) return fallback;
        }
        return node.Value<string>() ?? fallback;
    }

    public void Set(string dottedKey, string value)
    {
        var parts = dottedKey.Split('.');
        JToken node = _data;
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (node[parts[i]] == null)
                node[parts[i]] = new JObject();
            node = node[parts[i]]!;
        }
        node[parts[^1]] = value;
    }

    private static void DeepMerge(JObject baseObj, JObject overlay)
    {
        foreach (var prop in overlay.Properties())
        {
            if (baseObj[prop.Name] is JObject baseChild && prop.Value is JObject overChild)
                DeepMerge(baseChild, overChild);
            else
                baseObj[prop.Name] = prop.Value;
        }
    }
}
