using System;
using System.Linq;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NUnit.Framework;

namespace NinjaTasks.Tests
{
    [TestFixture]
    public class TestStorage
    {
        [Test]
        public void TestGetById()
        {
            var conFac = Helpers.CreateConnectionFactory();
            var fac = new SQLiteFactory(conFac, "test.sqlite");
            Helpers.ClearDatabase(fac);
            var db = new MvxSqliteTodoStorage(fac.Get("tests"));

            var list = new TodoList();
            list.Description = "test";
            db.Save(list);
            var todoTask = new TodoTask { Description = "test", ListFk = list.Id};  
            db.Save(todoTask);

            var tasks = db.GetIds(SelectionMode.SelectSpecified, TrackableType.Task, todoTask.Id)
                          .ToList();

            Assert.AreEqual(1,tasks.Count);
            Assert.AreEqual(todoTask.Id, tasks[0].ObjectId);

        }

        [Test]
        public void TestAdditionalProperties()
        {
            var conFac = Helpers.CreateConnectionFactory();
            var fac = new SQLiteFactory(conFac, "test.sqlite");
            Helpers.ClearDatabase(fac);

            var db = new MvxSqliteTodoStorage(fac.Get("tests"));

            var list = new TodoList();
            list.Description = "test";
            db.Save(list);
            var todoTask = new TodoTask { Description = "test", ListFk = list.Id };

            todoTask.SetProperty("ext1", "ext1propertyValue");
            db.Save(todoTask);

            todoTask = db.GetTasks(ids: todoTask.Id).First();

            Assert.IsTrue(todoTask.AdditionalProperties.Count == 1);
            Assert.AreEqual("ext1propertyValue", todoTask.GetProperty("ext1"));

            todoTask.SetProperty("ext1", null);
            db.Save(todoTask);

            todoTask = db.GetTasks(ids: todoTask.Id).First();

            Assert.IsTrue(todoTask.AdditionalProperties.Count == 1);
            Assert.AreEqual(null, todoTask.GetProperty("ext1"));

            db.DeleteTask(todoTask);

        }

        [Test]
        public void TestTicks()
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Console.WriteLine("unixepoch-ticks:" + unixEpoch.Ticks);

        }
    }
}
