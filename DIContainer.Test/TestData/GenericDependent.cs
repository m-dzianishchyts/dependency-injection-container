namespace DIContainer.Test.TestData;

internal class GenericDependent<T> : IGenericDependent<T>
    where T : ITrivial
{
    public GenericDependent(T trivial)
    {
        Trivial = trivial;
    }

    public T Trivial { get; set; }
}
