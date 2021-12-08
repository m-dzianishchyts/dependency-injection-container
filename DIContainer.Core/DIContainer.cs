using System.Collections;
using System.Runtime.CompilerServices;

namespace DIContainer.Core
{
    public class DIContainer
    {
        private readonly DependenciesConfig _dependenciesConfig;

        private readonly ISet<Type> _instantiatingTypes = new HashSet<Type>();

        public DIContainer(DependenciesConfig dependenciesConfig)
        {
            _dependenciesConfig = dependenciesConfig;
        }

        public TInterface Resolve<TInterface>(string? name = null)
        {
            return (TInterface) ResolveExplicit(typeof(TInterface), name);
        }

        public object ResolveExplicit(Type @interface, string? name = null)
        {
            if (typeof(IEnumerable<>).IsAssignableFrom(@interface))
            {
                return ResolveAll(@interface.GetGenericArguments().First());
            }

            Dependency dependency = GetDependency(@interface, name);

            object instance = ResolveDependency(dependency);
            return instance;
        }

        public IEnumerable<TInterface> ResolveAll<TInterface>()
            where TInterface : class
        {
            return (IEnumerable<TInterface>) ResolveAll(typeof(TInterface));
        }

        public IEnumerable<object> ResolveAll(Type @interface)
        {
            if (_dependenciesConfig.Dependencies.TryGetValue(@interface, out List<Dependency>? dependencies))
            {
                var collection = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(@interface))!;

                foreach (Dependency dependency in dependencies)
                {
                    collection.Add(ResolveDependency(dependency));
                }

                return (IEnumerable<object>) collection;
            }

            throw new ArgumentException($"Dependency for the type {@interface} is not registered");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private object ResolveDependency(Dependency dependency)
        {
            if (_instantiatingTypes.Contains(dependency.Type))
            {
                throw new InvalidOperationException($"Dependency type {dependency.Type} leads to recursive resolving");
            }

            _instantiatingTypes.Add(dependency.Type);

            object instance;
            switch (dependency.InstanceAccessMode)
            {
                case Dependency.AccessMode.Transient:
                {
                    instance = DependencyInstantiatingHelper.Instantiate(dependency.Type, _dependenciesConfig);
                    break;
                }
                case Dependency.AccessMode.Singleton:
                {
                    lock (this)
                    {
                        dependency.Instance ??=
                            DependencyInstantiatingHelper.Instantiate(dependency.Type, _dependenciesConfig);
                        instance = dependency.Instance;
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(dependency), nameof(dependency.InstanceAccessMode));
            }

            _instantiatingTypes.Remove(dependency.Type);

            return instance;
        }

        private Dependency GetGenericDependency(Type @interface, string? name = null)
        {
            Type genericType = @interface.GetGenericTypeDefinition();
            if (_dependenciesConfig.Dependencies.TryGetValue(genericType, out List<Dependency>? genericDependencies))
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

        private Dependency GetDependency(Type @interface, string? name = null)
        {
            Dependency resultDependency;
            if (@interface.IsGenericType)
            {
                resultDependency = GetGenericDependency(@interface, name);
                return resultDependency;
            }

            if (_dependenciesConfig.Dependencies.TryGetValue(@interface, out List<Dependency>? dependencies))
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
}
