using System;
using JetBrains.Application.changes;
using JetBrains.Application.CommandProcessing;
using JetBrains.Application.FileSystemTracker;
using JetBrains.Application.Progress;
using JetBrains.Application.UI.Components;
using JetBrains.Collections.Viewable;
using JetBrains.Diagnostics;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Transaction;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.Unity.ProjectModel
{
  [SolutionComponent]
  public class MetaFileTrackerNew
  {
    private readonly ISolution mySolution;
    private readonly IIsApplicationActiveState myIsApplicationActiveState;
    private readonly ILogger myLogger;

    public MetaFileTrackerNew(IFileSystemTracker fileSystemTracker, Lifetime lifetime, ISolution solution, 
      IIsApplicationActiveState isApplicationActiveState, UnitySolutionTracker unitySolutionTracker, ILogger logger)
    {
      mySolution = solution;
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

    private void OnDeleted(FileSystemPath path)
    {
      if (path.ExtensionNoDot == "meta")
        return; // ignore changes to meta files
      
      myLogger.Trace("*** resharper-unity: File deleted {0}", path);

      var metaFile = GetMetaFile(path);
      DeleteMetaFile(metaFile);
    }
    
    private void OnRenamed(FileSystemChangeDelta delta)
    {
      if (delta.OldPath.ExtensionNoDot == "meta")
        return; // ignore changes to meta files
      
      myLogger.Trace("*** resharper-unity: Renamed {0} -> {1}", delta.OldPath, delta.NewPath);

      var newMetaFile = GetMetaFile(delta.NewPath);
      if (!newMetaFile.ExistsFile)
      {
        var oldMetaFile = GetMetaFile(delta.OldPath);
        if (newMetaFile != oldMetaFile)
        {
          if (oldMetaFile.ExistsFile)
            RenameMetaFile(oldMetaFile, newMetaFile, string.Empty);
        }
      }
    }
    
    private static FileSystemPath GetMetaFile(FileSystemPath location)
    {
      return FileSystemPath.Parse(location + ".meta");
    }
    
    private void DeleteMetaFile(FileSystemPath path)
    {
      try
      {
        if (path.ExistsFile)
        {
          DoUnderTransaction("Unity::DeleteMetaFile", path.DeleteFile);
          myLogger.Info("*** resharper-unity: Meta removed {0}", path);
        }
      }
      catch (Exception e)
      {
        myLogger.LogException(LoggingLevel.ERROR, e, ExceptionOrigin.Assertion,
          $"Failed to delete Unity meta file {path}");
      }
    }
    
    private void RenameMetaFile(FileSystemPath oldPath, FileSystemPath newPath, string extraDetails)
    {
      try
      {
        myLogger.Info("*** resharper-unity: Meta renamed{2} {0} -> {1}", oldPath, newPath, extraDetails);
        DoUnderTransaction("Unity::RenameMetaFile", () => oldPath.MoveFile(newPath, true));
      }
      catch (Exception e)
      {
        myLogger.LogException(LoggingLevel.ERROR, e, ExceptionOrigin.Assertion,
          $"Failed to rename Unity meta file {oldPath} -> {newPath}");
      }
    }
    
    private void DoUnderTransaction(string command, Action action)
    {
      // Create a transaction - Rider will hook the file system and cause the VFS to refresh
      using (mySolution.GetComponent<ICommandProcessor>().UsingCommand("Project Model Modification"))
      {
        // create ProjectModelBatchChangeCookie at first to send all changes only after commit transaction (and changes in file system)
        using (WriteLockCookie.Create())
        using (new ProjectModelBatchChangeCookie(mySolution, SimpleTaskExecutor.Instance))
        using (var cookie = mySolution.CreateTransactionCookie(DefaultAction.Commit, "Execute Rider action"))
        {
          action();
        }
      }
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
        
        myLogger.Info($"*** resharper-unity: {delta.ChangeType}, {delta.NewPath}, {delta.OldPath}");

        switch (delta.ChangeType)
        {
          case FileSystemChangeType.DELETED:
            myMetaFileTrackerNew.OnDeleted(delta.OldPath);
            break;
          case FileSystemChangeType.RENAMED:
            myMetaFileTrackerNew.OnRenamed(delta);
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