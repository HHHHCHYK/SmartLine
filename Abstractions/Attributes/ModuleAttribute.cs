namespace Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ModuleAttribute : Attribute
{
    public ModuleAttribute(string moduleName)
    {
        ModuleName = moduleName;
    }
    public string ModuleName { get; }
}