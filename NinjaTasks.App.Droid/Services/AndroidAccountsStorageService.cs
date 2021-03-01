using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Android.Accounts;
using Android.Content;
using Android.OS;
using Java.Lang;
using Newtonsoft.Json;
using NinjaTasks.App.Droid.RemoteStorages.NonsenseApps;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools;
using Exception = System.Exception;

namespace NinjaTasks.App.Droid.Services
{
    public class AndroidAccountsStorageService : IAccountsStorage, ITaskWarriorAccountsStorage
    {
        private const string AccountType = "org.ninjatasks.account";
        private const string Authority = "org.ninjatasks.provider";

        private const string Version = "VERSION 1.0";
        
        private readonly Context _ctx;
        private readonly IPropertyCopier _copier = new SimplePropertyCopier();

        public AndroidAccountsStorageService(Context ctx)
        {
            _ctx = ctx;
        }

        internal class MyAccount : Tuple<SyncAccount, TaskWarriorAccount, Account>
        {
            public MyAccount(SyncAccount item1, TaskWarriorAccount item2, Account item3) : base(item1, item2, item3)
            {
            }
        }

        public IEnumerable<TaskWarriorAccount> GetTaskWarriorAccounts()
        {
            return ListAccounts().Select(p => p.Item2)
                                 .Where(p => p != null);
        }

        public IEnumerable<SyncAccount> GetAccounts()
        {
            return ListAccounts().Select(p => p.Item1);
        }

        internal IEnumerable<MyAccount> ListAccounts()
        {
            AccountManager accountManager = AccountManager.Get(_ctx);
            foreach (var a in accountManager.GetAccountsByType(AccountType))
            {
                SyncAccount syncAccount;
                TaskWarriorAccount twAccount;
                try
                {
                    FromAccount(accountManager, a, out syncAccount, out twAccount);
                }
                catch (Exception) { continue; }// skip.

                yield return new MyAccount(syncAccount, twAccount, a);
            }
        }


        public void Delete(SyncAccount account)
        {
            var accounts = ListAccounts().ToList();
            var prev = accounts.FirstOrDefault(a => a.Item1.Id == account.Id);
            if (prev == null) return;

            AccountManager accountManager = AccountManager.Get(_ctx);

            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.LollipopMr1)
                accountManager.RemoveAccount(prev.Item3, null, null, null);
            else
#pragma warning disable CS0618 // Type or member is obsolete
                accountManager.RemoveAccount(prev.Item3, null, null);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void SaveAccount(SyncAccount account, IList<string> selectedProperties)
        {
            SaveAccount(null, account, selectedProperties);
        }

        private void SaveAccount(TaskWarriorAccount twAccount, SyncAccount account, IList<string> selectedProperties)
        {
            AccountManager accountManager = AccountManager.Get(_ctx);
            var accounts = ListAccounts().ToList();

            var prev = accounts.FirstOrDefault(a => a.Item1.Id == account.Id);
            
            if (prev != null)
            {
                try
                {
                    if (selectedProperties != null)
                    {
                        _copier.Copy(prev.Item1, account, selectedProperties);
                        account = prev.Item1;
                    }
                    accountManager.SetUserData(prev.Item3, "sync", JsonConvert.SerializeObject(account));
                    if(twAccount != null)
                        accountManager.SetUserData(prev.Item3, "taskwarrior", JsonConvert.SerializeObject(twAccount));
                    return;
                }
                catch (Java.Lang.SecurityException)
                {
                }
            }

            // creation of new account.
            // automatically create a NotePad account if none exists.
            CreateNotepadAccountIfNotExists(account, ref accounts);


            account.Id = accounts.Select(p => p.Item1.Id).DefaultIfEmpty().Max() + 1;

            Bundle userData = new Bundle();
            userData.PutString("VERSION", Version);
            userData.PutString("sync", JsonConvert.SerializeObject(account));
            if (twAccount != null)
            {
                twAccount.Id = account.Id;
                userData.PutString("taskwarrior", JsonConvert.SerializeObject(twAccount));
            }


            Account n = new Account(account.Name+ " (" + account.Address + ")", AccountType);
            accountManager.AddAccountExplicitly(n, "(dummy password)", userData);

            SetAccountSyncSettings(n, account);

        }

