using System.Collections;
using Abstractions.Events;

namespace SmartLine.util;

using System.Text.Json;

public static class ConfigReader
{
    /// <summary>
    /// 默认Json设置
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, // 属性名字全局设置为snake_case_lower
        WriteIndented = true // 优化输出
    };

    /// <summary>
    /// 默认配置数据类
    /// </summary>
    private static readonly ConfigData DefaultConfig = new ConfigData(
        ["./Modules"],
        new()
    );

    //配置文件路径
    private static readonly string ConfigPath = "./config";
    private static readonly string ConfigFileName = "config.json";
    private static readonly string configFull = Path.Combine(ConfigPath, ConfigFileName);

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns></returns>
    public static ConfigData LoadConfig()
    {
        //--如果配置文件不存在，则生成一个新配置文件--
        if (!File.Exists(configFull))
        {
            InitializeConfigFile();
            return DefaultConfig;
        }

        // 读取文件
        string json = File.ReadAllText(configFull);
        if (string.IsNullOrEmpty(json))
        {
            InitializeConfigFile();
            return DefaultConfig;
        }

        ConfigData? configData = JsonSerializer.Deserialize<ConfigData>(json, JsonOptions);
        if (configData == null)
        {
            MainEventBus.Instance.Publish(new LogEvent(LogLevel.Error, "配置文件错误"));
            return DefaultConfig;
        }

        return configData;
    }

    private static void InitializeConfigFile()
    {
        Directory.CreateDirectory(ConfigPath);
        try
        {
            string defaultConfigJson = JsonSerializer.Serialize(DefaultConfig, JsonOptions);
            File.WriteAllText(configFull, defaultConfigJson);
        }
        catch (Exception ioe)
        {
            MainEventBus.Instance.PublishByException(ioe);
        }
    }
}

public class ConfigData(List<string> modulePaths, List<ModuleData> moduleData)
{
    public List<string> ModulePaths { get; init; } = modulePaths;
    public ModuleSelector ModuleSelectors { get; init; } = new ModuleSelector(moduleData);
}

public class ModuleSelector(List<ModuleData> moduleSelectors)
{
    private List<string>? _keys;
    private List<string>? _modules;
    private readonly List<ModuleData> _gotModules = new();

    public string? GetNewValue(string key)
    {
        var value = moduleSelectors.Find(m => m.Interface == key &&
                                              !_gotModules.Contains(m));
        if (value == null) return null;
        _gotModules.Add(value);
        return value.ModuleName;
    }

    public List<string> GetKeys()
    {
        _keys = moduleSelectors.Select(s => s.Interface).ToList();
        return _keys;
    }

    public List<string> GetModuleNamesEnumerator()
    {
        _modules = moduleSelectors.Select(s => s.ModuleName).ToList();
        return _modules;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class ModuleData(string @interface, string moduleName)
{
    public string Interface { get; } = @interface;
    public string ModuleName { get; } = moduleName;
}