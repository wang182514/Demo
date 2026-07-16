using Newtonsoft.Json.Linq;

namespace Demo.Models;

/// <summary>
/// Dynamically-typed config node — supports dotted access and JSON serialization.
/// Thin wrapper around JToken.
/// </summary>
public class ConfigNode
{
    private readonly JToken _token;

    public ConfigNode(JToken token) => _token = token;

    public ConfigNode this[string key]
    {
        get
        {
            var child = _token[key];
            if (child == null) throw new KeyNotFoundException($"No such config key: {key}");
            return new ConfigNode(child);
        }
        set
        {
            if (value._token != null)
                _token[key] = value._token;
        }
    }

    // implicit conversions for readability
    public static implicit operator string(ConfigNode n) => n._token.Value<string>() ?? "";
    public static implicit operator int(ConfigNode n) => n._token.Value<int>();
    public static implicit operator double(ConfigNode n) => n._token.Value<double>();
    public static implicit operator bool(ConfigNode n) => n._token.Value<bool>();
    public static implicit operator float(ConfigNode n) => (float)n._token.Value<double>();

    public string Get(string key, string fallback = "")
    {
        var child = _token[key];
        return child?.Value<string>() ?? fallback;
    }

    public void Set(string key, string value) => _token[key] = value;

    public T Value<T>() => _token.Value<T>()!;

    public double[] ToDoubleArray()
    {
        return ((JArray)_token).Select(t => t.Value<double>()).ToArray();
    }

    public int[] ToIntArray()
    {
        return ((JArray)_token).Select(t => t.Value<int>()).ToArray();
    }

    public string[] ToStringArray()
    {
        return ((JArray)_token).Select(t => t.Value<string>()!).ToArray();
    }
}
