using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.Unity.CSharp.Daemon.CallGraph;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace JetBrains.ReSharper.Plugins.Unity.CSharp.Feature.Services.QuickFixes.CallGraph.PerformanceAnalysis
{
    public class PerformanceAnalysisDisableByCommentBulbAction : AddCommentActionBase
    {
        public PerformanceAnalysisDisableByCommentBulbAction([NotNull] IMethodDeclaration methodDeclaration)
            : base(methodDeclaration)
        {
        }
        
        protected override string Comment => "// " + ReSharperControlConstruct.DisablePrefix + " " +
                                             UnityCallGraphUtil.PerformanceExpensiveComment;
        public override string Text => PerformanceDisableUtil.MESSAGE;
    }
}