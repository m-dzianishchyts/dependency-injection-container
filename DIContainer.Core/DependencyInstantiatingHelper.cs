using System.Reflection;

namespace DIContainer.Core;

public class DependencyInstantiatingHelper
{
    internal static object Instantiate(Type type, DependenciesConfig dependenciesConfig)
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
                IEnumerable<object> arguments = InstantiateParameters(parameters, dependenciesConfig);
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
                                                             DependenciesConfig dependenciesConfig)
    {
        var parameterInstances = new List<object>();
        foreach (ParameterInfo parameter in parameters)
        {
            if (dependenciesConfig.Dependencies.ContainsKey(parameter.ParameterType))
            {
                Dependency dependency = dependenciesConfig.Dependencies[parameter.ParameterType].First();
                object instance = Instantiate(dependency.GetType(), dependenciesConfig);
                parameterInstances.Add(instance);
            }
            else
            {
                throw new ArgumentException("No dependency registered for a parameter " +
                                            $"(${parameter.ParameterType} {parameter.Name})");
            }
        }

        return parameterInstances;
    }
}
