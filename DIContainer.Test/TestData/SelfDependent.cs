namespace DIContainer.Test.TestData;

internal class SelfDependent : ITrivial
{
    public SelfDependent(SelfDependent selfDependent)
    {
    }
}
