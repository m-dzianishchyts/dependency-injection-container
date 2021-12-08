namespace DIContainer.Core;

public class Dependency
{
    public enum AccessMode
    {
        Transient,
        Singleton
    }

    internal Dependency(Type type, string? name, AccessMode instanceAccessMode)
    {
        Type = type;
        InstanceAccessMode = instanceAccessMode;
        Name = name;
    }

    internal Type Type { get; }

    internal AccessMode InstanceAccessMode { get; }

    internal string? Name { get; }

    internal object? Instance { get; set; }
}
