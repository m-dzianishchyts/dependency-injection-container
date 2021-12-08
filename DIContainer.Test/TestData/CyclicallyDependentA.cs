namespace DIContainer.Test.TestData
{
    public class CyclicallyDependentA : ITrivial
    {
        public CyclicallyDependentA(CyclicallyDependentB cyclicallyDependentB)
        {
        }
    }
}