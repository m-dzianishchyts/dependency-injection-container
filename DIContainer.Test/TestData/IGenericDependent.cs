namespace DIContainer.Test.TestData;

internal interface IGenericDependent<T> where T : ITrivial
{
    T Trivial { get; set; }
}
