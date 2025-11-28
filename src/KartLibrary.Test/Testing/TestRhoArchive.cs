using KartLibrary.Consts;
using KartLibrary.File;
using eP.Command;
using KartCity.Common.FileType;

namespace KartLibrary.Tests.Testing;

public class TestRhoArchive: TestIRhoArchiveBase<RhoFolder, RhoFile>
{
    private RhoArchive? _rhoArchive;

    protected override IRhoArchive<RhoFolder, RhoFile>? BaseArchive => _rhoArchive;

    public TestRhoArchive()
    {
        
    }
    
    [Command("init", "init RhoArchive.")]
    private CommandExecuteResult commandInit(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        if (_rhoArchive is not null)
        {
            return new CommandExecuteResult(ResultType.Failure, "Please close current RhoArchive before initializing new Rho5Archive instance.");
        }
        else
        {
            string rhoFileName = argumentQueue.PopArgumentString();
            if(!System.IO.File.Exists(rhoFileName))
            {
                return new CommandExecuteResult(ResultType.Failure, $"Cannot found file: {rhoFileName}");
            }

            _rhoArchive = new RhoArchive();
            _rhoArchive.Open(rhoFileName);
            return new CommandExecuteResult(ResultType.Success, "");
        }
    }
    
    [Command("modify", "")]
    private CommandExecuteResult commandModify(IConsole commandConsole, CommandArgumentQueue argumentQueue)
    {
        if (BaseArchive is null)
            return new CommandExecuteResult(ResultType.Failure, "It isn't initialized.");
        if (CurrentFolder is null)
            CurrentFolder = BaseArchive.RootFolder;
        string modifiyTarget = argumentQueue.PopArgumentString();
        string modifiySource = argumentQueue.PopArgumentString();
        RhoFile? file = CurrentFolder.GetFile(modifiyTarget);
        if(file is null)
            return new CommandExecuteResult(ResultType.Failure, $"Current folder is not exist file: {modifiyTarget}.");
        if(!System.IO.File.Exists(modifiySource))
            return new CommandExecuteResult(ResultType.Failure, $"Source file is not exist.");
        file.DataSource = new FileDataSource(modifiySource);
        return new CommandExecuteResult(ResultType.Success, "");
    }

    [CommandAutoComplete("modify")]
    protected string[] commandAutoComplModify(CommandArgumentQueue argumentQueue)
    {
        List<string> suggestions = new List<string>();
        if (BaseArchive is null)
            return Array.Empty<string>();
        if (CurrentFolder is null)
            CurrentFolder = BaseArchive.RootFolder;
        if (argumentQueue.Count > 0)
        {
            string findFileName = argumentQueue.PopArgumentString();
            foreach (RhoFile file in CurrentFolder.Files)
                if (file.Name.StartsWith(findFileName))
                    suggestions.Add(file.Name);
        }
        else
        {
            foreach (RhoFile file in CurrentFolder.Files)
                suggestions.Add(file.Name);
        }
        return suggestions.ToArray();
    }
}