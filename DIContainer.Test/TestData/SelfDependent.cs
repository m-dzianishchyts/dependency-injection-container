namespace DIContainer.Test.TestData;

public class SelfDependent : ITrivial
{
    public SelfDependent(SelfDependent selfDependent)
    {
    }
}
