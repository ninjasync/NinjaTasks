using System;

namespace NinjaSync.MasterSlave
{
    public interface ISyncWithMasterService : ISyncService
    {
        event EventHandler RemoteModificationsRetrieved;
    }
}