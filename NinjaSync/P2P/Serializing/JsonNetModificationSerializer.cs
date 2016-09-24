using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NinjaSync.Model.Journal;
using NinjaTools;

namespace NinjaSync.P2P.Serializing
{
    public class JsonNetModificationSerializer : IModificationSerializer
    {
        private readonly ITrackableFactory _objects;
        private static JsonSerializerSettings _jsonSettings;
        private readonly JsonSerializer _serializer;

        public JsonNetModificationSerializer(ITrackableFactory objects)
        {
            _objects = objects;
            _serializer = JsonSerializer.Create(JsonSettings);
        }

        public CommitList Deserialize(TextReader reader)
        {
            // do not dispose!
            // TODO: get this to work without having to read things into lines.
            var line = reader.ReadLine();
            //var read = new JsonTextReader(reader) {CloseInput = false};
            var jsonCommitLists = _serializer.Deserialize<JsonCommitList>(new JsonTextReader(new StringReader(line)));
            return ToCommitList(jsonCommitLists);
        }

        public T Deserialize<T>(TextReader reader)
        {
            // do not dispose!
            //var read = new JsonTextReader(reader) { CloseInput = false };
            var line = reader.ReadLine();
            return _serializer.Deserialize<T>(new JsonTextReader(new StringReader(line)));
        }

        public void Serialize(TextWriter write, object obj)
        {
            var jsonwrit = new JsonTextWriter(write) { CloseOutput = false };
            _serializer.Serialize(jsonwrit, obj);
            jsonwrit.Flush();
            write.WriteLine();
        }

        public void Serialize(TextWriter writer,CommitList list)
        {
            var commitList = ToJsonCommitList(list);
            Serialize(writer, commitList);
        }


        private CommitList ToCommitList(JsonCommitList jsonCommitList)
        {
            Debug.Assert(!jsonCommitList.StorageId.IsNullOrEmpty());

            CommitList ret = new CommitList
            {
                StorageId = jsonCommitList.StorageId
            };

            string basedOnCommitId = jsonCommitList.BasedOnCommitId;

            foreach (var jsonCommit in jsonCommitList.Commits)
            {
                var commit = new Commit
                {
                    CommitId = jsonCommit.CommitId,
                    BasedOnCommitId = basedOnCommitId,
                    BasedOnCommitId2 = jsonCommit.BasedOnCommitId2
                };

                basedOnCommitId = jsonCommit.CommitId;

                if (jsonCommit.Changes == null)
                {
                    commit = commit.ToPlaceholder();
                }
                else
                {
                    foreach (var c in jsonCommit.Changes)
                    {
                        Modification mod = ToModification(c);
                        if (mod.IsDeletion)
                            commit.Deleted.Add(mod);
                        else
                            commit.Modified.Add(mod);
                    }
                }

                ret.Commits.Add(commit);
            }
            return ret;
        }

        private JsonCommitList ToJsonCommitList(CommitList list)
        {
            var commitList = new JsonCommitList
            {
                BasedOnCommitId = list.BasedOnCommitId, 
                StorageId = list.StorageId
            };

            Debug.Assert(!commitList.StorageId.IsNullOrEmpty());

            foreach (var commit in list.Commits)
            {
                var jsonCommit = new JsonCommit
                {
                    CommitId = commit.CommitId,
                    BasedOnCommitId2 = commit.BasedOnCommitId2
                };

                if (!commit.IsPlaceholder)
                {
                    commitList.DeletionCount += commit.Deleted.Count;
                    commitList.ModificationCount = commit.Modified.Count;

                    jsonCommit.Changes = new List<JsonModification>();

                    foreach (var mod in commit.Deleted)
                        jsonCommit.Changes.Add(ToJsonModification(mod));

                    foreach (var mod in commit.Modified)
                        jsonCommit.Changes.Add(ToJsonModification(mod));
                }

                commitList.Commits.Add(jsonCommit);
            }
            return commitList;
        }

