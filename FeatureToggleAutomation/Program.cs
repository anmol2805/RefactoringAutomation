using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureToggleAutomation
{
    class Program
    {
        private static readonly string solutionPath = @"";
        private static readonly string featureToggle = @"";
        private static readonly string codeWithOnlyIfStatement = @"
`       private static void SomeMethod()
        {
            // some logic before featureToggle condition 
            var someValue;

            if(_featureToggleName.isEnabled)
            {
                  //logic if feature toggle is enabled or disabled
            }

            //some logic after featureToggle condition
        }";

        private static readonly string codeWithIfElseStatement = @"
`       private static void SomeMethod()
        {
            // some logic before featureToggle condition 
            var someValue;

            if(_featureToggleName.isEnabled)
            {
                  //logic if feature toggle is enabled or disabled
            }
            else
            {
                  //logic if feature toggle is enabled or disabled
            }

            //some logic after featureToggle condition
        }";

        private static readonly string codeWithReturnStatement = @"
`       private static boolean FeatureToggleEnabled()
        {

            if(_featureToggleName.isEnabled)
            {
                  return true;
            }
            
            return false;
        }";
        static void Main(string[] args)
        {
            //SearchFeatureToggleKeywodInSolution(solutionPath,featureToggle);

            //RemoveToggleWithOnlyIfStatement(codeWithOnlyIfStatement);

            //RemoveToggleWithIfElseStatement(codeWithIfElseStatement);

            //RemoveToggleWithReturnStatement(codeWithReturnStatement);
                       
            Console.ReadKey();
        }

        private static void SearchFeatureToggleKeywodInSolution(string solutionPath, string featureToggle)
        {
            var msWorkspace = MSBuildWorkspace.Create();

            var solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;
            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var syntaxRoot = document.GetSyntaxRootAsync();
                    if (syntaxRoot.ToString().Contains(featureToggle)) {
                        Console.WriteLine(project.Name + "\t\t\t" + document.FilePath);
                    }
                                        
                }
            }
        }

        private static void RemoveToggleWithReturnStatement(string codeWithReturnStatement)
        {
            // Method returning featureToggle value
            var syntaxTree = CSharpSyntaxTree.ParseText(codeWithReturnStatement);

            var syntaxRoot = syntaxTree.GetRoot();
            
            Console.WriteLine(syntaxRoot);
            var Ifnode = syntaxRoot.DescendantNodes().OfType<IfStatementSyntax>().First();
            var returnNode = syntaxRoot.DescendantNodes().OfType<ReturnStatementSyntax>().First();
            
            if (Ifnode.Condition.ToString().Contains("!"))
            {
                var newCode = syntaxRoot.RemoveNode(Ifnode,SyntaxRemoveOptions.KeepNoTrivia);
                Console.WriteLine(newCode);
            }
            else
            {
                var newNode = syntaxRoot.ReplaceNode(Ifnode, returnNode);
                var lastReturnNode = newNode.DescendantNodes().OfType<ReturnStatementSyntax>().Last();
                var newCode = newNode.RemoveNode(lastReturnNode,SyntaxRemoveOptions.KeepNoTrivia);
                Console.WriteLine(newCode);
            }
        }

        private static void RemoveToggleWithOnlyIfStatement(string codeWithOnlyIfStatement)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(codeWithOnlyIfStatement);

            var syntaxRoot = syntaxTree.GetRoot();

            Console.WriteLine(syntaxRoot);
            var Ifnode = syntaxRoot.DescendantNodes().OfType<IfStatementSyntax>().First();
            
            if (Ifnode.Condition.ToString().Contains("!"))
            {
                var newCode = syntaxRoot.RemoveNode(Ifnode,SyntaxRemoveOptions.KeepNoTrivia);
                Console.WriteLine(newCode);
            }
            else
            {
                var newCode = syntaxRoot.ReplaceNode(Ifnode, Ifnode.Statement);
                Console.WriteLine(newCode);
            }

        }

        private static void RemoveToggleWithIfElseStatement(string codeWithIfElseStatement)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(codeWithIfElseStatement);

            var syntaxRoot = syntaxTree.GetRoot();

            Console.WriteLine(syntaxRoot);
            var Ifnode = syntaxRoot.DescendantNodes().OfType<IfStatementSyntax>().First();

            if (Ifnode.Condition.ToString().Contains("!"))
            {
                var newCode = syntaxRoot.ReplaceNode(Ifnode, Ifnode.Else.Statement);
                Console.WriteLine(newCode);
            }
            else
            {
                var newNode = syntaxRoot.RemoveNode(Ifnode.Else,SyntaxRemoveOptions.KeepNoTrivia);
                var newIfnode = newNode.DescendantNodes().OfType<IfStatementSyntax>().First();
                var newCode = newNode.ReplaceNode(newIfnode, newIfnode.Statement);
                Console.WriteLine(newCode);
            }

        }


    }

    public class SyntaxWalker : CSharpSyntaxWalker
    {
        static int Tabs = 0;
        
        public SyntaxWalker() : base(SyntaxWalkerDepth.Token)
        {
        }
        public override void Visit(SyntaxNode node)
        {
            Tabs++;
            var indents = new String('\t', Tabs);
            Console.WriteLine("node" + indents + node.Kind());
            base.Visit(node);
            Tabs--;
        }

        public override void VisitToken(SyntaxToken token)
        {
            var indents = new String('\t', Tabs);
            Console.WriteLine("token" + indents + token);
            base.VisitToken(token);
        }
    }

}
