using System;
using System.Diagnostics;
using System.Reflection;
using NinjaTools.Npc;
using NUnit.Framework;

namespace NinjaTasks.Tests
{
    [TestFixture]
    public class TestGetterSetterPerformance
    {
        class TestClass
        {
            public DateTime DtProp { get; set; }
            public int IntProp { get; set; }
            public TestClass ClassProp { get; set; }
        }

        [Test]
        public void TestReferenceDelegates()
        {
            TestClass obj = new TestClass();
            TestClass obj1 = new TestClass();

            var prop = obj.GetType().GetProperty("ClassProp");

            RunTest(prop, obj1, obj);
        }

        [Test]
        public void TestIntDelegates()
        {
            TestClass obj = new TestClass();
            TestClass obj1 = new TestClass();

            var prop = obj.GetType().GetProperty("IntProp");

            RunTest(prop, obj, obj1);
        }

        [Test]
        public void TestDateTimeDelegates()
        {
            TestClass obj = new TestClass();
            TestClass obj1 = new TestClass();

            var prop = obj.GetType().GetProperty("DtProp");

            RunTest(prop, obj, obj1);
        }

        private static void RunTest(PropertyInfo prop, TestClass obj1, TestClass obj)
        {
            Func<TestClass, object> typedGetter = prop.CreateGet<TestClass>();
            Action<TestClass, object> typedSetter = prop.CreateSet<TestClass>();

            const int iterations = 1000000;

            Stopwatch w = new Stopwatch();
            w.Start();

            for (int i = 0; i < iterations; ++i)
            {
                typedSetter(obj1, typedGetter(obj));
            }

            w.Stop();
            Console.WriteLine("Elapsed w/ optimized, typed getter/setter: {0}", w.Elapsed);

            w = new Stopwatch();
            w.Start();

            Func<object, object> untypedGetter = prop.CreateObjectGetter();
            Action<object, object> untypedSetter = prop.CreateObjectSetter();

            for (int i = 0; i < iterations; ++i)
            {
                untypedSetter(obj1, untypedGetter(obj));
            }

            w.Stop();
            Console.WriteLine("Elapsed w/ optimized,untyped getter/setter: {0}", w.Elapsed);

            w = new Stopwatch();
            w.Start();

            Func<object, object> defaultGetter = prop.GetValue;
            Action<object, object> defaultSetter = prop.SetValue;

            for (int i = 0; i < iterations; ++i)
            {
                defaultSetter(obj1, defaultGetter(obj));
            }

            w.Stop();
            Console.WriteLine("Elapsed w/ standard, untyped getter/setter: {0}", w.Elapsed);
        }
    }
}
