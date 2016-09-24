using System.Collections.Generic;
using Mono.Cecil;

public class Annotation
{
    public string TypeName 
    { 
        get
        {
            return Type != null ? Type.FullName : Member != null ? Member.DeclaringType.FullName : null;
        } 
    }
    public string FullName
    {
        get
        {
            return Type != null ? Type.FullName : Member != null ? Member.FullName : null;
        }
    }

    public TypeDefinition Type { get; set; }
    public IMemberDefinition Member { get; set; }
    public ISet<string> Attributes { get; set; }

    public Annotation()
    {
        Attributes = new HashSet<string>();
    }
}