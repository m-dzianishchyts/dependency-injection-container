using System.Reflection;

namespace DIContainer.Core;

public class DependencyInstantiatingHelper
{
    internal static object Instantiate(Type type, DependencyContainer dependencyContainer)
    {
        List<ConstructorInfo> constructors = type.GetConstructors()
            .Where(constructor => constructor.GetParameters()
                       .All(parameter => parameter.GetType().IsClass))
            .ToList();
        if (!constructors.Any())
        {
            throw new ArgumentException($"No appropriate constructors present ({type})");
        }

        var constructorInvocationFails = new List<Exception>();
        foreach (ConstructorInfo constructor in constructors)
        {
            try
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                IEnumerable<object> arguments = InstantiateParameters(parameters, dependencyContainer);
                object instance = constructor.Invoke(arguments.ToArray());
                return instance;
            }
            catch (Exception e)
            {
                constructorInvocationFails.Add(e);
            }
        }

        throw new AggregateException(constructorInvocationFails);
    }

    private static IEnumerable<object> InstantiateParameters(IEnumerable<ParameterInfo> parameters,
                                                             DependencyContainer dependencyContainer)
    {
        IEnumerable<object> instances =
            parameters.Select(parameter => dependencyContainer.Resolve(parameter.ParameterType));
        return instances;
    }
}
