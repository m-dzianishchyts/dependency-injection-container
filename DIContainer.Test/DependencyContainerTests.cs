using System;
using System.Collections.Generic;
using System.Linq;
using DIContainer.Core;
using DIContainer.Test.TestData;
using NUnit.Framework;

namespace DIContainer.Test
{
    [TestFixture]
    public class DependencyContainerTests
    {
        private static DependencyConfig? _dependencyConfig;
        private static DependencyContainer? _dependencyContainer;

        [SetUp]
        public static void SetUp()
        {
            _dependencyConfig = new DependencyConfig();
            _dependencyContainer = new DependencyContainer(_dependencyConfig);
        }

        [Test]
        public static void DependencyContainer_Resolve_ITrivial()
        {
            _dependencyConfig!.Register<ITrivial, TrivialA>();
            var instance = _dependencyContainer!.Resolve<ITrivial>();

            Assert.AreSame(typeof(TrivialA), instance.GetType());
        }

        [Test]
        public static void DependencyContainer_Resolve_Dependent()
        {
            _dependencyConfig!.Register<ITrivial, TrivialA>();
            _dependencyConfig.Register<Dependent, Dependent>();
            var instance = _dependencyContainer!.Resolve<Dependent>();

            Assert.AreSame(typeof(Dependent), instance.GetType());
            Assert.AreSame(typeof(TrivialA), instance.Trivial.GetType());
        }

        [Test]
        public static void DependencyContainer_Resolve_IGenericDependent()
        {
            _dependencyConfig!.Register<ITrivial, TrivialA>();
            _dependencyConfig.Register<IGenericDependent<ITrivial>, GenericDependent<ITrivial>>();
            var instance = _dependencyContainer!.Resolve<IGenericDependent<ITrivial>>();

            Assert.AreSame(typeof(GenericDependent<ITrivial>), instance.GetType());
            Assert.AreSame(typeof(TrivialA), instance.Trivial.GetType());
        }

        [Test]
        public static void DependencyContainer_Resolve_AccessMode_Transient()
        {
            _dependencyConfig!.Register<ITrivial, TrivialA>();
            var instanceA = _dependencyContainer!.Resolve<ITrivial>();
            var instanceB = _dependencyContainer!.Resolve<ITrivial>();

            Assert.AreNotSame(instanceA, instanceB);
        }

        [Test]
        public static void DependencyContainer_Resolve_AccessMode_Singleton()
        {
            _dependencyConfig!.Register<ITrivial, TrivialA>(Dependency.AccessMode.Singleton);
            var instanceA = _dependencyContainer!.Resolve<ITrivial>();
            var instanceB = _dependencyContainer!.Resolve<ITrivial>();

            Assert.AreSame(instanceA, instanceB);
        }

        [Test]
        public static void DependencyContainer_Resolve_IEnumerable()
        {
            var expectedTypes = new List<Type> { typeof(TrivialA), typeof(TrivialB) };
            expectedTypes.ForEach(dependency => _dependencyConfig!.Register(typeof(ITrivial), dependency));

            List<ITrivial> instances = _dependencyContainer!.Resolve<IEnumerable<ITrivial>>().ToList();
            IEnumerable<Type> actualTypes = instances.Select(instance => instance.GetType());

            CollectionAssert.AreEquivalent(expectedTypes, actualTypes);
        }

        [Test]
        public static void DependencyContainer_ResolveAll()
        {
            var expectedTypes = new List<Type> { typeof(TrivialA), typeof(TrivialB) };
            expectedTypes.ForEach(dependency => _dependencyConfig!.Register(typeof(ITrivial), dependency));

            List<ITrivial> instances = _dependencyContainer!.ResolveAll<ITrivial>().ToList();
            IEnumerable<Type> actualTypes = instances.Select(instance => instance.GetType());

            CollectionAssert.AreEquivalent(expectedTypes, actualTypes);
        }

        [Test]
        public static void DependencyContainer_Resolve_ThrowsOn_CyclicDependency()
        {
            _dependencyConfig!.Register<ITrivial, CyclicallyDependentA>();
            _dependencyConfig.Register<ITrivial, CyclicallyDependentB>();

            Assert.Throws<AggregateException>(() => _dependencyContainer!.Resolve<ITrivial>());
        }

        [Test]
        public static void DependencyContainer_Resolve_ThrowsOn_SelfCyclicDependency()
        {
            _dependencyConfig!.Register<ITrivial, SelfDependent>();

            Assert.Throws<AggregateException>(() => _dependencyContainer!.Resolve<ITrivial>());
        }

        [Test]
        public static void DependencyContainer_Resolve_ThrowsOn_NamedDependencyNotFound()
        {
            _dependencyConfig!.Register<ITrivial, TrivialA>(nameof(TrivialA));

            Assert.Throws<ArgumentException>(() => _dependencyContainer!.Resolve<ITrivial>(nameof(TrivialB)));
        }

        [Test]
        public static void DependencyContainer_Resolve_NamedDependency()
        {
            _dependencyConfig!.Register<ITrivial, TrivialA>(nameof(TrivialA));
            var instance = _dependencyContainer!.Resolve<ITrivial>(nameof(TrivialA));

            Assert.AreSame(typeof(TrivialA), instance.GetType());
        }
    }
}
