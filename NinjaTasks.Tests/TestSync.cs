using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NinjaSync;
using NinjaSync.MasterSlave;
using NinjaSync.Model;
using NinjaSync.P2P;
using NinjaSync.P2P.Serializing;
using NinjaSync.Storage;

using NinjaTasks.Core;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTasks.Sync.TaskWarrior;
using NinjaTools;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Logging;
using NUnit.Framework;
using TaskWarriorLib.Network;

#if !DOT42
using NinjaTasks.App.Wpf.Services.TcpIp;
#else
using TslConnectionFactory = NinjaTasks.App.Droid.Services.Tls.AndroidTslConnectionFactory;
#endif


namespace NinjaTasks.Tests
{
#if DOT42
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    class TestCaseAttribute : Attribute
    {
        public TestCaseAttribute(params object[] args)
        {
        }
    }
#endif

    class MirrorSetup : IDisposable
    {
        public ITodoStorage Storage1;
        public ISyncService Sync1;
        public ITodoStorage Storage2;
        public ISyncService Sync2;

        public IDisposable[] Factories;
        public TokenBag Keep;

        public void Dispose()
        {
            if (Factories != null)
                foreach (var f in Factories)
                    f.Dispose();
            if (Keep != null)
                Keep.Dispose();
        }
    }
    
    public  class TestSync
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        [Test]
        [Ignore("TaskWarrior integration testing.")]
        public void TwIntegrationTestMirror()
        {
            var conFac = Helpers.CreateConnectionFactory();
            var localFac = new SQLiteFactory(conFac, @"test-local.sqlite");
            Helpers.ClearDatabase(localFac);

            var syncFac = new MvxSqliteSyncServiceFactory(new TslConnectionFactory());
            var sync = syncFac.CreateTaskWarriorSync(Helpers.Account, localFac.Get("tw"));

            // sync.
            sync.Sync(new NullSyncProgress());
        }
       [TestCase("RemoteToLocalToTw")]
       [TestCase("LocalToTw")]
       [TestCase("LocalToTwWithIdMap")]
       [Ignore("TaskWarrior integration testing.")]
        public void TwIntegrationTestSyncToAndFroSync(string type)
       {
           TestSyncToAndFroSync(type);
       }

