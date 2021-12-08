namespace DIContainer.Test.TestData
{
    public class CyclicallyDependentB : ITrivial
    {
        public CyclicallyDependentB(CyclicallyDependentA cyclicallyDependentA)
        {
        }
    }
}