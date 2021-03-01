using Android.Net;

namespace NinjaTasks.App.Droid.RemoteStorages.org.Tasks
{
    public static class TasksContract
    {
        public static readonly string[] ColumnsTask = new[]
        {
            ColId, ColTaskTitle, ColTaskImportance, ColTaskCompleted,
            ColTaskCreated, ColTaskModified, ColTaskDeleted, ColTaskRemoteId
            //ColTaskDueDate, ColTaskHideUntil,ColTaskNotes
        };

        public static readonly string[] ColumnsMetadata = new[]
        {
            ColId, ColMetadataTask, ColMetadataKey,ColMetadataValue,ColMetadataValue2,ColMetadataValue3
        };

        public static readonly Uri UriMetadata = Uri.Parse(Scheme + Authority + "/" + TableMetadata);
        public static readonly Uri UriTask = Uri.Parse(Scheme + Authority + "/" + TableTask);
        public static readonly Uri UriTagdata = Uri.Parse(Scheme + Authority + "/" + TableTagdata);
        

        public const string Authority = "org.tasks";
        public const string Scheme = "content://";

        public const string ColId = "_id";

        public const string ColTaskTitle = "title";
        public const string ColTaskImportance = "importance";
        public const string ColTaskDueDate = "dueDate";
        public const string ColTaskCompleted = "completed";
        public const string ColTaskCreated = "created";
        public const string ColTaskModified = "modified";
        public const string ColTaskDeleted = "deleted";
        public const string ColTaskHideUntil = "hideUntil";
        public const string ColTaskNotes = "notes";
        public const string ColTaskRemoteId = "remoteId";


        public const string ColMetadataTask = "task";
        public const string ColMetadataKey = "key";
        public const string ColMetadataValue = "value";
        public const string ColMetadataValue2 = "value2";
        public const string ColMetadataValue3 = "value3";
        public const string ColMetadataDeleted = "deleted";

        public const string ColTagdataName = "name";
        public const string ColTagdataUuid = "remote_id";


        public const string TableTask = "tasks";
        public const string TableMetadata = "metadata";
        public const string TableTagdata = "tagdata";

        public enum Importance
        {
            DoOrDie = 0,
            MustDo = 1,
            ShouldDo = 2,
            None = 3
        }
    }
}