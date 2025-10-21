using System.Collections.Generic;
using System.Linq;
using System.Text;
using VFS;    // IVFSNodeなどのVFS関連クラスを使うために必要
using Events;
using Unity.VisualScripting.Dependencies.NCalc; // CommandResultやGameEventsを使うために必要
/// <summary>
/// 仮想ファイルシステムの全体を管理し、コマンド実行ロジックを持つクラス。
/// Manages the entire virtual file system and contains command execution logic.
/// </summary>
public class VirtualFileSystem
{
    private readonly VirtualDirectory root;
    public VirtualDirectory CurrentDirectory { get; private set; }

    public VirtualFileSystem()
    {
        root = new VirtualDirectory("/");
        CurrentDirectory = root;
        InitializeFileSystem();
    }

    private void InitializeFileSystem()
    {
        var forest = new VirtualDirectory("forest");
        root.AddNode(forest);

        var diary = new VirtualFile("diary.txt", "This is a secret diary...");
        forest.AddNode(diary);
        for (int n = 1; n <= 100; n++)
        {
            var tree = new VirtualDirectory($"tree{n}");
                        forest.AddNode(tree);

        }

        var fairy = new VirtualFile("fairy.exe", "I am Ririn.");
        root.AddNode(fairy);
    }

    public void ExecuteCommand(string command, List<string> arguments)
    {
        CommandResult result;

        switch (command)
        {
            case "echo":
                result = ExecuteEcho(command, arguments);
                break;
            case "ls":
                result = ExecuteLs(command, arguments);
                break;
            case "cd":
                result = ExecuteCd(command, arguments);
                break;
            case "cat":
                result = ExecuteCat(command, arguments);
                break;
            default:
                result = new CommandResult($"command not found: {command}", isError: true, commandExecuted: command);
                break;
        }
        
        GameEvents.TriggerCommandExecuted(result);
    }

    // --- 各コマンドの実行ロジック ---

    private CommandResult ExecuteEcho(string command, List<string> arguments)
    {
        string message = string.Join(" ", arguments);
        return new CommandResult(message, commandExecuted: command);
    }
    
    private CommandResult ExecuteLs(string command, List<string> arguments)
    {
        var output = new StringBuilder();
        var nodes = new List<IVFSNode>();

        foreach (var node in CurrentDirectory.Children.Values.OrderBy(n => n.Name))
        {
            string displayName = node is VirtualDirectory ? $"{node.Name}/" : node.Name;
            output.AppendLine(displayName);
            nodes.Add(node);
        }
        
        return new CommandResult(output.ToString().TrimEnd(), vfsNodes: nodes, commandExecuted: command);
    }

    private CommandResult ExecuteCd(string command, List<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return new CommandResult("path required", isError: true, commandExecuted: command);
        }
        
        string path = arguments[0].TrimEnd('/');

        if (path == "..")
        {
            if (CurrentDirectory.Parent != null)
            {
                CurrentDirectory = CurrentDirectory.Parent;
            }
            return new CommandResult("", commandExecuted: command, isError: false); 
        }
        
        if (CurrentDirectory.Children.TryGetValue(path.ToLower(), out IVFSNode node))
        {
            if (node is VirtualDirectory targetDirectory)
            {
                CurrentDirectory = targetDirectory;
                // cd成功時は isError: false を明示
                return new CommandResult("", commandExecuted: command, isError: false);
            }
            else
            {
                return new CommandResult($"not a directory: {path}", isError: true, commandExecuted: command);
            }
        }
        else
        {
            return new CommandResult($"no such file or directory: {path}", isError: true, commandExecuted: command);
        }
    }

    private CommandResult ExecuteCat(string command, List<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return new CommandResult("filename required", isError: true, commandExecuted: command);
        }

        string filename = arguments[0];
        
        if (CurrentDirectory.Children.TryGetValue(filename.ToLower(), out IVFSNode node))
        {
            if (node is VirtualFile targetFile)
            {
                // targetNodeに対象ファイルを設定して結果を返す
                return new CommandResult(targetFile.Content, commandExecuted: command, targetNode: targetFile);
            }
            else
            {
                return new CommandResult($"is a directory: {filename}", isError: true, commandExecuted: command, targetNode: node);
            }
        }
        else
        {
            return new CommandResult($"no such file: {filename}", isError: true, commandExecuted: command);
        }
    }
}

