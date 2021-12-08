namespace DIContainer.Core;

public static class TypeExtensions
{
    internal static bool IsAssignableFromGeneric(this Type @interface, Type implementation)
    {
        if (@interface.IsGenericTypeDefinition || !implementation.IsGenericTypeDefinition)
        {
            return false;
        }

        Type interfaceTypeDefinition = @interface.DetermineTypeDefinition();
        Type implementationTypeDefinition = implementation.DetermineTypeDefinition();
        IEnumerable<Type> assignableTypes = implementationTypeDefinition.DetermineAssignableTypes();
        bool isAssignable = assignableTypes
            .Select(DetermineTypeDefinition)
            .Contains(interfaceTypeDefinition);
        return isAssignable;
    }

    private static Type DetermineTypeDefinition(this Type type)
    {
        Type typeDefinition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        return typeDefinition;
    }

    private static IEnumerable<Type> DetermineAssignableTypes(this Type type)
    {
        var ancestorTypes = new List<Type>();
        for (Type? ancestorType = type; ancestorType != null; ancestorType = ancestorType.BaseType)
        {
            ancestorTypes.Add(ancestorType);
        }

        IEnumerable<Type> interfaces = type.GetInterfaces().SelectMany(DetermineAssignableTypes).Distinct();
        IEnumerable<Type> assignableTypes = ancestorTypes.Concat(interfaces);
        return assignableTypes;
    }
}
