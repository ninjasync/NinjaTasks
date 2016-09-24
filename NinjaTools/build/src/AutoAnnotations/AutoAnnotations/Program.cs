using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoAnnotations.AssemblyResolving;
using CommandLine;
using CommandLine.Text;
using Mono.Cecil;
using Mono.Cecil.Pdb;

namespace AutoAnnotations
{
    public class Options
    {
        [Option("i", "input", HelpText = "input assembly file", Required = true)]
        public string Input { get; set; }

        [Option("p", "project", HelpText = "project file, to read framework configuration", Required = true)]
        public string ProjectFile { get; set; }

        [Option("o", "output", HelpText = "output assembly file. if note set, overwrite input assembly.")]
        public string Output { get; set; }

        [Option("f", "force", HelpText = "force processing, even when AutoAnnotation believes it has already processed the assembly, and not changed to the rules are detected.")]
        public bool Force { get; set; }

        [OptionArray("l", "libs", HelpText = "where to look for referenced assemblies.", Required = false)]
        public string[] LibraryPaths { get; set; }

        [Option("h", "help", HelpText = "show help")]
        public bool ShowHelp { get; set; }

        [Option("v", "verbose", HelpText = "be verbose?")]
        public bool Verbose { get; set; }
    }

    public class Program
    {
        
        [STAThread]
        static int Main(string[] args)
        {
            Options opt = new Options();
            if (!CommandLineParser.Default.ParseArguments(args, opt) || opt.ShowHelp)
            {
                ShowHelp();
                return 1;
            }

            try
            {
                RunWeaver(opt);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 2;
            }
            return 0;
        }

        public static void RunWeaver(Options opt)
        {
            string pdbInputFile = Path.ChangeExtension(opt.Input, ".pdb");
            bool hasPdb = File.Exists(pdbInputFile);

            // copy .pdb if exists
            if (opt.Output != null)
            {
                File.Copy(opt.Input, opt.Output, true);

                if(hasPdb)
                    File.Copy(pdbInputFile, Path.ChangeExtension(opt.Output, ".pdb"), true);

                opt.Input = opt.Output;
                
            }
            else 
                opt.Output = opt.Input;

            // split library path by command and semicolon.
            List<string> libraryPaths = new List<string>();
            if(opt.LibraryPaths != null)
                libraryPaths.AddRange(opt.LibraryPaths.SelectMany(p=>p.Split(',', ';')).Select(p=>p.Trim().Trim('"')));

            var assemblyResolver = new AssemblyResolver(opt.Input, opt.ProjectFile, libraryPaths);
            var moduleDefinition = ModuleDefinition.ReadModule(opt.Input,
                                            new ReaderParameters
                                            {
                                                AssemblyResolver = assemblyResolver,
                                                ReadSymbols = hasPdb,
                                                SymbolReaderProvider = new PdbReaderProvider()
                                            });
            
            var weavingTask = new ModuleWeaver { ModuleDefinition = moduleDefinition,
                                                 FrameworkMainAssembly=assemblyResolver.FrameworkMainAssembly,
                                                 ProjectDirectory = assemblyResolver.ProjectDirectory,
                                                 Force=opt.Force
                                               };

            if (!opt.Verbose)
                weavingTask.LogInfo = m => { };


            bool hasChanges = weavingTask.Execute();

            if (hasChanges)
            {
                moduleDefinition.Write(opt.Output, new WriterParameters
                {
                    WriteSymbols = hasPdb,
                });
            }
        }

        public static void ShowHelp()
        {

            var mainHelp = new HelpText
            {
                Heading = new HeadingInfo("Automatic Annotations Generator", Assembly.GetEntryAssembly().GetName().Version.ToString()),
                Copyright = new CopyrightInfo("NinjaTools Contributors ", 2013, 2015),
            };
            mainHelp.AddOptions(new Options());
            Console.Error.WriteLine(mainHelp);
        }

    }
}