       /// <summary>
       /// TODO: test intermediate changes
       /// </summary>
       [TestCase("LocalToLocal")]
       [TestCase("LocalToLocalWithIdMaps")]
       [TestCase("LocalToLocalNoIdMapOn2")]
       [TestCase("LocalToLocalWithImplicitLists")]
       [TestCase("LocalToLocalWithIdMappingAndImplicitLists")]
       [TestCase("LocalToP2P_Direct")]
       [TestCase("LocalToP2P_Pipes")]
       public void TestSyncToAndFroSync(string type)
        {
            // set up two databases: two "remotes"; then sync them with task-warrior.

            using (var s = SetupMirrors(type))
            {
                // make sure we have an empty database to start with.
                Assert.AreEqual(0, s.Storage1.GetLists().Count());
                Assert.AreEqual(0, s.Storage1.GetTasks().Count());
                Assert.AreEqual(0, s.Storage2.GetLists().Count());
                Assert.AreEqual(0, s.Storage2.GetTasks().Count());

                s.Sync1.Sync(new NullSyncProgress());
                s.Sync2.Sync(new NullSyncProgress());

                Assert.AreEqual(s.Storage1.GetLists().Count(), s.Storage2.GetLists().Count());
                Assert.AreEqual(s.Storage1.GetTasks().Count(), s.Storage2.GetTasks().Count());

                // create a new tasks on remote1, sync it, expect it on remote2.
                var task1 = new TodoTask { Description = "My Test Task " + Guid.NewGuid() };
                var list1 = new TodoList { Description = "My Test List " + Guid.NewGuid() };
                s.Storage1.Save(list1);
                task1.ListFk = list1.Id;
                s.Storage1.Save(task1);

                s.Sync1.Sync(new NullSyncProgress());
                s.Sync2.Sync(new NullSyncProgress());

                var task2 = s.Storage2.GetTasks().FirstOrDefault(t => t.Description == task1.Description);
                Assert.IsNotNull(task2);
                var list2 = s.Storage2.GetLists(task2.ListFk).FirstOrDefault();
                Assert.IsNotNull(list2);
                Assert.AreEqual(list1.Description, list2.Description);

                // sync again...
                s.Sync1.Sync(new NullSyncProgress());
                s.Sync2.Sync(new NullSyncProgress());

                task2 = s.Storage2.GetTasks().FirstOrDefault(t => t.Description == task1.Description);
                Assert.IsNotNull(task2);
                list2 = s.Storage2.GetLists(task2.ListFk).FirstOrDefault();
                Assert.IsNotNull(list2);
                Assert.AreEqual(list1.Description, list2.Description);

                // modify the task on storage2, and wait for change to propagate.
                task2.Status = Status.Completed;
                s.Storage2.Save(task2);

                s.Sync2.Sync(new NullSyncProgress());
                s.Sync1.Sync(new NullSyncProgress());

                task1 = s.Storage1.GetTasks(ids: task1.Id).First();
                Assert.AreEqual(Status.Completed, task1.Status);

                // now modify both tasks at the same time, but different properties,
                // and check.
                list1 = new TodoList() { Description = "My Test List 2 " + Guid.NewGuid() };
                s.Storage1.Save(list1);
                task1.ListFk = list1.Id;
                s.Storage1.Save(task1);

                task2.Status = Status.Pending;
                s.Storage2.Save(task2);

                s.Sync1.Sync(new NullSyncProgress());
                s.Sync2.Sync(new NullSyncProgress());
                s.Sync1.Sync(new NullSyncProgress());

                task1 = s.Storage1.GetTasks(ids: task1.Id).First();
                task2 = s.Storage2.GetTasks(ids: task2.Id).First();
                Assert.AreEqual(task1.Status, task2.Status);
                list2 = s.Storage2.GetLists(task2.ListFk).FirstOrDefault();
                Assert.IsNotNull(list2);
                Assert.AreEqual(list1.Description, list2.Description);

                // now delete task2 and let it propagate.
                // modify task1 locally. and make sure, it stays deleted.
                s.Storage2.DeleteTask(task2);
                task1.Description += " Modified";
                s.Storage1.Save(task1);

                s.Sync2.Sync(new NullSyncProgress());
                s.Sync1.Sync(new NullSyncProgress());

                Assert.IsNull(s.Storage1.GetTasks(ids: task1.Id).FirstOrDefault());
            }
        }

       [TestCase("RemoteToLocalToTw")]
       [TestCase("RemoteToLocalToTwFail2At2")]
       [TestCase("RemoteToLocalToTwFail2At1")]
       [TestCase("LocalToTw")]
       [TestCase("LocalToTwWithIdMap")]
       [Ignore("TaskWarrior integration testing.")]
       public void TwIntegrationTestFirstTimeSyncUploadLocals(string type)
       {
           TestFirstTimeSyncUploadLocals(type);
       }


       [TestCase("LocalToLocal")]
       [TestCase("LocalToLocalWithIdMaps")]
       [TestCase("LocalToLocalNoIdMapOn2")]
       [TestCase("LocalToLocalWithImplicitLists")]
       [TestCase("LocalToLocalWithIdMappingAndImplicitLists")]
       [TestCase("LocalToP2P_Direct")]
       [TestCase("LocalToP2P_Pipes")]
       public void TestFirstTimeSyncUploadLocals(string type)
       {
           // set up two databases: two "remotes"; then sync them with task-warrior.

           using (var s = SetupMirrors(type))
           {
               var list1 = new TodoList { Description = "My Test List " + Guid.NewGuid() };
               s.Storage1.Save(list1);
               var task1 = new TodoTask { Description = "My Test Task " + Guid.NewGuid() };
               task1.ListFk = list1.Id;
               s.Storage1.Save(task1);

               s.Sync1.Sync(new NullSyncProgress());

               // make sure it did'nt get deleted.
               task1 = s.Storage1.GetTasks(ids: task1.Id).FirstOrDefault();
               Assert.IsNotNull(task1);

               // now create a task on 2, sync it, check it does'nt get get deleted.
               var list2 = new TodoList { Description = "My Test List 2 " + Guid.NewGuid() };
               s.Storage2.Save(list2);
               var task2 = new TodoTask { Description = "My Test Task 2 " + Guid.NewGuid() };
               task2.ListFk = list2.Id;
               s.Storage2.Save(task2);

               // allow for simulation of failure, with retry.
               int count = 0;
               while (++count < 3)
               {
                   try
                   {
                       s.Sync2.Sync(new NullSyncProgress());
                       break;
                   }
                   catch (Exception ex)
                   {
                       Log.Error(ex);
                   }
               }
               // make sure it did'nt get deleted.
               task2 = s.Storage2.GetTasks(ids: task2.Id).FirstOrDefault();
               Assert.IsNotNull(task2);


               // check task 1 got transferred.
               var task1_2 = s.Storage2.GetTasks().FirstOrDefault(t=>t.Description == task1.Description);
               Assert.IsNotNull(task1_2);

               // check storage 1 again after sync.
               s.Sync1.Sync(new NullSyncProgress());
               task1 = s.Storage1.GetTasks(ids: task1.Id).FirstOrDefault();
               Assert.IsNotNull(task1);
               var task2_1 = s.Storage1.GetTasks().FirstOrDefault(t => t.Description == task2.Description);
               Assert.IsNotNull(task2_1);

               // clean up.
               foreach(var l in s.Storage1.GetLists().Where(p=>p.Description !=null && p.Description.StartsWith("My Test List")))
                   s.Storage1.DeleteList(l);
               s.Sync1.Sync(new NullSyncProgress());

           }
       }

