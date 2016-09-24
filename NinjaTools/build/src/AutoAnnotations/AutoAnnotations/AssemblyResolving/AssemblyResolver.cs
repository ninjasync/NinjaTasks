using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Mono.Cecil;

namespace AutoAnnotations.AssemblyResolving
{
    public class AssemblyResolver : IAssemblyResolver
    {
        public string ProjectDirectory { get; private set; }
        public string FrameworkMainAssembly { get; private set; }
        List<string> gacPaths;
        List<string> directories;

        public  AssemblyResolver(string targetPath, string projectPath, IList<string> additionalPaths = null)
        {
            ProjectDirectory = Path.GetDirectoryName(projectPath);

            var versionReader = new ProjectFileVersionReader(projectPath);
            directories = new List<string>();
            
            if(additionalPaths != null)
                directories.AddRange(additionalPaths);

            if (versionReader.IsSilverlight)
            {
                if (string.IsNullOrEmpty(versionReader.TargetFrameworkProfile))
                {
                    directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\Silverlight\{1}\", GetProgramFilesPath(), versionReader.FrameworkVersionAsString));
                }
                else
                {
                    directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\Silverlight\{1}\Profile\{2}", GetProgramFilesPath(), versionReader.FrameworkVersionAsString, versionReader.TargetFrameworkProfile));
                }
                FrameworkMainAssembly = "mscorlib";
            }
            if (versionReader.IsDot42)
            {
                FindDot42ReferenceAssemblies(versionReader);
                FrameworkMainAssembly = "dot42";
            }
            else
            {
                if (string.IsNullOrEmpty(versionReader.TargetFrameworkProfile))
                {
                    if (versionReader.FrameworkVersionAsNumber == 3.5m)
                    {
                        directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\v3.5\", GetProgramFilesPath()));
                        directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\v3.0\", GetProgramFilesPath()));
                        directories.Add(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Microsoft.NET\Framework\v2.0.50727\"));
                    }
                    else
                    {
                        directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\.NETFramework\{1}\", GetProgramFilesPath(), versionReader.FrameworkVersionAsString));
                    }
                }
                else
                {
                    directories.Add(string.Format(@"{0}\Reference Assemblies\Microsoft\Framework\.NETFramework\{1}\Profile\{2}", GetProgramFilesPath(), versionReader.FrameworkVersionAsString, versionReader.TargetFrameworkProfile));
                }
                FrameworkMainAssembly = "mscorlib";
            }
            directories.Add(Path.GetDirectoryName(targetPath));

            GetGacPaths();

        }

        private void FindDot42ReferenceAssemblies(ProjectFileVersionReader versionReader)
        {
            try
            {
                using (RegistryKey keyuser = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\dot42\Android"))
                using (RegistryKey keymachine = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\dot42\Android"))
                {
                    string assemblyDir = null;
                    if (keyuser != null)
                    {
                        assemblyDir = (keyuser.GetValue("ReferenceAssemblies") ?? "").ToString();
                    }
                    if (string.IsNullOrEmpty(assemblyDir) && keymachine != null)
                    {
                        assemblyDir = (keymachine.GetValue("ReferenceAssemblies") ?? "").ToString();
                    }

                    if (!string.IsNullOrEmpty(assemblyDir))
                        directories.Add(assemblyDir + "\\" + versionReader.FrameworkVersionAsString);
                    else
                    {
                        Console.Error.WriteLine(
                            "warning: unable to find dot42 reference assembly directory from registry.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    "warning: unable to find dot42 reference assembly directory from registry.");
            }
        }

        void GetGacPaths()
        {
            gacPaths = GetDefaultWindowsGacPaths().ToList();
        }

        IEnumerable<string> GetDefaultWindowsGacPaths()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("WINDIR");
            if (environmentVariable != null)
            {
                yield return Path.Combine(environmentVariable, "assembly");
                yield return Path.Combine(environmentVariable, Path.Combine("Microsoft.NET", "assembly"));
            }
        }

        public string GetProgramFilesPath()
        {
            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (programFiles == null)
            {
                return Environment.GetEnvironmentVariable("ProgramFiles");
            }

            return programFiles;
        }


        string SearchDirectory(string name)
        {
            foreach (var directory in directories)
            {
                var dllFile = Path.Combine(directory, name + ".dll");
                if (File.Exists(dllFile))
                {
                    return dllFile;
                }
                var exeFile = Path.Combine(directory, name + ".exe");
                if (File.Exists(exeFile))
                {
                    return exeFile;
                }
            }
            return null;
        }


        public string Find(AssemblyNameReference assemblyNameReference)
        {
            var file = SearchDirectory(assemblyNameReference.Name);
            if (file == null)
            {
                file = GetAssemblyInGac(assemblyNameReference);
            }
            if (file != null)
            {
                return file;
            }
            throw new FileNotFoundException();
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return AssemblyDefinition.ReadAssembly(Find(name));
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return AssemblyDefinition.ReadAssembly(Find(name));
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return AssemblyDefinition.ReadAssembly(Find(fullName));
        }


        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            return AssemblyDefinition.ReadAssembly(Find(fullName));
        }
        public string Find(string assemblyName)
        {
            var file = SearchDirectory(assemblyName);
            if (file != null)
            {
                return file;
            }
            throw new FileNotFoundException();
        }

        string GetAssemblyInGac(AssemblyNameReference reference)
        {
            if ((reference.PublicKeyToken == null) || (reference.PublicKeyToken.Length == 0))
            {
                return null;
            }
            return GetAssemblyInNetGac(reference);
        }

        string GetAssemblyInNetGac(AssemblyNameReference reference)
        {
            var gacs = new[] { "GAC_MSIL", "GAC_32", "GAC" };
            var prefixes = new[] { string.Empty, "v4.0_" };

            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < gacs.Length; j++)
                {
                    var gac = Path.Combine(gacPaths[i], gacs[j]);
                    var file = GetAssemblyFile(reference, prefixes[i], gac);
                    if (Directory.Exists(gac) && File.Exists(file))
                    {
                        return file;
                    }
                }
            }

            return null;
        }


        static string GetAssemblyFile(AssemblyNameReference reference, string prefix, string gac)
        {
            var builder = new StringBuilder();
            builder.Append(prefix);
            builder.Append(reference.Version);
            builder.Append("__");
            for (var i = 0; i < reference.PublicKeyToken.Length; i++)
            {
                builder.Append(reference.PublicKeyToken[i].ToString("x2"));
            }
            return Path.Combine(Path.Combine(Path.Combine(gac, reference.Name), builder.ToString()), reference.Name + ".dll");
        }



    }
}