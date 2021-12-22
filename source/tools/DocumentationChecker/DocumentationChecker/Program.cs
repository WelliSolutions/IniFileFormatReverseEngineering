using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IniFileFormatTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocumentationChecker
{
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
            var undocumentedMethods = testMethods.Except(documentedMethods).ToList();
            var notUndocumentedMethods = new List<MethodInfo>();
            var missingParens = new List<MethodInfo>();
            var wrongClassName = new List<MethodInfo>();

            var gitRoot = FindGitRoot();
            var markdownFiles = gitRoot.GetFiles("*.md", SearchOption.AllDirectories);

            foreach (var markdownFile in markdownFiles)
            {
                var markdown = File.ReadAllText(markdownFile.FullName);
                foreach (var documentedMethod in IteratableClone(documentedMethods))
                {
                    if (markdown.Contains(GetFullMethodName(documentedMethod) + "()"))
                    {
                        documentedMethods.Remove(documentedMethod);
                    }
                    else if (markdown.Contains(GetFullMethodName(documentedMethod)))
                    {
                        documentedMethods.Remove(documentedMethod);
                        missingParens.Add(documentedMethod);
                    }
                    else if (markdown.Contains(documentedMethod.Name))
                    {
                        documentedMethods.Remove(documentedMethod);
                        wrongClassName.Add(documentedMethod);
                    }
                }

                foreach (var undocumentedMethod in IteratableClone(undocumentedMethods))
                {
                    if (markdown.Contains(GetFullMethodName(undocumentedMethod) + "()"))
                    {
                        notUndocumentedMethods.Add(undocumentedMethod);
                        undocumentedMethods.Remove(undocumentedMethod);
                    }
                    else if (markdown.Contains(GetFullMethodName(undocumentedMethod)))
                    {
                        notUndocumentedMethods.Add(undocumentedMethod);
                        undocumentedMethods.Remove(undocumentedMethod);
                        missingParens.Add(undocumentedMethod);
                    }
                    else if (markdown.Contains(undocumentedMethod.Name))
                    {
                        notUndocumentedMethods.Add(undocumentedMethod);
                        wrongClassName.Add(undocumentedMethod);
                    }
                }
            }

            foreach (var documentedMethod in documentedMethods)
            {
                Console.WriteLine($"{GetFullMethodName(documentedMethod)} marked as documented but isn't.");
            }

            foreach (var documentedMethod in notUndocumentedMethods)
            {
                Console.WriteLine($"{GetFullMethodName(documentedMethod)} is documented but not marked with an attribute.");
            }

            foreach (var method in missingParens)
            {
                Console.WriteLine($"{method.Name} is documented without parentheses.");
            }

            foreach (var method in wrongClassName)
            {
                Console.WriteLine($"{method.Name} is documented using a wrong class name.");
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
                typeof(DoNotRenameAttribute), true).Length > 0);
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
