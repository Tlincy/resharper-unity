using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.Analyses.Bulbs;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Features.Inspections.CallHierarchy;
using JetBrains.ReSharper.Plugins.Unity.CSharp.Daemon.CallGraph;
using JetBrains.ReSharper.Plugins.Unity.CSharp.Daemon.Stages.BurstCodeAnalysis.ContextSystem;
using JetBrains.ReSharper.Plugins.Unity.CSharp.Feature.Services.ContextActions;
using JetBrains.ReSharper.Plugins.Unity.CSharp.Feature.Services.QuickFixes.CallGraph.BurstCodeAnalysis.AddDiscardAttribute;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using static JetBrains.ReSharper.Plugins.Unity.CSharp.Feature.Services.QuickFixes.CallGraph.BurstCodeAnalysis.ShowBurstCalls.ShowBurstOutcomingCallsUtil;

namespace JetBrains.ReSharper.Plugins.Unity.CSharp.Feature.Services.QuickFixes.CallGraph.BurstCodeAnalysis.ShowBurstCalls
{
    public static class ShowBurstOutcomingCallsUtil
    {
        public const string MESSAGE = "Show Burst outcoming calls";
    }

    public class ShowBurstOutcomingCallsBulbAction : IBulbAction
    {
        private readonly IClrDeclaredElement myFunction;

        public ShowBurstOutcomingCallsBulbAction(IClrDeclaredElement function)
        {
            myFunction = function;
        }

        public void Execute(ISolution solution, ITextControl textControl)
        {
            if (!solution.GetPsiServices().Caches.WaitForCaches("Burst Call Hierarchy"))
                return;

            var burstProvider = solution.GetComponent<BurstContextProvider>();

            CallHierarchyExplorerViewManager.GetInstance(solution)
                .ShowOutgoing(new DeclaredElementInstance<IClrDeclaredElement>(myFunction), result =>
                {
                    var referenceElement = result.ReferenceElement;
                    var containing = (referenceElement as ICSharpTreeNode)
                        ?.GetContainingFunctionLikeDeclarationOrClosure();

                    return burstProvider.HasContext(containing, DaemonProcessKind.GLOBAL_WARNINGS);
                });
        }

        public string Text => MESSAGE;
    }

    [ContextAction(
        Group = UnityContextActions.GroupID,
        Name = MESSAGE,
        Description = MESSAGE,
        Disabled = false,
        AllowedInNonUserFiles = false,
        Priority = 1)]
    public class ShowBurstOutcomingCallsContextAction : IContextAction
    {
        private readonly ICSharpContextActionDataProvider myDataProvider;
        private readonly SolutionAnalysisService mySwa;
        private readonly BurstContextProvider myBurstContextProvider;

        public ShowBurstOutcomingCallsContextAction([NotNull] ICSharpContextActionDataProvider dataProvider)
        {
            myDataProvider = dataProvider;

            mySwa = dataProvider.Solution.GetComponent<SolutionAnalysisService>();
            myBurstContextProvider = dataProvider.Solution.GetComponent<BurstContextProvider>();
        }

        public IEnumerable<IntentionAction> CreateBulbItems()
        {
            var identifier = myDataProvider.GetSelectedElement<ITreeNode>() as ICSharpIdentifier;
            var methodDeclaration = MethodDeclarationNavigator.GetByNameIdentifier(identifier);

            if (methodDeclaration == null)
                return EmptyList<IntentionAction>.Instance;

            if (!UnityCallGraphUtil.IsSweaCompleted(mySwa))
                return EmptyList<IntentionAction>.Instance;

            var bulbAction = new ShowBurstOutcomingCallsBulbAction(methodDeclaration.DeclaredElement);
            var processKind = UnityCallGraphUtil.GetProcessKindForGraph(mySwa);
            var isBurstContext = myBurstContextProvider.HasContext(methodDeclaration, processKind);

            return isBurstContext
                ? bulbAction.ToContextActionIntentions(IntentionsAnchors.ContextActionsAnchor,
                    CallHierarchyIcons.CalledArrow)
                : EmptyList<IntentionAction>.Instance;
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            var identifier = myDataProvider.GetSelectedElement<ITreeNode>() as ICSharpIdentifier;
            var methodDeclaration = MethodDeclarationNavigator.GetByNameIdentifier(identifier);

            return AddDiscardAttributeUtil.IsAvailable(methodDeclaration);
        }
    }
}