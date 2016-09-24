using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using CustomAttributeNamedArgument = Mono.Cecil.CustomAttributeNamedArgument;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

public partial class ModuleWeaver
{
    // Will log an informational message to MSBuild
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }
    public string FrameworkMainAssembly { get; set; }
    public string ProjectDirectory { get; set; }
    public bool Force { get; set; }

    TypeSystem typeSystem;

    // Init logging delegates to make testing easier
    public ModuleWeaver()
    {
        LogInfo = Console.WriteLine;
        LogWarning = Console.WriteLine;
    }

    

    public bool Execute()
    {
        //Console.WriteLine(_parser.ToString());
        typeSystem = ModuleDefinition.TypeSystem;

        var matchers = ParseMatchersFromAssemblyAttributes();
        matchers.AddRange(ParseMatchersFromFiles());

        foreach (var m1 in matchers)
            LogInfo(m1.ToString());

        if (matchers.Count == 0)
        {
            LogInfo("nothing to do.");
            Console.Error.WriteLine("Warning: no matching rules found.");
            return false;
        }

        var existingHash = GetPreviousHashcodeFromAssembly();
        var hash = GetMatchersHashCode(matchers);

        if (existingHash == hash && !Force)
        {
            LogInfo("nothing to do (1).");
            Console.WriteLine("Info: assembly was already processed with AutoAnnotations, and no changes to matching rules found. bailing out.");
            return false;
        }

        var annotations = GetAnnotationList(matchers);

        foreach (var anno in annotations.OrderBy(a => a.Key))
            LogInfo("should be annotated: " + anno.Key + ": " + string.Join(", ", anno.Value.Attributes.Select(a=>a.Replace("Attribute", ""))));

        var ass = ModuleDefinition.AssemblyResolver.Resolve(FrameworkMainAssembly);
        attributeType = ass.MainModule.Types.First(t => t.FullName == "System.Attribute");
        attributeUsageType = ass.MainModule.Types.First(t => t.FullName == "System.AttributeUsageAttribute");
        attributeTargetsType = ass.MainModule.Types.First(t => t.FullName == "System.AttributeTargets");
        

        ModuleDefinition.Import(attributeType);
        ModuleDefinition.Import(attributeUsageType);
        
        foreach (var anno in annotations.Values)
        {
            if (anno.Type != null)
                AddAnnotations(anno.Type, anno.Attributes);
            else
                AddAnnotations(anno.Member, anno.Attributes);
        }

        SetHashcodeAssemblyAttribute(hash);

        return true;
    }

    private string GetMatchersHashCode(List<Matcher> matchers)
    {
        StringBuilder concat = new StringBuilder();
        foreach (var x in matchers)
        {
            concat.Append(x.ApplyToMembers);
            concat.Append(string.Join(",",x.Attributes));
            concat.Append(x.MemberMatcher);
            concat.Append(x.MembersPublicOnly);
            concat.Append(x.TypeInheritanceKeyword);
            concat.Append(x.TypeInheritanceName);
            concat.Append(x.TypeMatcher);
            concat.Append("\n");
        }

        return GetSHA1HashData(concat.ToString());
    }

    /// <summary>
    /// take any string and encrypt it using SHA1 then
    /// return the encrypted data
    /// </summary>
    /// <param name="data">input text you will enterd to encrypt it</param>
    /// <returns>return the encrypted text as hexadecimal string</returns>
    private string GetSHA1HashData(string data)
    {
        //create new instance of md5
        SHA1 sha1 = SHA1.Create();

        //convert the input text to array of bytes
        byte[] hashData = sha1.ComputeHash(Encoding.Default.GetBytes(data));

        //create new instance of StringBuilder to save hashed data
        StringBuilder returnValue = new StringBuilder();

        //loop for each byte and add it to StringBuilder
        for (int i = 0; i < hashData.Length; i++)
        {
            returnValue.Append(hashData[i].ToString());
        }

        // return hexadecimal string
        return returnValue.ToString();
    }

    private string GetPreviousHashcodeFromAssembly()
    {
        foreach (var attr in ModuleDefinition.Assembly.CustomAttributes.Where(a => a.AttributeType.FullName.StartsWith("ProcessedWithAutoAnnotations_")))
            return attr.AttributeType.FullName.Split('_')[1];
        return null;
    }

    private void SetHashcodeAssemblyAttribute(string hash)
    {
        // remove previous attribute
        foreach (var attr in ModuleDefinition.Assembly.CustomAttributes.Where(a => a.AttributeType.FullName.StartsWith("ProcessedWithAutoAnnotations_")).ToList())
            ModuleDefinition.Assembly.CustomAttributes.Remove(attr);

        TypeDefinition annoType = GetOrCreateAnnotationType("ProcessedWithAutoAnnotations_" + hash);
        ModuleDefinition.Assembly.CustomAttributes.Add(new CustomAttribute(annoType.GetConstructors().First()));
    }

    private
        TypeDefinition attributeType;
    private TypeDefinition attributeUsageType;
    Dictionary<string, TypeDefinition> _annotationTypes = new Dictionary<string, TypeDefinition>();
    private TypeDefinition attributeTargetsType;

    private void AddAnnotations(IMemberDefinition member, ISet<string> attrs)
    {
        foreach (var annoTypeFullName in attrs.Except(member.CustomAttributes.Select(x => x.AttributeType.FullName)))
        {
            TypeDefinition annoType;

            annoType = GetOrCreateAnnotationType(annoTypeFullName);

            LogInfo("annotating " + annoTypeFullName + " on " + member.FullName);
            var constructor = ModuleDefinition.Import(annoType.GetConstructors().First());
            
            member.CustomAttributes.Add(new CustomAttribute(constructor));
        }
    }

    private TypeDefinition GetOrCreateAnnotationType(string annoTypeFullName)
    {
        TypeDefinition annoType;
        if (!_annotationTypes.TryGetValue(annoTypeFullName, out annoType))
        {
            annoType = ModuleDefinition.Types.FirstOrDefault(t => t.FullName == annoTypeFullName);
            if (annoType == null)
            {
                // search in framework main assembly.
                var assembly = ModuleDefinition.AssemblyResolver.Resolve(FrameworkMainAssembly);
                annoType = assembly.MainModule.Types.FirstOrDefault(t => t.FullName == annoTypeFullName);
            }
            if (annoType == null)
                annoType = CreateAnnotationType(annoTypeFullName);
            _annotationTypes.Add(annoTypeFullName, annoType);
        }
        return annoType;
    }

    private TypeDefinition CreateAnnotationType(string typeFullName)
    {
        LogInfo("creating type " + typeFullName);

        string nameSpace=null, typeName=typeFullName;
        int lastPoint = typeFullName.LastIndexOf('.');
        if (lastPoint != -1)
        {
            nameSpace = typeFullName.Substring(0, lastPoint);
            typeName = typeFullName.Substring(lastPoint + 1);
        }

        TypeDefinition annoType;
        annoType = new TypeDefinition(nameSpace, typeName, TypeAttributes.NotPublic|TypeAttributes.Class|TypeAttributes.AnsiClass|TypeAttributes.BeforeFieldInit, 
                                                ModuleDefinition.Import(attributeType));
        var method = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, typeSystem.Void);
        var baseConstructor = ModuleDefinition.Import(attributeType.GetConstructors().First(x=>x.Parameters.Count == 0));
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Call, baseConstructor);
        processor.Emit(OpCodes.Ret);
        annoType.Methods.Add(method);

        // [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
        var attributeUsageConstructor = ModuleDefinition.Import(attributeUsageType.GetConstructors().First(p => !p.Parameters.Any()));
        var usageAttr = new CustomAttribute(attributeUsageConstructor);
        usageAttr.Properties.Add(new CustomAttributeNamedArgument("Inherited", new CustomAttributeArgument(typeSystem.Boolean, false)));
        usageAttr.Properties.Add(new CustomAttributeNamedArgument("AllowMultiple", new CustomAttributeArgument(typeSystem.Boolean, false)));
        
        annoType.CustomAttributes.Add(usageAttr);


        ModuleDefinition.Types.Add(annoType);
        return annoType;
    }
}