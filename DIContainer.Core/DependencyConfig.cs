namespace DIContainer.Core
{
    public class DependencyConfig
    {
        internal Dictionary<Type, List<Dependency>> Dependencies { get; }

        public DependencyConfig()
        {
            Dependencies = new Dictionary<Type, List<Dependency>>();
        }

        public void Register<TInterface, TImplementation>(string name,
                                                          Dependency.AccessMode accessMode =
                                                              Dependency.AccessMode.Transient)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            RegisterExplicit(typeof(TInterface), typeof(TImplementation), name, accessMode);
        }

        public void Register<TInterface, TImplementation>(Dependency.AccessMode accessMode =
                                                              Dependency.AccessMode.Transient)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            RegisterExplicit(typeof(TInterface), typeof(TImplementation), null, accessMode);
        }

        public void Register(Type @interface, Type implementation,
                             string name, Dependency.AccessMode accessMode =
                                 Dependency.AccessMode.Transient)
        {
            RegisterExplicit(@interface, implementation, name, accessMode);
        }

        public void Register(Type @interface, Type implementation,
                             Dependency.AccessMode accessMode =
                                 Dependency.AccessMode.Transient)
        {
            RegisterExplicit(@interface, implementation, null, accessMode);
        }

        private void RegisterExplicit(Type @interface, Type implementation, string? name,
                                      Dependency.AccessMode accessMode)
        {
            if (implementation.IsValueType)
            {
                throw new ArgumentException($"{implementation} cannot be a value type");
            }

            if (implementation.IsAbstract || implementation.IsInterface)
            {
                throw new ArgumentException($"{implementation} must be a concrete class");
            }

            if (!@interface.IsAssignableFrom(implementation) && !implementation.IsAssignableFromGeneric(@interface))
            {
                throw new ArgumentException($"{@interface} is not assignable from {implementation}");
            }

            var dependency = new Dependency(implementation, name, accessMode);
            if (Dependencies.ContainsKey(@interface))
            {
                Dependencies[@interface].Add(dependency);
            }
            else
            {
                Dependencies.Add(@interface, new List<Dependency> { dependency });
            }
        }
    }
}
