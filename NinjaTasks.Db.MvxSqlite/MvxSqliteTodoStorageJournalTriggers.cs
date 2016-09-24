using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using NinjaSync.Model;
using NinjaSync.Model.Journal;
using NinjaSync.Storage.MvxSqlite;

namespace NinjaTasks.Db.MvxSqlite
{
    [SuppressMessage("dot42", "StaticFieldInGenericType")]
    public class MvxSqliteTodoStorageJournalTriggers<TTrackable>
                            where TTrackable : ITrackable, new()
    {
        public void CreateTriggers(ISQLiteConnection connection,
                                   string tablePrefix,
                                   bool createTriggersForAdditionalProperties = false)
        {
            string journalTable = tablePrefix + connection.GetMapping<JournalEntry>().TableName;
            string trackableTableName = tablePrefix + connection.GetMapping<TTrackable>().TableName;
            string additionalPropertiesTableName = createTriggersForAdditionalProperties ? (trackableTableName + "Properties") : null;

            connection.EnsureTableCreated<JournalEntry>(journalTable);

            TrackableType trackableType = new TTrackable().TrackableType;

            var trackedColumns = typeof(TTrackable).GetRuntimeProperties()
                                           .Where(prop => prop.GetCustomAttributes<Track>().Any())
                                           .Select(prop => prop.Name)
                                           .ToArray();

            var cmds = GetTriggerStatements(journalTable, trackableTableName, (int)trackableType, trackedColumns, additionalPropertiesTableName);

            connection.RunInTransaction(() =>
            {
                // SqliteNet only executes the first of a list of statements.
                // so better give them one by one.
                foreach (var cmd in cmds)
                    connection.Execute(cmd);
            });
        }

        private IList<string> GetTriggerStatements(string journalTable, string trackableTableName, int trackableType, IEnumerable<string> trackedColumns, string additionalPropertiesTableName)
        {
            List<string> triggerNames = new List<string>();
            List<string> triggers = new List<string>();

            CreateStandardTriggers(journalTable, trackableTableName, trackableType, trackedColumns, triggers, triggerNames);

            if (additionalPropertiesTableName != null)
                CreateAdditionalPropertiesTriggers(journalTable, trackableTableName, trackableType,
                                                   additionalPropertiesTableName, triggers, triggerNames);


            List<string> cmds = new List<string>();
            foreach (var triggerName in triggerNames)
                cmds.Add("DROP TRIGGER IF EXISTS " + triggerName);
            cmds.AddRange(triggers);

            return cmds;
        }

        private static void CreateAdditionalPropertiesTriggers(string journalTable, string trackableTableName, int trackableType,
                                                               string additionalPropertiesTableName, List<string> triggers, List<string> triggerNames)
        {
            // AdditionalProperties table works with INSERT OR REPLACE, so no update needed.
            string cmd, name;
            name = additionalPropertiesTableName + "_journal_insert";
            cmd =
                string.Format(
                    "CREATE TRIGGER {0} " +
                    " AFTER INSERT ON  {1} \n" +
                    " BEGIN INSERT OR REPLACE INTO {2} (Timestamp, Change, Type, ObjectId, Member) \n" +
                    "   Values ((SELECT ModifiedAt FROM {3} WHERE Id=new.Id), {4}, {5}, new.Id, new.Member); END;",
                    name, additionalPropertiesTableName, journalTable, trackableTableName,
                    (int)ChangeType.Modified, trackableType.ToString(CultureInfo.InvariantCulture));
            triggers.Add(cmd);
            triggerNames.Add(name);

            name = additionalPropertiesTableName + "_journal_modified";
            cmd =
                string.Format(
                   "CREATE TRIGGER {0}\n" +
                        "   AFTER UPDATE ON {1} \n" +
                        "   WHEN old.Value IS NOT new.Value \n" +
                        "   BEGIN \n" +
                        "   INSERT OR REPLACE INTO {2} (Timestamp, Change, Type, ObjectId, Member) \n" +
                        "       Values ((SELECT ModifiedAt FROM {3} WHERE Id=new.Id), {4}, {5}, new.Id, new.Member);\n" +
                        "   END;",
                    name, additionalPropertiesTableName, journalTable, trackableTableName,
                    (int)ChangeType.Modified, trackableType.ToString(CultureInfo.InvariantCulture));
            triggers.Add(cmd);
            triggerNames.Add(name);
        }

        private static void CreateStandardTriggers(string journalTable, string trackableTableName, int trackableType,
                                                   IEnumerable<string> trackedColumns, List<string> triggers, List<string> triggerNames)
        {
            // insert.
            string cmd, name;
            name = trackableTableName + "_journal_insert";
            cmd =
                string.Format(
                    "CREATE TRIGGER {0} " +
                    " AFTER INSERT ON  {1} \n" +
                    " BEGIN INSERT INTO {2} (Timestamp, Change, Type, ObjectId, Member) \n" +
                    "   Values (new.ModifiedAt, {3}, {4}, new.Id, ''); END;",
                    name, trackableTableName, journalTable,
                    (int)ChangeType.Created, trackableType.ToString(CultureInfo.InvariantCulture));
            triggers.Add(cmd);
            triggerNames.Add(name);

            // delete: NOTE: modifiedAt is not correcly updated 
            name = trackableTableName + "_journal_delete";
            cmd =
                string.Format(
                    "CREATE TRIGGER {0}\n" +
                    "   AFTER DELETE ON  {1}\n" +
                    "   BEGIN\n" +
                    "       INSERT INTO {2} (Timestamp, Change, Type, ObjectId, Member)\n" +
                    "           Values (old.ModifiedAt, {3}, {4}, old.Id, '');\n" +
                    "   END;",
                    name, trackableTableName, journalTable,
                    (int)ChangeType.Deleted, trackableType.ToString(CultureInfo.InvariantCulture));
            triggers.Add(cmd);
            triggerNames.Add(name);

            foreach (string col in trackedColumns)
            {
                name = trackableTableName + "_journal_update_" + col;
                cmd =
                    string.Format(
                        "CREATE TRIGGER {0}\n" +
                        "   AFTER UPDATE OF {5} ON {1} \n" +
                        "   WHEN old.{5} IS NOT new.{5} \n" +
                        "   BEGIN \n" +
                        "   INSERT OR REPLACE INTO {2} (Timestamp, Change, Type, ObjectId, Member) \n" +
                        "       Values (new.ModifiedAt, {3}, {4}, new.Id, '{5}');\n" +
                        "   END;",
                        name, trackableTableName, journalTable,
                        (int)ChangeType.Modified, trackableType.ToString(CultureInfo.InvariantCulture),
                        col);
                triggers.Add(cmd);
                triggerNames.Add(name);
            }
        }
    }
}
