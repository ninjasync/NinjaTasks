using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

partial class ModuleWeaver
{
    Dictionary<string, bool> typeReferencesImplementingInterface = new Dictionary<string, bool>();
    public bool HierarchyImplementsInterface(TypeReference typeReference, string interfaceName)
    {
        bool implementsINotify;
        var fullName = typeReference.FullName;
        var key = fullName + "|" + interfaceName;
        if (typeReferencesImplementingInterface.TryGetValue(key, out implementsINotify))
            return implementsINotify;

        TypeDefinition typeDefinition;
        if (typeReference.IsDefinition)
        {
            typeDefinition = (TypeDefinition)typeReference;
        }
        else
        {
            typeDefinition = Resolve(typeReference);
        }

        if (typeDefinition.Name == interfaceName
         || typeDefinition.Interfaces.Any(i => i.Name == interfaceName)
            /*|| typeDefinition.NestedTypes.Any(t => t.Name == interfaceName)*/)
        {
            typeReferencesImplementingInterface[key] = true;
            return true;
        }

        var baseType = typeDefinition.BaseType;
        if (baseType == null)
        {
            typeReferencesImplementingInterface[fullName] = false;
            return false;
        }
        return HierarchyImplementsInterface(baseType, interfaceName);
    }

    Dictionary<string, TypeDefinition> definitions = new Dictionary<string, TypeDefinition>();
    public TypeDefinition Resolve(TypeReference reference)
    {
        TypeDefinition definition;
        if (definitions.TryGetValue(reference.FullName, out definition))
            return definition;
        return definitions[reference.FullName] = InnerResolve(reference);
    }

    private TypeDefinition InnerResolve(TypeReference reference)
    {
        try
        {
            return reference.Resolve();
        }
        catch (Exception exception)
        {
            //throw new Exception(string.Format("Could not resolve '{0}'.", reference.FullName), exception);    
        }
        try
        {
            LogWarning("manually resolving " + reference.FullName + " in " + reference.Scope.Name);
            var assembly = ModuleDefinition.AssemblyResolver.Resolve((AssemblyNameReference)reference.Scope);
            return assembly.MainModule.GetType(reference.FullName);
            //return reference.Resolve();
        }
        catch (Exception exception)
        {
            throw new Exception(string.Format("Could not resolve '{0}'.", reference.FullName), exception);
        }
    }
}
