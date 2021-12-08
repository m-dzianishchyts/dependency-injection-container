namespace DIContainer.Test.TestData;

internal class CyclicallyDependentA : ITrivial
{
    public CyclicallyDependentA(CyclicallyDependentB cyclicallyDependentB)
    {
    }
}
