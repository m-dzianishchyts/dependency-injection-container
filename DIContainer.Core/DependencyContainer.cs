using System.Collections;
using System.Runtime.CompilerServices;

namespace DIContainer.Core;

public class DependencyContainer
{
    private readonly ThreadLocal<ISet<Type>> _instantiatingTypes = new(() => new HashSet<Type>());

    public DependencyContainer(DependencyConfig dependencyConfig)
    {
        DependencyConfig = dependencyConfig;
    }

    internal DependencyConfig DependencyConfig { get; }

    public TInterface Resolve<TInterface>(string? name = null)
    {
        return (TInterface) Resolve(typeof(TInterface), name);
    }

    public object Resolve(Type @interface, string? name = null)
    {
        if (typeof(IEnumerable).IsAssignableFrom(@interface))
        {
            return ResolveAll(@interface.GetGenericArguments().First());
        }

        IEnumerable<Dependency> dependencies = DetermineDependencies(@interface, name);
        var resolvingFails = new List<Exception>();
        foreach (Dependency dependency in dependencies)
        {
            try
            {
                object instance = ResolveExplicitDependency(dependency);
                return instance;
            }
            catch (Exception e)
            {
                resolvingFails.Add(e);
            }
        }

        throw new AggregateException(resolvingFails);
    }

    public IEnumerable<TInterface> ResolveAll<TInterface>()
        where TInterface : class
    {
        return (IEnumerable<TInterface>) ResolveAll(typeof(TInterface));
    }

    public IEnumerable<object> ResolveAll(Type @interface)
    {
        if (DependencyConfig.Dependencies.TryGetValue(@interface, out List<Dependency>? dependencies))
        {
            var collection = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(@interface))!;

            foreach (Dependency dependency in dependencies)
            {
                collection.Add(ResolveExplicitDependency(dependency));
            }

            return (IEnumerable<object>) collection;
        }

        throw new ArgumentException($"Dependency for the type {@interface} is not registered");
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private object ResolveExplicitDependency(Dependency dependency)
    {
        try
        {
            if (_instantiatingTypes.Value!.Contains(dependency.Type))
            {
                throw new InvalidOperationException(
                    $"Dependency type {dependency.Type} leads to recursive resolving");
            }

            _instantiatingTypes.Value.Add(dependency.Type);

            object instance;
            if (dependency.Type.IsAbstract)
            {
                instance = Resolve(dependency.Type);
            }
            else
            {
                switch (dependency.InstanceAccessMode)
                {
                    case Dependency.AccessMode.Transient:
                    {
                        instance = DependencyInstantiatingHelper.Instantiate(dependency.Type, this);
                        break;
                    }
                    case Dependency.AccessMode.Singleton:
                    {
                        dependency.Instance ??= DependencyInstantiatingHelper
                            .Instantiate(dependency.Type, this);
                        instance = dependency.Instance;

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dependency),
                                                              nameof(dependency.InstanceAccessMode));
                }
            }

            _instantiatingTypes.Value.Remove(dependency.Type);

            return instance;
        }
        catch (Exception)
        {
            _instantiatingTypes.Value!.Clear();
            throw;
        }
    }

    private IEnumerable<Dependency> DetermineDependencies(Type @interface, string? name = null)
    {
        if (DependencyConfig.Dependencies.TryGetValue(@interface, out List<Dependency>? dependencies))
        {
            IEnumerable<Dependency> resultDependencies;
            if (name is null)
            {
                resultDependencies = dependencies;
                return resultDependencies;
            }

            try
            {
                resultDependencies = dependencies.Where(dependency => name.Equals(dependency.Name));
                return resultDependencies;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException($"Dependency for the type {@interface} named " +
                                            $"{name} is not registered", e);
            }
        }

        throw new ArgumentException($"Dependency for the type {@interface} is not registered");
    }
}
