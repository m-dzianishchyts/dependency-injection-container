namespace DIContainer.Test.TestData;

internal class Dependent
{
    public ITrivial Trivial;

    public Dependent(ITrivial trivial)
    {
        Trivial = trivial;
    }
}
