using System;
using System.Reflection;
using Newtonsoft.Json;
using NinjaTasks.Model;
using NUnit.Framework;

namespace NinjaTasks.Tests
{
 
    [TestFixture]
    public class TestReflection
    {
        private JsonToken? e;

        class X<T>
        {
            #pragma warning disable  CS0169
            private int x;
            #pragma warning restore CS0169

            public virtual int XX { get; set; }

            public void Do(T x)
            {
            }
            public void Do(object x)
            {
            }

            public object Do<TT>(object y)
            {
                return null;
            }

            public void Do<YY>(JsonToken y)
            {
            }
        }


 
        [Test]
        public void TestReflectionSynthetic()
        {
            var fields = typeof(X<>).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static|BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                Console.WriteLine("Name: {0}; DeclaringType: {1}", field.Name, field.DeclaringType);
            }

        }

        [Test]
        public void TestProps()
        {
            var task = new TodoTask();
            System.Type t = task.GetType();

            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var prop in props)
            {
                Console.WriteLine("Name: {0}; DeclaringType: {1}", prop.Name, prop.DeclaringType);
            }

        }

        [Test]
        public void TestEquality()
        {
            var task = new TodoTask();
            var task2 = new TodoTask();
            System.Type t = task.GetType();
            System.Type t2 = task2.GetType();

            var prop1 = t.GetProperty("Id");
            var prop2 = t2.GetProperty("Id");

            Assert.AreEqual(prop1, prop2);
            Assert.AreSame(prop1, prop2);

        }

        [Test]
        public void TestEnum()
        {
            Console.WriteLine(typeof(JsonToken?).ToString());
            Console.WriteLine(Nullable.GetUnderlyingType(typeof(JsonToken?)).ToString());

            Assert.AreEqual(typeof(JsonToken), Nullable.GetUnderlyingType(typeof(JsonToken?)));

            Assert.AreEqual(JsonToken.None, e.GetValueOrDefault());
            Assert.AreEqual(JsonToken.Comment, e.GetValueOrDefault(JsonToken.Comment));

            try
            {
                Console.WriteLine(e.GetType().ToString());
                Assert.Fail("should throw");
            }
            catch (NullReferenceException)
            {
            }
            
            e = JsonToken.Boolean;

            Assert.AreEqual(typeof(JsonToken), e.GetType());
            Assert.IsNull(Nullable.GetUnderlyingType(e.GetType()));

            Assert.AreEqual(JsonToken.Boolean, e.GetValueOrDefault());
            Assert.AreEqual(JsonToken.Boolean, e.GetValueOrDefault(JsonToken.Comment));

            e = null;

            Assert.AreEqual(JsonToken.None, e.GetValueOrDefault());
            Assert.AreEqual(JsonToken.Comment, e.GetValueOrDefault(JsonToken.Comment));


            try
            {
                Console.WriteLine(e.GetType().ToString());
                Assert.Fail("should throw");
            }
            catch (NullReferenceException)
            {
            }
            

            
        }

        [Test]
        public void TestAssignableFrom()
        {
            int i = 1;
            long l = 2;
            int? i2 = 3;

            
            Console.WriteLine(i.GetType().IsAssignableFrom(l.GetType()));
            Console.WriteLine(l.GetType().IsAssignableFrom(i.GetType()));

            Console.WriteLine(i.GetType().IsAssignableFrom(i2.GetType()));
            Console.WriteLine(i2.GetType().IsAssignableFrom(i.GetType()));

            Console.WriteLine(i.GetType().IsInstanceOfType(i2));
            Console.WriteLine(i2.GetType().IsInstanceOfType(i));

            Console.WriteLine("" + (((object)i) is int?));
            Console.WriteLine("" + (((object)i2) is int));

            Console.WriteLine("" + (i.GetType() == typeof(int?)));
            Console.WriteLine("" + (i2.GetType() == typeof(int)));

             //new TestGenericsCreateGenericInstance().testCorrectGenericInstance();
            object x = (object) 1;
            if (x.GetType().FullName == "")
                x = "";

            Console.WriteLine("" + x.GetType().FullName);
            Assert.IsTrue(x is int);
            Assert.IsTrue(x.GetType() == typeof(int));
            

        }
    }
}
