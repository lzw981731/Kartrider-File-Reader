using System.Reflection;
using eP.Command;

namespace eP.Testing
{
    public class TestProgram: TestStage
    {
        private bool _exited;

        private TestStage _activeStage;
        private Dictionary<string, TestStage> _registeredStages;
        private StandardConsole _console;

        public TestProgram() 
        {
            _exited = false;
            _activeStage = this;
            _registeredStages = new Dictionary<string, TestStage>();
            _console = new StandardConsole();
            _console.AutoComplete += ConsoleAutoComplete;
            _console.Separators = new char[] { ' ' };

            RegisterAllTestStages();
        }

        public void Run()
        {
            AddStep("Test all stages", () =>
            {
                foreach (TestStage testStage in _registeredStages.Values)
                    testStage.RunSteps(_console);
            });
            while (!_exited)
            {
                _console.SetForegroundColor(ConsoleColor.Cyan);
                _console.Write($"{_activeStage.StageName}> ");
                _console.SetDefaultColor();
                string? command = _console.ReadLine();
                if (command is null)
                    return;
                command = command.Trim();
                if(command.Length > 0)
                    _activeStage.ExecuteCommand(_console, command);
            }
        }

        private void RegisterAllTestStages()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly assembly in assemblies)
                foreach(Type type in assembly.GetTypes().Select(x=> x).Where(x=> x.IsSubclassOf(typeof(TestStage)) && x != typeof(TestProgram) && !x.IsAbstract))
                {
                    ConstructorInfo? constructorInfo = type.GetConstructor(new Type[0]);
                    if(constructorInfo is not null)
                    {
                        TestStage newTestStage = (TestStage)constructorInfo.Invoke(new object[0]);
                        _registeredStages.Add(newTestStage.StageName, newTestStage);
                    }
                }
        }

        [Command("exit", "Exit the test program.")]
        protected override CommandExecuteResult CommandExit(IConsole commandConsole, CommandArgumentQueue argumentQueue)
        {
            _exited = true;
            commandConsole.SetForegroundColor(ConsoleColor.Green);
            commandConsole.WriteLine("Bye.");
            commandConsole.SetDefaultColor();
            return new CommandExecuteResult(ResultType.Success, "");
        }

        [Command("list", "List all test stages in this test program.")]
        private CommandExecuteResult CommandListTestStages(IConsole commandConsole, CommandArgumentQueue argumentQueue)
        {
            commandConsole.WriteLine($"Available test stages:");
            foreach (TestStage testStage in _registeredStages.Values)
            {
                commandConsole.SetForegroundColor(ConsoleColor.Green);
                commandConsole.WriteLine($"{testStage.StageName}");
                commandConsole.SetDefaultColor();
            }
            commandConsole.WriteLine("");
            return new CommandExecuteResult(ResultType.Success, "");
        }

        [Command("enter", "Enter to a test stage.")]
        private CommandExecuteResult CommandEnter(IConsole commandConsole, CommandArgumentQueue argumentQueue)
        {
            string stageName = argumentQueue.PopArgumentString();
            if (!_registeredStages.ContainsKey(stageName))
                return new CommandExecuteResult(ResultType.Failure, $"Cannot found stage \"{stageName}\".");
            commandConsole.Clear();
            _activeStage = _registeredStages[stageName];
            _activeStage.Exited += ActiveStageExited;
            return new CommandExecuteResult(ResultType.Success, "");
        }

        [CommandAutoComplete("enter")]
        private string[] CommandAutoComplEnter(CommandArgumentQueue argumentQueue)
        {
            List<string> suggestions = new List<string>();
            if(argumentQueue.Count > 0)
            {
                string partStageName = argumentQueue.PopArgumentString();
                foreach (string stageName in _registeredStages.Keys)
                {
                    if(stageName.StartsWith(partStageName))
                        suggestions.Add(stageName);
                }
            }
            else
            {
                foreach(string stageName in _registeredStages.Keys)
                {
                    suggestions.Add(stageName);
                }
            }
            return suggestions.ToArray();
        }

        [Command("about", "About this program.")]
        private CommandExecuteResult CommandAbout(IConsole commandConsole, CommandArgumentQueue argumentQueue)
        {
            commandConsole.WriteLine(
"KartLibrary.Tests is a part of KartLibrary.\n" +
"KartLibrary is a open-source project to decode KartRider game files.\n" +
"This project is release under \x1b[33mGPL 3.0\x1b[0m license.\n");
            return new CommandExecuteResult(ResultType.Success, "");
        }

        private void ActiveStageExited()
        {
            _activeStage.Exited -= ActiveStageExited;
            _activeStage = this;
        }
        
        private string[] ConsoleAutoComplete(string text, int index)
        {
            return _activeStage.AutoCompleteCommand(text, index);
        }
    }
}
