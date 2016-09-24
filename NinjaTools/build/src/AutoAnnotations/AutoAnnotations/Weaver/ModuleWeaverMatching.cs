using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Cecil;

partial class ModuleWeaver
{
    private bool Matches(Matcher matcher, TypeDefinition type)
    {
        
        bool matchesTypeName = matcher.TypeMatcher == null || MatchesWildcard(matcher.TypeMatcher, type.FullName);
        if (!matchesTypeName) return false;

        if (matcher.TypeInheritanceName != null)
            return HierarchyImplementsInterface(type, matcher.TypeInheritanceName);

        return true;
    }

    private readonly Dictionary<string, Regex> _matchRex = new Dictionary<string, Regex>();
    private bool MatchesWildcard(string wildcardExpression, string name)
    {
        Regex reg;
        if (!_matchRex.TryGetValue(wildcardExpression, out reg))
        {
            var rex = Regex.Escape(wildcardExpression.Replace("*", "§1").Replace("?", "§2"))
                .Replace("§1", ".*").Replace("§2", ".?");
            reg = new Regex("^" + rex + "$", RegexOptions.Compiled | RegexOptions.Singleline);
            _matchRex[wildcardExpression] = reg;
        }
        return reg.IsMatch(name);
    }
}