       [TestCase("RemoteToLocalToTw")]
       [TestCase("LocalToTw")]
       [TestCase("LocalToTwWithIdMap")]
       [Ignore("TaskWarrior integration testing.")]
       public void TwIntegrationTestMergeConflictsByModificationDate(string type)
       {
           TestMergeConflictsByModificationDate(type);
       }

       [TestCase("LocalToLocal")]
       [TestCase("LocalToLocalWithIdMaps")]
       [TestCase("LocalToLocalNoIdMapOn2")]
       [TestCase("LocalToLocalWithImplicitLists")]
       [TestCase("LocalToLocalWithIdMappingAndImplicitLists")]
       [TestCase("LocalToP2P_Direct")]
       [TestCase("LocalToP2P_Pipes")]
       public void TestMergeConflictsByModificationDate(string type)
       {
           // set up two databases: two "remotes"; then sync them with task-warrior.

           using (var s = SetupMirrors(type))
           {
               var list1 = new TodoList { Description = "My Test List " + Guid.NewGuid() };
               s.Storage1.Save(list1);
               var task1 = new TodoTask { Description = "My Test Task " + Guid.NewGuid() };
               task1.ListFk = list1.Id;
               s.Storage1.Save(task1);

               s.Sync1.Sync(new NullSyncProgress());
               s.Sync2.Sync(new NullSyncProgress());

               var task2 = s.Storage2.GetTasks().FirstOrDefault(t => t.Description == task1.Description);
               Assert.IsNotNull(task2);
               // as a side note, make sure the task still exists.
               task1 = s.Storage1.GetTasks().FirstOrDefault(t => t.Description == task1.Description);
               Assert.IsNotNull(task1);
               // now conflict-modify sync2 "after" sync1, but sync "before"
               // this should result in sync1's changes beeing overwritten,
               // even though he syncs lalter.


               string newDescTask1 = "My Test Task Mod1 " + Guid.NewGuid();
               task1.Description = newDescTask1;
               task1.ModifiedAt = task1.ModifiedAt + TimeSpan.FromSeconds(5);// change occured before that on sync2.
               s.Storage1.Save(task1);
               
               string newDescTask2 = "My Test Task Mod2 " + Guid.NewGuid();
               task2.Description = newDescTask2;
               task2.ModifiedAt = task2.ModifiedAt + TimeSpan.FromSeconds(10);
               s.Storage2.Save(task2);

               // sync 2 first.
               s.Sync2.Sync(new NullSyncProgress());
               s.Sync1.Sync(new NullSyncProgress());

               // make sure our changes on sync2 were overwritten, but not those on sync1
               task1 = s.Storage1.GetTasks(ids: task1.Id).FirstOrDefault();
               Assert.IsNotNull(task1);
               Assert.AreEqual(newDescTask2, task1.Description);

               s.Sync2.Sync(new NullSyncProgress());
               task2 = s.Storage2.GetTasks(ids: task2.Id).FirstOrDefault();
               Assert.IsNotNull(task2);
               Assert.AreEqual(newDescTask2, task2.Description);

               // clean up.
               s.Storage2.DeleteTask(task2);
               s.Sync2.Sync(new NullSyncProgress());

           }
       }

