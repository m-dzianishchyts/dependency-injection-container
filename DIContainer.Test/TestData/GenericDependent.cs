namespace DIContainer.Test.TestData;

public class GenericDependent<T> : IGenericDependent<T>
    where T : ITrivial
{
    public GenericDependent(T trivial)
    {
        Trivial = trivial;
    }

    public T Trivial { get; set; }
}
