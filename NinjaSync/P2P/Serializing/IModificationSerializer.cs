using System.IO;
using NinjaSync.Model.Journal;

namespace NinjaSync.P2P.Serializing
{
    public interface IModificationSerializer
    {
        T Deserialize<T>(TextReader reader);
        CommitList Deserialize(TextReader reader);

        void Serialize(TextWriter write, object obj);
        void Serialize(TextWriter writer, CommitList list);
        
    }
}