       [TestCase("RemoteToLocalToTw")]
       [TestCase("LocalToTw")]
       [TestCase("LocalToTwWithIdMap")]
       [Ignore("TaskWarrior integration testing.")]
       public void TwIntegrationTestOnlyUploadChangesAfterNewSync(string type)
       {
           TestOnlyUploadChangesAfterNewSync(type);
       }

       [TestCase("LocalToLocal")]
       [TestCase("LocalToLocalWithIdMaps")]
       [TestCase("LocalToLocalNoIdMapOn2")]
       [TestCase("LocalToLocalWithImplicitLists")]
       [TestCase("LocalToLocalWithIdMappingAndImplicitLists")]
       [TestCase("LocalToP2P_Direct")]
       [TestCase("LocalToP2P_Pipes")]
       public void TestOnlyUploadChangesAfterNewSync(string type)
       {
           using (var s = SetupMirrors(type))
           {
               var list1 = new TodoList { Description = "My Test List " + Guid.NewGuid() };
               s.Storage1.Save(list1);
               var task1 = new TodoTask { Description = "My Test Task " + Guid.NewGuid() };
               task1.ListFk = list1.Id;
               s.Storage1.Save(task1);

               s.Sync1.Sync(new NullSyncProgress());
               s.Sync2.Sync(new NullSyncProgress());

               // make a modification on 2
               var task2 = s.Storage2.GetTasks().FirstOrDefault(t => t.Description == task1.Description);
               Assert.IsNotNull(task2);
               task2.Description = "My Test Task Mod2 " + Guid.NewGuid();
               s.Storage2.Save(task2);

               var p = new NullSyncProgress();

               s.Sync2.Sync(p);

               Assert.AreEqual(1, p.LocalModified, "this is known to  fail for ID mapping w/o list-mapping atm, " +
                                                   "since a new list gets assigned to an intermediate changes commit, " +
                                                   "instead of the correct commit. Should not be serious, only has the " +
                                                   "implication that the newly created lists will be send as modified over the " +
                                                   "wire next time.");
               Assert.AreEqual(0, p.LocalDeleted);
               Assert.AreEqual(0, p.RemoteDeleted);
               Assert.AreEqual(0, p.RemoteModified);

               // don't make any modifications.
               p = new NullSyncProgress();
               s.Sync1.Sync(p);

               Assert.AreEqual(0, p.LocalModified);
               Assert.AreEqual(0, p.LocalDeleted);
               Assert.AreEqual(0, p.RemoteDeleted);

               // on p2p, everything has already been synchronized with s.Sync2.Sync()!
               if(!type.Contains("P2P"))
                    Assert.AreEqual(1, p.RemoteModified);
           }
       }

       [TestCase("RemoteToLocalToTw")]
       [TestCase("LocalToTw")]
       [TestCase("LocalToTwWithIdMap")]
       [Ignore("TaskWarrior integration testing.")]
       public void TwIntegrationProtectIntermediateChanges(string type)
       {
           TestOnlyUploadChangesAfterNewSync(type);
       }

