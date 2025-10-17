using System.Collections.Generic;
using System.Linq;
using System.Text;
using VFS;    // IVFSNodeなどのVFS関連クラスを使うために必要

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

    /// <summary>
    /// ゲーム開始時に、基本的なディレクトリとファイルを構築します。
    /// Builds the basic directory and file structure at the start of the game.
    /// </summary>
    private void InitializeFileSystem()
    {
        var forest = new VirtualDirectory("forest");
        root.AddNode(forest);

        var diary = new VirtualFile("diary.txt", "This is a secret diary...");
        forest.AddNode(diary);
        
        var fairy = new VirtualFile("fairy.exe", "I am Ririn.");
        root.AddNode(fairy);
    }

    /// <summary>
    /// コマンドと引数を受け取り、適切な処理を実行して結果をイベントとして放送します。
    /// Receives a command and arguments, executes the appropriate action, and broadcasts the result as an event.
    /// </summary>
    public void ExecuteCommand(string command, List<string> arguments)
    {
        CommandResult result;

        switch (command)
        {
            case "echo":
                result = ExecuteEcho(arguments);
                break;
            case "ls":
                result = ExecuteLs(arguments);
                break;
            case "cd":
                result = ExecuteCd(arguments);
                break;
            case "cat":
                result = ExecuteCat(arguments);
                break;
            // --- touchなどの新しいコマンドはここに追加 ---
            default:
                result = new CommandResult($"command not found: {command}", isError: true);
                break;
        }
        
        GameEvents.TriggerCommandExecuted(result);
    }

    // --- 各コマンドの実行ロジック ---

    private CommandResult ExecuteEcho(List<string> arguments)
    {
        // 引数をすべて連結して、そのままコンソール出力として返す。
        string message = string.Join(" ", arguments);
        return new CommandResult(message);
    }
    
    private CommandResult ExecuteLs(List<string> arguments)
    {
        // (このコマンドは今のところ引数を使いませんが、将来の拡張のために引数を取れるようにしておきます)
        var output = new StringBuilder();
        var nodes = new List<IVFSNode>();

        // 名前順でソートして表示
        foreach (var node in CurrentDirectory.Children.Values.OrderBy(n => n.Name))
        {
            string displayName = node is VirtualDirectory ? $"{node.Name}/" : node.Name;
            output.AppendLine(displayName);
            nodes.Add(node);
        }

        // 末尾の余分な改行を削除して返す
        return new CommandResult(output.ToString().TrimEnd(), nodes);
    }

   
    private CommandResult ExecuteCd(List<string> arguments)
    {
        if (arguments.Count == 0)
        {
            // 引数がない場合はエラーメッセージを返す
            return new CommandResult("path required", isError: true);
        }

        string path = arguments[0];

        if (path == "..")
        {
            if (CurrentDirectory.Parent != null)
            {
                CurrentDirectory = CurrentDirectory.Parent;
            }
            // 成功時はコンソール出力を空にするため、空文字列("")を渡す
            return new CommandResult(""); 
        }
        
        // 大文字・小文字を区別せずにディレクトリを検索
        if (CurrentDirectory.Children.TryGetValue(path.ToLower(), out IVFSNode node))
        {
            if (node is VirtualDirectory targetDirectory)
            {
                CurrentDirectory = targetDirectory;
                return new CommandResult(""); // 成功
            }
            else
            {
                return new CommandResult($"not a directory: {path}", isError: true);
            }
        }
        else
        {
            return new CommandResult($"no such file or directory: {path}", isError: true);
        }
    }

    private CommandResult ExecuteCat(List<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return new CommandResult("filename required", isError: true);
        }

        string filename = arguments[0];
        
        if (CurrentDirectory.Children.TryGetValue(filename.ToLower(), out IVFSNode node))
        {
            if (node is VirtualFile targetFile)
            {
                // ファイルの中身をそのままコンソール出力として返す
                return new CommandResult(targetFile.Content);
            }
            else
            {
                return new CommandResult($"is a directory: {filename}", isError: true);
            }
        }
        else
        {
            return new CommandResult($"no such file: {filename}", isError: true);
        }
    }
}