        private Modification ToModification(JsonModification change)
        {
            if (change.Change == ChangeType.Deletion)
            {
                Debug.Assert(change.Type != null);

                var delAt = change.DeletedAt ?? DateTime.UtcNow;
                Modification ret = new Modification(new TrackableId(change.Type.Value, change.Id), delAt);
                return ret;
            }
            if (change.Change == ChangeType.Full)
            {
                ITrackable obj = null;
                foreach (var type in _objects.Types)
                {
                    var typeName = _objects.GetName(type);
                    if (change.AdditionalData.ContainsKey(typeName))
                    {
                        var json = (JToken)change.AdditionalData[typeName];
                        obj = (ITrackable) json.ToObject(_objects.GetType(type), _serializer);
                    }
                }

                if(obj == null)
                    throw new Exception("object not delivered or recognized.");
                return new Modification(obj);
            }
            if (change.Change == ChangeType.Update)
            {
                Debug.Assert(change.Type != null);
                ITrackable obj = _objects.Create(change.Type.Value);
                obj.Id = change.Id;

                List<ModifiedProperty> props = new List<ModifiedProperty>();

                foreach (var mod in change.Modifications)
                {
                    props.Add(new ModifiedProperty(mod.Property, mod.ModifiedAt));
                    var property = obj.GetType().GetRuntimeProperty(mod.Property);

                    
                    if(mod.Value==null)
                        property.SetValue(obj, null);
                    if (mod.Value is JToken)
                    {
                        var jsonVal = (JToken)mod.Value;
                        var val = jsonVal.ToObject(property.PropertyType, _serializer);
                        property.SetValue(obj, val);
                    }
                    else if(mod.Value != null)
                    {
                        // there is probalby an easyer way, though i don't know how. 
                        // what what should i do?
                        var jsonVal = new JObject();
                        jsonVal.Add(mod.Property, new JValue(mod.Value));
                        JsonConvert.PopulateObject(jsonVal.ToString(), obj, JsonSettings);
                    }
                    
                }
                return new Modification(obj, props);
            }

            throw new Exception("unrecognized change type.");
        }


        public JsonModification ToJsonModification(Modification mod)
        {
            JsonModification obj =new JsonModification();
            if (mod.IsDeletion)
            {
                obj.Change = ChangeType.Deletion;
                obj.Type = mod.Key.Type;
                obj.Id = mod.Key.ObjectId;
            }
            else if (mod.ModifiedProperties == null)
            {
                obj.Change = ChangeType.Full;
                string typeName = _objects.GetName(mod.Object.TrackableType);

                obj.AdditionalData.Add(typeName, mod.Object);
            }
            else
            {
                obj.Change = ChangeType.Update;
                obj.Type = mod.Object.TrackableType;
                obj.Id = mod.Object.Id;
                obj.Modifications = new List<Mod>();
                foreach (var p in mod.ModifiedPropertiesEx)
                {
                    Mod m = new Mod();
                    m.Property = p.Property;
                    m.ModifiedAt = p.ModifiedAt;
                    m.Value = GetPropValue(mod.Object, p.Property);
                    obj.Modifications.Add(m);
                }
            }

            return obj;
        }

        private static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetRuntimeProperty(propName).GetValue(src, null);
        }

        public JsonSerializerSettings JsonSettings
        {
            get
            {
                if (_jsonSettings == null)
                {
                    var settings = new JsonSerializerSettings();
                    settings.NullValueHandling = NullValueHandling.Include;

                    var datetimeconv = new IsoDateTimeConverter
                    {
                        Culture = CultureInfo.InvariantCulture,
                        DateTimeFormat = "yyyyMMdd'T'HHmmss'Z'"
                    };
                    settings.Converters.Add(datetimeconv);
                    settings.Converters.Add(new StringEnumConverter());
                    //settings.ContractResolver = new LowercaseContractResolver();

                    _jsonSettings = settings;
                }
                return _jsonSettings;
            }
            set { _jsonSettings = value; }
        }
    }

}
