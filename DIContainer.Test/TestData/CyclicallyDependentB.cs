namespace DIContainer.Test.TestData;

internal class CyclicallyDependentB : ITrivial
{
    public CyclicallyDependentB(CyclicallyDependentA cyclicallyDependentA)
    {
    }
}
