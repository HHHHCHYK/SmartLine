using System.Text.Json;
using Abstractions;
using Abstractions.Events;

namespace LogWriter;

public class LogWriterModule : IModule, INotifier
{
    public string ModuleName => nameof(LogWriterModule);

    // 模块状态控制
    private bool IsStarted { get; set; } = false;

    // 日志文件写入相关
    private FileStream? FileStream { get; set; } = null;
    private string LogFileName => "Logs--" + DateTime.Now.Day + ".txt";

    // 配置文件读取
    private const string LogConfigPath = "./config";
    private const string LogConfigFileName = "LogConfig.json";
    private string LogConfigFullPath => Path.Combine(LogConfigPath, LogConfigFileName);

    public LogWriterModule(IEventBus eventBus)
    {
        EventBus = eventBus;
        AppDomain.CurrentDomain.UnhandledException += OnAppCrashed;
        Initialize();
        EventBus.Publish(LogEvent.Info(ModuleName + "模块加载成功"));
    }

    private void OnAppCrashed(object sender, UnhandledExceptionEventArgs e)
    {
        if (FileStream != null)
        {
            FileStream.Dispose();
        }
    }

    public void Initialize()
    {
        EventBus.Subscribe<LogEvent>(OnLog);
    }

    private void OnLog(LogEvent logEvent)
    {
        if (!IsStarted) return;
    }

    public void Start()
    {
        IsStarted = true;
        var config = GetConfig(); // 读取配置文件
        FileStream?.DisposeAsync();
        string path = Path.Combine(config.LogPath, LogFileName);
        // 还未存在文件
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(config.LogPath);
            FileStream = File.Create(path);
        }
        // 已经存在日志文件
        else
        {
            FileStream = File.Open(path, FileMode.Append);
        }
    }

    public void Stop()
    {
        IsStarted = false;
        FileStream?.Dispose();
    }

    private static LogConfigData DefaultConfig => new LogConfigData("./logs");

    private LogConfigData GetConfig()
    {
        LogConfigData config = DefaultConfig;

        // 读取配置文件
        if (File.Exists(LogConfigFullPath))
        {
            try
            {
                string json = File.ReadAllText(LogConfigFullPath);
                LogConfigData? configData = JsonSerializer
                    .Deserialize<LogConfigData>(json);
                config = configData ?? throw new JsonException("Json 解析失败");
            }
            catch (Exception e)
            {
                EventBus.Publish(new LogEvent(LogLevel.Error, e.Message));
            }
        }
        else
        {
            // 帮你写好配置文件咯～下次别忘了！
            Directory.CreateDirectory(LogConfigPath);
            File.WriteAllText(LogConfigFullPath, JsonSerializer.Serialize(config));
        }

        return config;
    }

    public IEventBus EventBus { get; }
}

public class LogConfigData(string logPath)
{
    public string LogPath { get; } = logPath;
}