using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

partial class ModuleWeaver
{
     private Dictionary<string, Annotation> GetAnnotationList(List<Matcher> matchers)
    {
        Dictionary<string, Annotation> annotations = new Dictionary<string, Annotation>();
        var types = ModuleDefinition.Types.Where(x => x.IsClass || x.IsEnum || x.IsInterface || x.IsValueType);
        GetAnnotationList(matchers, types, annotations);
        return annotations;
    }

    private void GetAnnotationList(List<Matcher> matchers, IEnumerable<TypeDefinition> types, Dictionary<string, Annotation> annotations)
    {
        foreach (var type in types)
        {
            if (type.CustomAttributes.Any(t => t.AttributeType.Name == "CompilerGeneratedAttribute"))
            {
                LogInfo("skipping compiler generated " + type.FullName);
                continue;
            }


            foreach (var matcher in matchers)
            {
                if (!Matches(matcher, type)) continue;
                LogInfo(string.Format("{0}: {1}", type.FullName, matcher));

                Annotation a;
                if (!annotations.TryGetValue(type.FullName, out a))
                    annotations[type.FullName] = a = new Annotation {Type = type};

                if (matcher.MemberMatcher == null)
                    foreach (var an in matcher.Attributes)
                        AddAnnoation(a, an);

                if (!matcher.ApplyToMembers && matcher.MemberMatcher == null)
                    continue;

                ApplyToMembers(annotations, type, matcher);
            }

            // handle nested types.
            GetAnnotationList(matchers, type.NestedTypes, annotations);
        }
    }

    private void ApplyToMembers(Dictionary<string, Annotation> annotations, TypeDefinition type, Matcher matcher)
    {
        foreach (var member in type.Methods)
        {
            if (matcher.MembersPublicOnly && !member.IsPublic)
                continue;
            // DO ANNOTATE INDIVIDUAL GETTERS OR SETTERS. E.G. Caliburn.Micro depends on it.
            //if (member.IsGetter || member.IsSetter) continue; // do not annotate individual setters
            CheckMemberAnnotations(matcher, member, annotations);
        }
        foreach (var member in type.Fields)
        {
            if (matcher.MembersPublicOnly && !member.IsPublic)
                continue;
            CheckMemberAnnotations(matcher, member, annotations);
        }
        foreach (var member in type.Properties)
        {
            if (matcher.MembersPublicOnly && (member.GetMethod == null || !member.GetMethod.IsPublic))
                continue;
            CheckMemberAnnotations(matcher, member, annotations);
        }
        foreach (var member in type.Events)
        {
            if (matcher.MembersPublicOnly && (member.AddMethod == null || !member.AddMethod.IsPublic))
                continue;
            CheckMemberAnnotations(matcher, member, annotations);
        }
    }

    private void CheckMemberAnnotations(Matcher matcher, IMemberDefinition member, Dictionary<string, Annotation> annotations)
    {
        Annotation a;
        if (matcher.MemberMatcher != null && !MatchesWildcard(matcher.MemberMatcher, member.Name))
            return;

        if (!annotations.TryGetValue(member.FullName, out a))
        {
            annotations[member.FullName] = a = new Annotation {Member = member};
            foreach (var an in matcher.Attributes)
            {
                AddAnnoation(a, an);
            }
                
        }
    }

    private static void AddAnnoation(Annotation a, string an)
    {
        if (!an.EndsWith("Attribute"))
            a.Attributes.Add(an + "Attribute");
        else
            a.Attributes.Add(an);
    }
}

