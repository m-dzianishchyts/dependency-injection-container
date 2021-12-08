namespace DIContainer.Test.TestData;

public interface IGenericDependent<T> where T : ITrivial
{
    T Trivial { get; set; }
}