        [TestCase("LocalToLocal")]
        [TestCase("LocalToLocalWithIdMaps")]
        [TestCase("LocalToLocalNoIdMapOn2")]
        [TestCase("LocalToLocalWithImplicitLists")]
        [TestCase("LocalToLocalWithIdMappingAndImplicitLists")]
        public void TestProtectIntermediateChanges(string type)
        {
            using (var s = SetupMirrors(type))
            {
                var list1 = new TodoList { Description = "My Test List " + Guid.NewGuid() };
                s.Storage1.Save(list1);
                var task1 = new TodoTask { Description = "My Test Task " + Guid.NewGuid() };
                task1.ListFk = list1.Id;
                s.Storage1.Save(task1);

                s.Sync1.Sync(new NullSyncProgress());
                s.Sync2.Sync(new NullSyncProgress());

                // make a modification on 2
                var task2 = s.Storage2.GetTasks().FirstOrDefault(t => t.Description == task1.Description);
                Assert.IsNotNull(task2);
                task2.Description = "My Test Task Mod2 " + Guid.NewGuid();
                s.Storage2.Save(task2);
                s.Sync2.Sync(new NullSyncProgress());

                ISyncWithMasterService syncWithMaster = (ISyncWithMasterService)s.Sync1;
                string intermediateDesc = "My Intermediate Description " + Guid.NewGuid();

                syncWithMaster.RemoteModificationsRetrieved += (sender, e) =>
                {
                    task1.Description = intermediateDesc;
                    s.Storage1.Save(task1);
                };

                s.Sync1.Sync(new NullSyncProgress());

                task1 = s.Storage1.GetTasks(ids: task1.Id).First();
                Assert.AreEqual(intermediateDesc, task1.Description);

                s.Sync1.Sync(new NullSyncProgress());

                task1 = s.Storage1.GetTasks(ids: task1.Id).First();
                Assert.AreEqual(intermediateDesc, task1.Description);

                s.Sync2.Sync(new NullSyncProgress());

                task2 = s.Storage2.GetTasks(ids: task2.Id).First();
                Assert.AreEqual(intermediateDesc, task2.Description);
            }
        }

        [Test]
        public void TestCommitListDeserialize()
        {
            TestCommitListReflection();
            string json = @"{""DeletionCount"":0,""ModificationCount"":0,""BasedOnCommitId"":null,""StorageId"":""9d2c5034-9b42-4731-97c6-7a2d41900849"",""Commits"":[{""CommitId"":null,""Changes"":[]}]}""";
            var list = new JsonNetModificationSerializer(new TodoTrackableFactory()).Deserialize(new StringReader(json));
            Assert.AreEqual(0, list.DeletionCount);
            Assert.IsNull(list.BasedOnCommitId);
            Assert.AreEqual("9d2c5034-9b42-4731-97c6-7a2d41900849", list.StorageId);
            Assert.IsNotNull(list.Commits);
            Assert.AreEqual(1, list.Commits.Count);
        }

        [Test]
        public void TestCommitListReflection()
        {

            //var prop = typeof (JsonCommitList).GetProperty("Commits");
            //Console.WriteLine(prop.PropertyType.FullName);
        }

       private MirrorSetup SetupMirrors(string type)
        {
           if (type == "LocalToTw")
               return MirrorSetupFactory.LocalToTw("test-remote1", "test-remote2");
           if (type == "LocalToTwWithIdMap")
               return MirrorSetupFactory.LocalToTw("test-remote1", "test-remote2", true);
           if (type == "LocalToLocalWithIdMaps")
               return MirrorSetupFactory.LocalToLocal("test-remote1", "test-remote2", true, true, false, false);
           if (type == "LocalToLocal")
               return MirrorSetupFactory.LocalToLocal("test-remote1", "test-remote2", false, false, false, false);
           if (type == "LocalToLocalWithImplicitLists")
               return MirrorSetupFactory.LocalToLocal("test-remote1", "test-remote2", false, false, true, true);
           if (type == "LocalToLocalWithIdMappingAndImplicitLists")
               return MirrorSetupFactory.LocalToLocal("test-remote1", "test-remote2", true, true, true, true);
           if (type == "LocalToLocalNoIdMapOn2")
               return MirrorSetupFactory.LocalToLocal("test-remote1", "test-remote2", true, false, false, false);
           if (type == "RemoteToLocalToTw")
               return MirrorSetupFactory.RemoteToLocalToTw("test-remote1", "test-remote2");
           if (type == "RemoteToLocalToTwFail2At2")
               return MirrorSetupFactory.RemoteToLocalToTw("test-remote1", "test-remote2", 2);
           if (type == "RemoteToLocalToTwFail2At1")
               return MirrorSetupFactory.RemoteToLocalToTw("test-remote1", "test-remote2", 1);
           if (type == "LocalToP2P_Pipes")
               return MirrorSetupFactory.LocalToP2P_Pipes("test-remote1", "test-remote2");
           if (type == "LocalToP2P_Direct")
               return MirrorSetupFactory.LocalToP2P_Direct("test-remote1", "test-remote2");

           throw new NotImplementedException(type);
        }




    }
}
