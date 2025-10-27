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
    private VirtualDirectory binDirectory; // /bin ディレクトリへの参照を保持

    public VirtualFileSystem()
    {
        root = new VirtualDirectory("/");
        CurrentDirectory = root;
        InitializeFileSystem();
    }

    private void InitializeFileSystem()
    {
        binDirectory = new VirtualDirectory("bin");
        root.AddNode(binDirectory);
        var forest = new VirtualDirectory("forest");
        root.AddNode(forest);
        var home = new VirtualDirectory("home");
        root.AddNode(home);

        var diary = new VirtualFile("diary.txt", "This is a secret diary...");
        forest.AddNode(diary);
        for (int n = 1; n <= 100; n++)
        {
            var tree = new VirtualDirectory($"tree{n}");
                        forest.AddNode(tree);

        }

        var fairy = new VirtualFile("fairy.exe", "I am Ririn.");
        root.AddNode(fairy);
        var find = new VirtualFile("find", "Find command executable.");
        home.AddNode(find);
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
            case "find":
                result = ExecuteFind(command, arguments);
                break;
            case "cp":
                            result = ExecuteCp(command, arguments);
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
    
    // ★★★ findコマンドの実行メソッド ★★★
    private CommandResult ExecuteFind(string command, List<string> arguments)
    {
        if (arguments.Count != 1)
        {
            return new CommandResult("usage: find <filename>", isError: true, commandExecuted: command);
        }

        string targetName = arguments[0];
        string foundPath = SearchRecursive(root, targetName); // ルートから探索開始

        if (foundPath != null)
        {
            // 見つかったパスを返す (TargetNodeは設定しない)
            return new CommandResult(foundPath, commandExecuted: command);
        }
        else
        {
            // 見つからなかった場合のエラー
            return new CommandResult($"file not found: {targetName}", isError: true, commandExecuted: command);
        }
    }

    // ★★★ 再帰的にファイルを探索するヘルパーメソッド ★★★
    private string SearchRecursive(VirtualDirectory currentDir, string targetName)
    {
        // 1. まず現在のディレクトリ直下を探索
        foreach (var node in currentDir.Children.Values)
        {
            // 大文字小文字を無視して比較
            if (string.Equals(node.Name, targetName, System.StringComparison.OrdinalIgnoreCase))
            {
                // 見つかった！絶対パスを構築して返す
                return GetAbsolutePath(node);
            }
        }

        // 2. 直下に見つからなければ、サブディレクトリを再帰的に探索
        foreach (var node in currentDir.Children.Values)
        {
            if (node is VirtualDirectory subDir)
            {
                string foundPath = SearchRecursive(subDir, targetName);
                if (foundPath != null)
                {
                    // サブディレクトリで見つかった場合は、そのパスを上に伝播させる
                    return foundPath;
                }
            }
        }

        // このディレクトリとその配下には見つからなかった
        return null;
    }
    private CommandResult ExecuteCp(string command, List<string> arguments)
    {
        if (arguments.Count != 1)
        {
            return new CommandResult("usage: cp <filename>", isError: true, commandExecuted: command);
        }

        string targetName = arguments[0];

        // 1. コピー先 /bin ディレクトリの確認 (Initializeで生成される前提)
        if (binDirectory == null)
        {
             return new CommandResult("internal system error: /bin not found", isError: true, commandExecuted: command);
        }

        // 2. カレントディレクトリ内でコピー元ファイルを検索
        if (CurrentDirectory.Children.TryGetValue(targetName.ToLower(), out IVFSNode sourceNode))
        {
            // 3. ディレクトリはコピーできない
            if (sourceNode is VirtualDirectory)
            {
                return new CommandResult($"cannot copy directory: {targetName}", isError: true, commandExecuted: command);
            }

            if (sourceNode is VirtualFile sourceFile)
            {
                // 4. ファイルのコピーを作成
                var copiedFile = new VirtualFile(sourceFile.Name, sourceFile.Content);

                // 5. /bin に同名ファイル/ディレクトリが存在するか確認し、あれば削除 (ディレクトリは上書き不可)
                string lowerCaseName = copiedFile.Name.ToLower();
                if (binDirectory.Children.TryGetValue(lowerCaseName, out IVFSNode existingNode))
                {
                    if (existingNode is VirtualDirectory)
                    {
                        return new CommandResult($"cannot overwrite directory '{existingNode.Name}' with file '{copiedFile.Name}' in /bin", isError: true, commandExecuted: command);
                    }
                    // 既存のファイルを削除 (Dictionaryから直接Removeする)
                    if (!binDirectory.Children.Remove(lowerCaseName))
                    {
                         // 通常ここには来ないはずだが、念のためエラー処理
                         return new CommandResult($"internal system error: failed to overwrite file in /bin", isError: true, commandExecuted: command);
                    }
                     // 親への参照をクリア (AddNodeが設定する前に念のため)
                     existingNode.Parent = null;
                }

                // 6. コピーを /bin に追加
                binDirectory.AddNode(copiedFile);

                // 成功時は空メッセージを返す
                return new CommandResult("", commandExecuted: command,targetNode: sourceNode);
            }
            else
            {
                 // sourceNodeがIVFSNodeではあるが、VirtualFileでもVirtualDirectoryでもない場合 (通常ありえない)
                 return new CommandResult($"internal system error: unknown file type", isError: true, commandExecuted: command);
            }
        }
        else
        {
            // カレントディレクトリにファイルが見つからない
            return new CommandResult($"no such file: {targetName}", isError: true, commandExecuted: command);
        }
    }

    // ★★★ ノードから絶対パスを構築するヘルパーメソッド ★★★
    private string GetAbsolutePath(IVFSNode node)
    {
        if (node == root) // ルートディレクトリの場合
        {
            return "/";
        }

        var pathParts = new List<string>();
        IVFSNode currentNode = node;

        // ルートに到達するまで親をたどる
        while (currentNode != null && currentNode != root)
        {
            pathParts.Add(currentNode.Name);
            currentNode = currentNode.Parent;
        }

        // リストを逆順にして "/" で連結
        pathParts.Reverse();
        // 最後にファイル名がディレクトリの場合、末尾に "/" をつける (findの挙動に合わせる)
        string suffix = (node is VirtualDirectory) ? "/" : "";
        return "/" + string.Join("/", pathParts) + suffix;
    }
    
}

