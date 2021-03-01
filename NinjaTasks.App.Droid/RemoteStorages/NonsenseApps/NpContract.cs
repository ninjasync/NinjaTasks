using Android.Net;

namespace NinjaTasks.App.Droid.RemoteStorages.NonsenseApps
{
    public static class NpContract
    {
        public static readonly string[] ColumnsTask = new[]
        {
            ColId, ColTaskTitle, /*ColTaskNote,*/ ColTaskDblist, ColTaskCompleted, /*ColTaskDue, */
            ColTaskUpdated, /*ColTaskLocked,*/ ColTaskLeft, ColTaskRight
        };

        public static readonly string[] ColumnsTaskList = new[]
        {
            ColId, ColListTitle, ColListUpdated, /*ColListListType,ColListSorting*/
        };

        public static readonly Uri UriTaskList = Uri.Parse(Scheme + Authority + "/" + TableTaskList);
        public static readonly Uri UriTask = Uri.Parse(Scheme + Authority + "/" + TableTask);
        public static readonly Uri UriRemoteTaskList = Uri.Parse(Scheme + Authority + "/" + TableRemoteTaskList);
        public static readonly Uri UriRemoteTask = Uri.Parse(Scheme + Authority + "/" + TableRemoteTask);

        public const string Authority = "com.nononsenseapps.NotePad";
        public const string Scheme = "content://";

        public const string ColId = "_id";

        public const string ColTaskTitle = "title";
        public const string ColTaskDblist = "dblist";
        public const string ColTaskCompleted = "completed";
        public const string ColTaskUpdated = "updated";
        public const string ColTaskLeft = "lft";
        public const string ColTaskRight = "rgt";

        public const string ColTaskNote = "note";
        public const string ColTaskDue = "due";
        public const string ColTaskLocked = "locked";


        public const string ColListTitle = "title";
        public const string ColListUpdated = "updated";
        public const string ColListSorting = "sorting";
        private const string ColListListType = "tasktype";

        public const string ColRemoteAccount = "account";
        public const string ColRemoteService = "service";
        public const string ColRemoteDbId = "dbid";
        public const string ColRemoteUpdated = "updated";
        public const string ColRemoteRemoteId = "remoteid";
        public const string ColRemoteField1 = "field1";
        public const string ColRemoteField2 = "field2";
        public const string ColRemoteField3 = "field3";
        public const string ColRemoteField4 = "field4";
        public const string ColRemoteField5 = "field5";

        public const string ColRemoteTask_ListDbId = "listdbid";

        public const string TableTask = "task";
        public const string TableTaskList = "tasklist";

        public const string TableRemoteTask = "remotetask";
        public const string TableRemoteTaskList = "remotetasklist";
    }
}