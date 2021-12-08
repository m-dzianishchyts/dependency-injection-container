using System.Collections;
using System.Runtime.CompilerServices;

namespace DIContainer.Core;

public class DependencyContainer
{
    private readonly ISet<Type> _instantiatingTypes = new HashSet<Type>();

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

        Dependency dependency = DetermineDependency(@interface, name);
        object instance = ResolveExplicitDependency(dependency);
        return instance;
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
            if (_instantiatingTypes.Contains(dependency.Type))
            {
                throw new InvalidOperationException(
                    $"Dependency type {dependency.Type} leads to recursive resolving");
            }

            _instantiatingTypes.Add(dependency.Type);

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
                        lock (this)
                        {
                            dependency.Instance ??= DependencyInstantiatingHelper
                                .Instantiate(dependency.Type, this);
                            instance = dependency.Instance;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dependency),
                                                              nameof(dependency.InstanceAccessMode));
                }
            }

            _instantiatingTypes.Remove(dependency.Type);

            return instance;
        }
        catch (Exception)
        {
            _instantiatingTypes.Clear();
            throw;
        }
    }

    private Dependency GetGenericDependency(Type @interface, string? name = null)
    {
        // Type genericType = @interface.GetGenericTypeDefinition();
        if (DependencyConfig.Dependencies.TryGetValue(@interface, out List<Dependency>? genericDependencies))
        {
            Dependency resultGenericDependency;
            if (name is null)
            {
                resultGenericDependency = genericDependencies.First();
                return resultGenericDependency;
            }

            try
            {
                resultGenericDependency = genericDependencies
                    .First(genericDependency => name.Equals(genericDependency.Name));
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException($"Dependency for the type {@interface} named " +
                                            $"{name} is not registered", e);
            }

            return resultGenericDependency;
        }

        throw new ArgumentException($"Dependency for the type {@interface} is not registered");
    }

    private Dependency DetermineDependency(Type @interface, string? name = null)
    {
        Dependency resultDependency;
        if (@interface.IsGenericType)
        {
            resultDependency = GetGenericDependency(@interface, name);
            return resultDependency;
        }

        if (DependencyConfig.Dependencies.TryGetValue(@interface, out List<Dependency>? dependencies))
        {
            if (name is null)
            {
                resultDependency = dependencies.First();
                return resultDependency;
            }

            try
            {
                resultDependency = dependencies.First(dependency => name.Equals(dependency.Name));
                return resultDependency;
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
