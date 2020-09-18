using System;
using JetBrains.Application.changes;
using JetBrains.Application.FileSystemTracker;
using JetBrains.Application.UI.Components;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.Unity.ProjectModel
{
  [SolutionComponent]
  public class MetaFileTrackerNew
  {
    private readonly IIsApplicationActiveState myIsApplicationActiveState;
    private readonly ILogger myLogger;

    public MetaFileTrackerNew(IFileSystemTracker fileSystemTracker, Lifetime lifetime, ISolution solution, 
      IIsApplicationActiveState isApplicationActiveState, UnitySolutionTracker unitySolutionTracker, ILogger logger)
    {
      myIsApplicationActiveState = isApplicationActiveState;
      myLogger = logger;

      unitySolutionTracker.IsUnityProject.AdviseOnce(lifetime, args =>
      {
        if (!args) return;
        
        // todo: check file based packages, maybe use `RIDER-49511 Move PackageManger to backend` 
        fileSystemTracker.RegisterPrioritySink(lifetime, FileSystemChange, HandlingPriority.Other);
      });
    }

    private void FileSystemChange(FileSystemChange change)
    {
      // if (!myIsApplicationActiveState.IsApplicationActive.Value)
      //   return; // ignore external changes
      
      var visitor = new Visitor(this, myLogger);
      foreach (var fileSystemChangeDelta in change.Deltas)
        fileSystemChangeDelta.Accept(visitor);
    }

    internal void OnDelete(FileSystemPath path)
    {
      if (path.ExtensionNoDot == "meta")
        return; // ignore changes to meta files
      
      var metaFile = path.ChangeExtension(path.ExtensionNoDot + ".meta");
      if (metaFile.ExistsFile)
        metaFile.DeleteFile();
    }
    
    private class Visitor : RecursiveFileSystemChangeDeltaVisitor
    {
      private readonly MetaFileTrackerNew myMetaFileTrackerNew;
      private readonly ILogger myLogger;

      public Visitor(MetaFileTrackerNew tracker, ILogger logger)
      {
        myMetaFileTrackerNew = tracker;
        myLogger = logger;
      }

      public override void Visit(FileSystemChangeDelta delta)
      {
        base.Visit(delta);
        
        myLogger.Info($"MetaFileTrackerNew {delta.ChangeType}, {delta.NewPath}, {delta.OldPath}");

        switch (delta.ChangeType)
        {
          case FileSystemChangeType.DELETED:
            myMetaFileTrackerNew.OnDelete(delta.OldPath);
            break;
          case FileSystemChangeType.RENAMED:
            break;
          case FileSystemChangeType.UNKNOWN:
          case FileSystemChangeType.SUBTREE_CHANGED:
          case FileSystemChangeType.ADDED:
          case FileSystemChangeType.CHANGED:
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }
  }
}