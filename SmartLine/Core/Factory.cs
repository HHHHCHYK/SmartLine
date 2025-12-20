using System.Reflection;
using System.Runtime.Loader;
using Abstractions;
using Abstractions.Attributes;
using Abstractions.Events;
using SmartLine.util;

namespace SmartLine;

public static class Factory
{
    // ReSharper disable once CollectionNeverQueried.Local
    private static Dictionary<Type, object> ModuleInstances { get; } = new();

    public static void LoadModule()
    {
        MainEventBus.Instance.Publish(new LogEvent(LogLevel.Info, "开始加载模块"));
        // 读取配置
        var config = LoadConfig();

        // 读取模块文件夹中的模块
        var modules = GetModules(config);

        // 创建对应模块的实例
        InstantiateModules(config, modules);
        MainEventBus.Instance.Publish(new LogEvent(LogLevel.Info, "模块加载完毕"));
    }

    private static void InstantiateModules(ConfigData config, HashSet<Type> modules)
    {
        var moduleSelectors = config.ModuleSelectors;

        // 为每一个功能接口加载模块
        foreach (var selectorsKey in moduleSelectors.Keys)
        {
            // 如果Key的Value为空值
            if (string.IsNullOrEmpty(moduleSelectors[selectorsKey]))
            {
                MainEventBus.Instance.Publish(new LogEvent(LogLevel.Warning, "模块对应的配置项为空"));
                continue;
            }

            // 读取ModuleName和目标接口类型（由Key定义）
            var moduleName = moduleSelectors[selectorsKey];
            var targetInterface = GetModuleInterfaceByName(selectorsKey);

            // 遍历modules，查找目标type
            var targetModule = modules.FirstOrDefault
            (m =>
                {
                    // 获取属性
                    var obj = m.GetCustomAttributes(typeof(ModuleAttribute), inherit: true);
                    var attribute = obj.FirstOrDefault(o => o is ModuleAttribute) as ModuleAttribute;
                    // 属性判空
                    if (attribute == null) return false;
                    // 判断属性值
                    if (attribute.ModuleName != moduleName) return false;
                    // 判断是否是目标接口的实现
                    return targetInterface.IsAssignableFrom(m);
                }
            );

            foreach (var m in modules)
            {
                var baseAttributes = Attribute.GetCustomAttributes(m);
                var attribute = baseAttributes.FirstOrDefault() as ModuleAttribute;
                // 属性判空
                if (attribute == null) continue;
                // 判断属性值
                if (attribute.ModuleName != moduleName) continue;
                //判断是否是目标接口的实现
                if (targetInterface.IsAssignableFrom(m))
                {
                    targetModule = m;
                    break;
                }
            }

            if (targetModule == null) // 如果获取失败
            {
                MainEventBus.Instance.Publish(new LogEvent(LogLevel.Warning, "配置中的程序集缺失"));
                return;
            }

            // 创建实例
            var obj = Activator.CreateInstance(
                targetModule,
                MainEventBus.Instance
            );
            if (obj == null || !(obj is IModule && targetInterface.IsInstanceOfType(obj))) return;
            ModuleInstances.Add(targetInterface, obj);
        }
    }

    private static Assembly? AbstractionsAssembly;

    private static Type GetModuleInterfaceByName(string name)
    {
        // 若还未获取契约程序集，先获取
        if (AbstractionsAssembly == null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var abstractionsAssembly =
                assemblies.FirstOrDefault(a => a.GetName().Name?.EndsWith(nameof(Abstractions)) == true);
            if (abstractionsAssembly == null) throw new InvalidOperationException();
            AbstractionsAssembly = abstractionsAssembly;
        }

        // 获取程序集中的类
        var type = AbstractionsAssembly.GetType($"{nameof(Abstractions)}.{name}");
        if (type is not { IsInterface: true }) throw new InvalidOperationException();
        return type;
    }

    /// <summary>
    /// 将模块文件夹中的程序集加载到内存中，仅读取*.dll
    /// </summary>
    /// <param name="config">配置数据</param>
    private static HashSet<Type> GetModules(ConfigData config)
    {
        HashSet<Type> ret = new();

        //注册加载器
        RegisteringAssemblyResolveCallback();

        foreach (var path in config.ModulePaths)
        {
            // 将相对路径转换成绝对路径
            var fullPath = GetFullPath(path);
            
            if (!Directory.Exists(fullPath)) continue; //验证路径

            var files = Directory.GetFiles(fullPath, "*.dll"); //读取路径下所有dll文件
            foreach (var file in files) //遍历加载所有dll文件
            {
                if (string.IsNullOrEmpty(file)) continue; //保证文件名有效
                if (file.EndsWith("Abstractions.dll")) continue;
                Assembly moduleType = AssemblyLoadContext.Default.LoadFromAssemblyPath(file); //加载程序集
                var types = moduleType.GetTypes();
                foreach (var type in types) //筛选
                {
                    if (!typeof(IModule).IsAssignableFrom(type)) continue;
                    if (type.IsInterface || type.IsAbstract) continue;
                    ret.Add(type);
                }
            }
        }

        return ret;
    }

    /// <summary>
    /// 注册程序解析集回调
    /// </summary>
    private static void RegisteringAssemblyResolveCallback()
    {
        // 在加载任何模块之前执行
        AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
        {
            // 1. 存放公共 DLL 的“核心目录”
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 2. 拼接出可能的路径
            string expectedPath = Path.Combine(baseDir, $"{assemblyName.Name}.dll");

            // 3. 如果文件确实在根目录，就手动喂给加载器
            if (File.Exists(expectedPath))
            {
                return context.LoadFromAssemblyPath(expectedPath);
            }

            return null;
        };
    }

    private static string GetFullPath(string path)
    {
        // 如果是绝对路径，直接返回
        if (Path.IsPathFullyQualified(path))
            return path;
        // 如非，转换成绝对路径
        else
        {
            var absoluteDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
            var aPathLength = absoluteDirectoryPath.Length;
            
            // 判断路径深度
            var index = path.IndexOf('/');
            // 如果只有单层路径
            if (index <= 0)return Path.Combine(absoluteDirectoryPath, path);
            
            // 如果是更复杂的情况
            if (!path.StartsWith('.'))  //  如果不以.开头，代表目录不需要回溯
            {
                return  Path.Combine(absoluteDirectoryPath, path);
            }
            else // 如果以"."开头，则按数量回溯
            {
                
            }
            
            return Path.GetFullPath(path);
        }
    }

    private static ConfigData LoadConfig()
    {
        return ConfigReader.LoadConfig();
    }
}