using System.Collections.Generic;
using NinjaSync.Model.Journal;
using NinjaTools;

namespace NinjaTasks.Model
{
    public sealed class TodoTaskWithListName : TodoTask
    {
        /// <summary>
        /// don't add listname to the registered properties.
        /// </summary>
        public string ListName { get; set; }

        public TodoTaskWithListName()
        {
        }

        public TodoTaskWithListName(TodoTask task, string listName)
        {
            CopyFrom(task);
            ListName = listName;
        }

#if !DOT42
        public static readonly string ColListName = ExpressionHelper.GetMemberName<TodoTaskWithListName>(x => x.ListName);
#else
        public static readonly string ColListName = "ListName";
#endif

        public override ITrackable Clone()
        {
            var ret = (TodoTaskWithListName)base.Clone();
            ret.ListName = ListName;
            return ret;
        }

        static TodoTaskWithListName()
        {
            SetupProperties(typeof(TodoTaskWithListName), AllProperties);
        }
    }
}
