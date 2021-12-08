namespace DIContainer.Test.TestData;

public class Dependent
{
    public ITrivial Trivial;

    public Dependent(ITrivial trivial)
    {
        Trivial = trivial;
    }
}
