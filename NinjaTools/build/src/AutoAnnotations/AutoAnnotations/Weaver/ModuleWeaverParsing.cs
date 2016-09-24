using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;

partial class ModuleWeaver
{
    private const string RexApplyToType = @"Apply +to +type +([^ ]+) *(?:[:]|when +(extends|inherits)\('([^ ')]+)'\) *:)";
    private const string RexApplyToMember = @"Apply +to +member +([^ ]+) *(?:[:]|when +(public) *:)";
    private const string RexDefinition = @"([A-Za-z][A-Za-z0-9]*(?:\.[A-Za-z][A-Za-z0-9]*)*)";

    private static readonly string RexExpression = string.Format("^ *(?:{0})? *(?:{1})? *{2}(?: *, *{2})* *$", RexApplyToType,
                                                                RexApplyToMember, RexDefinition);
    private readonly Regex _parser = new Regex(RexExpression);

    private List<Matcher> ParseMatchersFromAssemblyAttributes()
    {
        List<Matcher> matchers = new List<Matcher>();
        return matchers;

        foreach (var attr in ModuleDefinition.Assembly.CustomAttributes
            .Where(a => a.AttributeType.FullName == "System.Reflection.ObfuscationAttribute")
            )
        {
            CustomAttributeNamedArgument featArg = attr.Properties.FirstOrDefault(f => f.Name == "Feature");
            if (string.IsNullOrEmpty(featArg.Name) || featArg.Argument.Value == null)
            {
                LogWarning("feature string not set.");
                continue;
            }

            string feature = featArg.Argument.Value.ToString();
            LogInfo("found obfuscation: " + feature);

            CustomAttributeNamedArgument excludeArg = attr.Properties.FirstOrDefault(f => f.Name == "Exclude");
            bool exclude = excludeArg.Name == null || (bool)excludeArg.Argument.Value;
            if (!exclude)
            {
                LogInfo("should not be excluded. skipping.");
                continue;
            }

            CustomAttributeNamedArgument membersArg = attr.Properties.FirstOrDefault(f => f.Name == "ApplyToMembers");
            bool applyToMembers = membersArg.Name != null && (bool)membersArg.Argument.Value;

            var match = ParseMatcher(feature, applyToMembers);
            if(match != null)
                matchers.Add(match);
        }
        return matchers;
    }

    private List<Matcher> ParseMatchersFromFiles()
    {
        List<Matcher> ret = new List<Matcher>();

        string path = ProjectDirectory;
        foreach (var f in Directory.GetFiles(path ?? ".", "*.autoannotations"))
        {
            LogInfo("parsing " + f);
            foreach (var line in File.ReadAllLines(f))
            {
                string l = line.Trim();
                if (l.Length == 0 || l.StartsWith("#") || l.StartsWith(";"))
                    continue;
                var match = ParseMatcher(l, false);
                if(match != null)
                    ret.Add(match);
            }
        }
        return ret;
    }

    private Matcher ParseMatcher(string feature, bool applyToMembers)
    {
        Match m = _parser.Match(feature);
        if (!m.Success)
        {
            LogWarning("parsing error: " + feature);
            return null;
        }

        Matcher match = new Matcher();
        if (m.Groups[1].Success) match.TypeMatcher = m.Groups[1].Value;
        if (m.Groups[2].Success) match.TypeInheritanceKeyword = m.Groups[2].Value;
        if (m.Groups[3].Success) match.TypeInheritanceName = m.Groups[3].Value;
        if (m.Groups[4].Success) match.MemberMatcher = m.Groups[4].Value;
        if (m.Groups[5].Success) match.MembersPublicOnly = true;
        if (m.Groups[6].Success) match.Attributes.Add(m.Groups[6].Value);
        if (m.Groups[7].Success)
            foreach (Capture cap in m.Groups[7].Captures)
                match.Attributes.Add(cap.Value);

        match.ApplyToMembers = applyToMembers;

        if (match.TypeMatcher == null && match.MemberMatcher == null)
        {
            LogWarning("either type or member matcher should be specified: " + feature);
            return match;
        }
        return match;
    }
}
