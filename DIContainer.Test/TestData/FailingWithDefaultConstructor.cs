using System;

namespace DIContainer.Test.TestData
{
    internal class FailingWithDefaultConstructor : ITrivial
    {
        public FailingWithDefaultConstructor()
        {
            throw new Exception();
        }

        public FailingWithDefaultConstructor(ITrivial trivial)
        {
        }
    }
}
