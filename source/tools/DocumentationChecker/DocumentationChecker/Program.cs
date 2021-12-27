using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using IniFileFormatTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocumentationChecker
{
    class Documentation
    {
        public MethodInfo Method;
        public List<FileInfo> Files = new List<FileInfo>();
    }

    class Program
    {
        static void Main()
        {
            new Program().Run();
        }

        private void Run()
        {
            LoadTestAssemblyForReflection();
            var testMethods = GetTestMethods().ToList();
            var documentedMethods = GetDocumentedMethods(testMethods).ToList();
            var documentedInFile = documentedMethods.Select(m => new Documentation() { Method = m }).ToList();
            var undocumentedMethods = testMethods.Except(documentedMethods).ToList();
            var notUndocumentedMethods = new List<MethodInfo>();
            var missingParens = new List<MethodInfo>();
            var wrongClassName = new List<MethodInfo>();

            var gitRoot = FindGitRoot();
            var markdownFiles = gitRoot.GetFiles("*.md", SearchOption.AllDirectories);

            foreach (var markdownFile in markdownFiles)
            {
                var markdown = File.ReadAllText(markdownFile.FullName);
                RemoveIfDocumented(documentedMethods, markdown);
                RemoveIfMissingParentheses(documentedMethods, markdown, missingParens);
                RemoveIfWrongClassName(documentedMethods, markdown, wrongClassName);

                CheckDocumentationFile(documentedInFile, markdown, markdownFile);

                RemoveIfUndocumented(undocumentedMethods, markdown, notUndocumentedMethods);
                RemoveIfUndocumentedMissingParentheses(undocumentedMethods, markdown, notUndocumentedMethods, missingParens);
                RemoveIfUndocumentedWrongClassName(undocumentedMethods, markdown, notUndocumentedMethods, wrongClassName);
            }

            foreach (var documentedMethod in documentedMethods)
            {
                Console.WriteLine($"{GetFullMethodName(documentedMethod)} marked as documented but isn't.");
            }

            foreach (var documentedMethod in notUndocumentedMethods)
            {
                Console.WriteLine($"{GetFullMethodName(documentedMethod)} is documented but not marked with an attribute.");
                Console.WriteLine($"     [UsedInDocumentation()]");
            }

            foreach (var method in missingParens)
            {
                Console.WriteLine($"{method.Name} is documented without parentheses.");
            }

            foreach (var method in wrongClassName)
            {
                Console.WriteLine($"{method.Name} is documented using a wrong class name.");
            }

            foreach (var undocumentedMethod in undocumentedMethods)
            {
                Console.WriteLine($"{GetFullMethodName(undocumentedMethod)} is not documented yet.");
            }

            foreach (var documentation in documentedInFile)
            {
                if (documentation.Files.Count > 0)
                    Console.WriteLine($"{GetFullMethodName(documentation.Method)} is missing filename ...");
                foreach (var file in documentation.Files)
                {
                    Console.WriteLine($"     [UsedInDocumentation(\"{file.Name}\")]");
                }
            }

            Console.ReadLine();
        }

        private void RemoveIfUndocumentedWrongClassName(List<MethodInfo> undocumentedMethods, string markdown, List<MethodInfo> notUndocumentedMethods,
            List<MethodInfo> wrongClassName)
        {
            foreach (var undocumentedMethod in IteratableClone(undocumentedMethods))
            {
                if (markdown.Contains(undocumentedMethod.Name))
                {
                    notUndocumentedMethods.Add(undocumentedMethod);
                    wrongClassName.Add(undocumentedMethod);
                }
            }
        }

        private void RemoveIfUndocumentedMissingParentheses(List<MethodInfo> undocumentedMethods, string markdown,
            List<MethodInfo> notUndocumentedMethods, List<MethodInfo> missingParens)
        {
            foreach (var undocumentedMethod in IteratableClone(undocumentedMethods))
            {
                if (markdown.Contains(GetFullMethodName(undocumentedMethod)))
                {
                    notUndocumentedMethods.Add(undocumentedMethod);
                    undocumentedMethods.Remove(undocumentedMethod);
                    missingParens.Add(undocumentedMethod);
                }
            }
        }

        private void RemoveIfUndocumented(List<MethodInfo> undocumentedMethods, string markdown, List<MethodInfo> notUndocumentedMethods)
        {
            foreach (var undocumentedMethod in IteratableClone(undocumentedMethods))
            {
                if (markdown.Contains(GetFullMethodName(undocumentedMethod) + "()"))
                {
                    notUndocumentedMethods.Add(undocumentedMethod);
                    undocumentedMethods.Remove(undocumentedMethod);
                }
            }
        }

        private static void CheckDocumentationFile(List<Documentation> documentedInFile, string markdown, FileInfo markdownFile)
        {
            foreach (var doc in documentedInFile)
            {
                if (markdown.Contains(GetFullMethodName(doc.Method)))
                {
                    // Check if method is already marked
                    bool hasFileName = false;
                    var attrs = doc.Method.GetCustomAttributes(typeof(UsedInDocumentation), true);
                    foreach (var a in attrs)
                    {
                        var u = (UsedInDocumentation)a;
                        hasFileName |= u.File == markdownFile.Name;
                    }

                    if (!hasFileName)
                    {
                        doc.Files.Add(markdownFile);
                    }
                }
            }
        }

        private void RemoveIfWrongClassName(List<MethodInfo> documentedMethods, string markdown, List<MethodInfo> wrongClassName)
        {
            foreach (var documentedMethod in IteratableClone(documentedMethods))
            {
                if (markdown.Contains(documentedMethod.Name))
                {
                    documentedMethods.Remove(documentedMethod);
                    Console.WriteLine(GetFullMethodName(documentedMethod) + "()");
                    wrongClassName.Add(documentedMethod);
                }
            }
        }

        private void RemoveIfMissingParentheses(List<MethodInfo> documentedMethods, string markdown, List<MethodInfo> missingParens)
        {
            foreach (var documentedMethod in IteratableClone(documentedMethods))
            {
                if (documentedMethod.Name == "Given_AValueOfLength65537_When_AccessingIt_Then_WeGetModuloBehavior"
                    && markdown.Contains(documentedMethod.Name))
                    Debugger.Break();
                if (markdown.Contains(GetFullMethodName(documentedMethod)))
                {
                    documentedMethods.Remove(documentedMethod);
                    missingParens.Add(documentedMethod);
                }
            }
        }

        private void RemoveIfDocumented(List<MethodInfo> documentedMethods, string markdown)
        {
            foreach (var documentedMethod in IteratableClone(documentedMethods))
            {
                if (markdown.Contains(GetFullMethodName(documentedMethod) + "()"))
                {
                    documentedMethods.Remove(documentedMethod);
                }
            }
        }

        private IEnumerable<MethodInfo> IteratableClone(List<MethodInfo> documentedMethods)
        {
            return documentedMethods.ToList();
        }

        private static string GetFullMethodName(MethodInfo documentedMethod)
        {
            return $"{documentedMethod.DeclaringType.Name}.{documentedMethod.Name}";
        }

        private DirectoryInfo FindGitRoot()
        {
            var exeName = Assembly.GetExecutingAssembly().FullName;
            var exe = new FileInfo(exeName);
            var directory = exe.Directory;
            while (directory != null && directory.GetDirectories(".git").Length == 0)
            {
                directory = directory.Parent;
            }

            return directory;
        }

        private void LoadTestAssemblyForReflection()
        {
            var _ = WindowsAPI.GetLastError.ERROR_ACCESS_DENIED;
        }



        private IEnumerable<MethodInfo> GetDocumentedMethods(IEnumerable<MethodInfo> testMethods)
        {
            var documentedMethods = testMethods.Where(m => m.GetCustomAttributes(
                typeof(UsedInDocumentation), true).Length > 0);
            return documentedMethods;
        }

        private IEnumerable<MethodInfo> GetTestMethods()
        {
            var classes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes(), (assembly, type) => type)
                .Where(t => t.IsClass)
                .Select(t => t);
            var testClasses = classes.Where(c => c.GetCustomAttributes(typeof(TestClassAttribute), true).Length > 0);
            var methods = testClasses.SelectMany(c => c.GetMethods());
            var testMethods = methods.Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), true).Length > 0);
            return testMethods;
        }
    }
}
