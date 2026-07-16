namespace Demo.Models;

/// <summary>
/// Holds the result of a single test module.
/// </summary>
public class TestResult
{
    public string TestName { get; set; } = "";
    public bool Passed { get; set; } = true;
    public Dictionary<string, object> Data { get; set; } = new();
    public List<string> Messages { get; set; } = new();
    public List<string> Screenshots { get; set; } = new();
}
