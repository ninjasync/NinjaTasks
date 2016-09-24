using System;
using NinjaSync.Storage;

namespace NinjaTasks.Model.Storage
{
    public class TodoSyncStorages : SyncStorages
    {
        public ITodoStorage Todo { get; set; }
    }
}