        private void CreateNotepadAccountIfNotExists(SyncAccount account, ref List<MyAccount> accounts)
        {
            if (account.Type == SyncAccountType.NonsenseAppsNotePad ||
                        accounts.Any(p => p.Item1.Type == SyncAccountType.NonsenseAppsNotePad)) 
                return;

            SyncAccount a = new SyncAccount
            {
                Name = NpContract.Authority,
                Type = SyncAccountType.NonsenseAppsNotePad,
                Address = "app",
                IsSyncOnDataChanged = true
            };
            SaveAccount(null, a, null);

            accounts = ListAccounts().ToList();
        }

        private void SetAccountSyncSettings(Account account, SyncAccount syncAccount)
        {
            if (syncAccount.Type == SyncAccountType.NonsenseAppsNotePad)
            {
                ContentResolver.SetIsSyncable(account, NpContract.Authority, 1);
                ContentResolver.SetSyncAutomatically(account, NpContract.Authority, true);
                //ContentResolver.AddPeriodicSync(account, NpContract.Authority, new Bundle(), 60*10);
            }
            else
            {
                ContentResolver.SetIsSyncable(account, Authority, 1);
                ContentResolver.SetSyncAutomatically(account, Authority, true);
                ContentResolver.AddPeriodicSync(account, NpContract.Authority, new Bundle(), 60 * 10);
            }

            //ContentResolver.SetIsSyncable(a, TasksContract.Authority, 1);
            //ContentResolver.SetSyncAutomatically(a, TasksContract.Authority, true);
            //ContentResolver.AddPeriodicSync(a, TasksContract.Authority, new Bundle(), 60 * 10);

            //ContentResolver.SetIsSyncable(a, Authority, 1);
            //ContentResolver.SetSyncAutomatically(a, Authority, false);
        }

        public SyncAccount SaveAccount(TaskWarriorAccount account)
        {
            var accounts = ListAccounts().ToList();
            var prev = accounts.FirstOrDefault(a => a.Item1.Id == account.Id);

            if (prev != null)
            {
                prev.Item1.IsManualSyncOnly = true;
                SaveAccount(account, prev.Item1, null);
                return prev.Item1;
            }
            else
            {
                SyncAccount s = new SyncAccount();
                s.Name = "Taskwarrior";
                s.Type = SyncAccountType.TaskWarrior;
                s.Address = account.ServerHostname;
                // TaskWarrior syncs are initiated by the Android System.
                s.IsManualSyncOnly = true; 

                SaveAccount(account, s, null);
                return s;
            }
        }

        public static void FromAccount(AccountManager accountManager, Account account, out SyncAccount syncAccount, out TaskWarriorAccount twAccount)
        {
            syncAccount = null;
            twAccount = null;

            string version = accountManager.GetUserData(account, "VERSION");
            if (version != Version) return;

            string json = accountManager.GetUserData(account, "sync");
            
            if(json.IsNullOrEmpty())
                throw new NullPointerException();

            syncAccount = JsonConvert.DeserializeObject<SyncAccount>(json);

            twAccount = null;

            try
            {
                json = accountManager.GetUserData(account, "taskwarrior");
                if (!json.IsNullOrEmpty())
                    twAccount = JsonConvert.DeserializeObject<TaskWarriorAccount>(json);
            }
            catch (Exception)
            {
            }
        }

        public Account ToAndroidAccount(SyncAccount account)
        {
            return ListAccounts().Where(p => p.Item1.Id == account.Id)
                                 .Select(p => p.Item3)
                                 .FirstOrDefault();
        }
    }
}
