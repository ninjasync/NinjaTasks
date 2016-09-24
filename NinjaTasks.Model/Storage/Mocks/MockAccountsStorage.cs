using System;
using System.Collections.Generic;
using NinjaTasks.Model.Sync;

namespace NinjaTasks.Model.Storage.Mocks
{
    public class MockAccountsStorage : IAccountsStorage
    {
        public IEnumerable<SyncAccount> GetAccounts()
        {
            return new[]
            {
                   new SyncAccount { Id=1, Name="Bluetooth P2P", Type = SyncAccountType.BluetoothP2P, Address = "33:33:33:11:11:11"},
                   new SyncAccount { Id=2, Name="Taskwarrior", Type = SyncAccountType.TaskWarrior}
            };
        }

        public void SaveAccount(SyncAccount account, IList<string> selectedProperties = null)
        {
        }


        public void Delete(SyncAccount account)
        {
            throw new NotImplementedException();
        }
    }
}
