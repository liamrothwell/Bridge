using System.Collections.Generic;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory;


namespace ScriptKit.NET
{
    public static class MemberResolver
    {
        private static string lastFileName;
        private static string editorText;
        private static ReadOnlyDocument document;
        private static IList<string> sourceFiles;
        private static ICompilation compilation;

        private static IProjectContent project;
        public static void InitResolver(IList<string> sourceFiles, IEnumerable<IAssemblyReference> assemblies)
        {            
            MemberResolver.project = null;
            MemberResolver.lastFileName = null;
            MemberResolver.sourceFiles = sourceFiles;

            MemberResolver.project = new CSharpProjectContent();
            MemberResolver.project = MemberResolver.project.AddAssemblyReferences(assemblies);
            MemberResolver.AddOrUpdateFiles();
        }

        private static void AddOrUpdateFiles()
        {
            var unresolvedFiles = new IUnresolvedFile[MemberResolver.sourceFiles.Count];
            Parallel.For(0, unresolvedFiles.Length, i =>
            {
                var file = sourceFiles[i];
                var syntaxTree = new CSharpParser().Parse(System.IO.File.ReadAllText(file), file);
                syntaxTree.Freeze();
                unresolvedFiles[i] = syntaxTree.ToTypeSystem();
            });
            MemberResolver.project = MemberResolver.project.AddOrUpdateFiles(unresolvedFiles);
            MemberResolver.compilation = MemberResolver.project.CreateCompilation();
        }

        private static int InitDocument(TextLocation location, SyntaxTree syntaxTree)
        {
            if (MemberResolver.lastFileName != syntaxTree.FileName)
            {
                MemberResolver.lastFileName = syntaxTree.FileName;
                MemberResolver.editorText = System.IO.File.ReadAllText(MemberResolver.lastFileName);
                MemberResolver.document = new ReadOnlyDocument(MemberResolver.editorText);                
            }

            return MemberResolver.document.GetOffset(location);
        }

        public static ResolveResult Resolve(MemberReferenceExpression memberReferenceExpression)
        {
            var syntaxTree = memberReferenceExpression.GetParent<SyntaxTree>();
            var offset = MemberResolver.InitDocument(memberReferenceExpression.DotToken.EndLocation, syntaxTree);
            var location = MemberResolver.document.GetLocation(offset);
            syntaxTree.Freeze();
            var unresolvedFile = syntaxTree.ToTypeSystem();
            return ResolveAtLocation.Resolve(compilation, unresolvedFile, syntaxTree, location);
        }

        public static ResolveResult ResolveParent(MemberReferenceExpression memberReferenceExpression)
        {
            var syntaxTree = memberReferenceExpression.GetParent<SyntaxTree>();
            var offset = MemberResolver.InitDocument(memberReferenceExpression.DotToken.EndLocation, syntaxTree);
            var location = MemberResolver.document.GetLocation(offset - 2);
            syntaxTree.Freeze();
            var unresolvedFile = syntaxTree.ToTypeSystem();
            return ResolveAtLocation.Resolve(compilation, unresolvedFile, syntaxTree, location);
        }

        public static ResolveResult ResolveExpression(Expression expression)
        {
            var syntaxTree = expression.GetParent<SyntaxTree>();
            var offset = MemberResolver.InitDocument(expression.StartLocation, syntaxTree);
            var location = MemberResolver.document.GetLocation(offset);
            syntaxTree.Freeze();
            var unresolvedFile = syntaxTree.ToTypeSystem();
            return ResolveAtLocation.Resolve(compilation, unresolvedFile, syntaxTree, location);
        }
    }
}