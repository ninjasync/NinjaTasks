using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Android.Util;
using NinjaTasks.Model;
using NinjaTools.Npc;
using NUnit.Framework;

namespace NinjaTasks.App.Dot42.Tests
{

    [DataContract]
    class Test1 : INotifyPropertyChanged
    {
        [DataMember]
        public DateTime Value { get; set; }

        public DateTime Element2 { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [DataContract]
    class Test0 : INotifyPropertyChanged
    {
        public DateTime Value { get; set; }

        public DateTime Element2 { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    class Test3 : INotifyPropertyChanged
    {
        public DateTime Value { get; set; }

        public DateTime Element2 { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class TestNpcBinding
    {
        public void RunTests()
        {
            TestReflection();
            TestTwoWayBind();
            TestTwoWayBindWithTask();
            TestSubscribeWeak();
            TestNullableEquals();
        }


        private void TestReflection()
        {
            PrintProperties(typeof(Test3));
            PrintProperties(typeof(Test0));
            PrintProperties(typeof (Test1));
            PrintProperties(typeof (TodoTask));

            ////var className = typeof (Test1).Name;
            //string className = "ninjaTasks.App.Dot42.Tests.Test1";
            //var cl = new MyClassLoader(ClassLoader.GetSystemClassLoader());
            //var type = cl.LoadClass(className);
            //type.NewInstance();
        }

        private static void PrintProperties(Type t)
        {
            var props = t.GetRuntimeProperties();
            foreach (var prop in props.ToList())
            {
                Log.I("test", string.Format("name: {0} type {1} attributes: {2}",
                    prop.Name,
                    prop.PropertyType,
                    string.Join(",", prop.GetCustomAttributes(true))));
            }
        }


        public void TestTwoWayBind()
        {
            var task = new Test1();
            var test1 = new Test1();

            task.TwoWayBind("Value", test1, "Value");

            task.Value = DateTime.UtcNow;
            Assert.AreEqual(task.Value, test1.Value);
            test1.Value = DateTime.MaxValue;

            Assert.AreEqual(task.Value, test1.Value);
        }

        public void TestTwoWayBindWithTask()
        {
            var task = new TodoTask();
            var test1 = new Test1();

            task.TwoWayBind(TodoTask.ColCreatedAt, test1, "Value");

            task.CreatedAt = DateTime.UtcNow;
            Assert.AreEqual(task.CreatedAt, test1.Value);
            test1.Value = DateTime.MaxValue;

            Assert.AreEqual(task.CreatedAt, test1.Value);
        }

        private void TestSubscribeWeak()
        {
            int count = 0;
            var task = new TodoTask();

            var sub = task.SubscribeWeak(TodoTask.ColDescription, () => ++count);

            task.Description = "1";
            Assert.AreEqual(count, 1);

            task.ModifiedAt = DateTime.UtcNow;
            Assert.AreEqual(count, 1);

            task.Description = "1";
            Assert.AreEqual(count, 1);

            task.Description = "2";
            Assert.AreEqual(count, 2);

            sub.Dispose();

            task.Description = "3";
            Assert.AreEqual(count, 2);
        
        }

        private void TestNullableEquals()
        {
            int count = 0;
            var task = new TodoTask();

            var sub = task.SubscribeWeak(TodoTask.ColCompletedAt, () => ++count);

            task.CompletedAt = DateTime.UtcNow;
            Assert.AreEqual(count, 1);

            task.CompletedAt = new DateTime(2000,1,1);
            Assert.AreEqual(count, 2);

            task.CompletedAt = new DateTime(2000, 1, 1);
            Assert.AreEqual(count, 2);


        }

    }
}
