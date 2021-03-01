using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTasks.Model.Sync;
using NinjaTools;

#if !DOT42
using System.Linq.Expressions;
#endif

namespace NinjaTasks.Model.Storage
{
    public interface IAccountsStorage
    {
        IEnumerable<SyncAccount> GetAccounts();
        
        void SaveAccount(SyncAccount account, IList<string> selectedProperties=null);
        void Delete(SyncAccount account);
    }

    public static class AccountsStorageExtensions
    {
        public static void SaveAccount(this IAccountsStorage storage, SyncAccount account)
        {
            storage.SaveAccount(account);
        }

        public static void SaveAccount(this IAccountsStorage storage, SyncAccount account, params string[] selectedProperties)
        {
            storage.SaveAccount(account, selectedProperties);
        }

#if!DOT42
        [Obsolete("use the SaveAccount invocation with nameof()")]
        public static void SaveAccount(this IAccountsStorage storage, SyncAccount account,
                                       params Expression<Func<SyncAccount, object>>[] selectedProperties)
        {
            storage.SaveAccount(account, selectedProperties.Select(ExpressionHelper.GetMemberName).ToList());
        }
#endif

    }

    public interface ITaskWarriorAccountsStorage
    {
        IEnumerable<TaskWarriorAccount> GetTaskWarriorAccounts();
        SyncAccount SaveAccount(TaskWarriorAccount account);
    }
}
