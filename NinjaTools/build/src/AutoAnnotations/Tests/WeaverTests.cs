using System;
using System.IO;
using System.Reflection;
using AutoAnnotations;
using NUnit.Framework;

[TestFixture]
public class WeaverTests
{
    Assembly assembly;
    string newAssemblyPath;
    string assemblyPath;

    //[TestFixtureSetUp]
    public void Setup()
    {
        var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
        assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif

        var options = new Options {Input = assemblyPath, Output = newAssemblyPath, ProjectFile = projectPath, Verbose = true};
        Program.RunWeaver(options);
        assembly = Assembly.LoadFile(newAssemblyPath);
    }

    [Test]
    public void EmptyTest()
    {
        Setup();
    }
    [Test]
    public void ValidateHelloWorldIsInjected()
    {
        Setup();
        var type = assembly.GetType("Hello");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("Hello World", instance.World());
    }

#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Setup();
        Verifier.Verify(assemblyPath,newAssemblyPath);
    }
#endif
}