using System.Linq;
using System.Text;
using VFS; // VirtualFileSystem を使うために必要

/// <summary>
/// コマンドを解釈し、VFSを操作し、結果をイベントとして発行するクラス（Publisher）。
/// Interprets commands, manipulates the VFS, and publishes the results as events.
/// </summary>
public class CommandProcessor
{
    private readonly VirtualFileSystem vfs;

    public CommandProcessor()
    {
        // このクラスが作られると同時に、VFS（ゲームの世界）も生成される
        vfs = new VirtualFileSystem();
    }

    /// <summary>
    /// 入力されたコマンドラインを処理し、結果をイベントとして放送する。
    /// 返り値は void になった。
    /// Processes the input command line and broadcasts the result as an event. Returns void now.
    /// </summary>
    /// <param name="inputLine">ユーザーが入力した一行の文字列</param>
    public void Process(string inputLine)
    {
        CommandResult result;

        if (string.IsNullOrWhiteSpace(inputLine))
        {
            // 空の入力は何もしないが、空の成功結果を返すこともできる
            result = new CommandResult("");
            GameEvents.TriggerCommandExecuted(result);
            return;
        }

        string[] parts = inputLine.Trim().Split(' ');
        string command = parts[0].ToLower();

        switch (command)
        {
            case "echo":
                result = ExecuteEcho(parts);
                break;
            case "ls":
                result = ExecuteLs();
                break;
            case "cd":
                result = ExecuteCd(parts);
                break;
            case "cat":
                result = ExecuteCat(parts);
                break;
            case "pwd":
                result = ExecutePwd();
                break;
            default:
                result = new CommandResult($"command not found: {command}", isError: true);
                break;
        }
        
        // 完成した CommandResult を GameEvents を通じて放送する
        GameEvents.TriggerCommandExecuted(result);
    }

    private CommandResult ExecuteEcho(string[] arguments)
    {
        string message = string.Join(" ", arguments.Skip(1));
        return new CommandResult(message);
    }

    private CommandResult ExecuteLs()
    {
        var nodes = vfs.Ls();
        var output = new StringBuilder();
        foreach (var node in nodes)
        {
            // ディレクトリの場合は名前に / をつけるなど、見た目を整形
            string displayName = node is VirtualDirectory ? $"{node.Name}/" : node.Name;
            output.AppendLine(displayName);
        }
        
        // コンソール用の整形済みテキストと、シーン表示用の生データの両方を渡す
        return new CommandResult(output.ToString(), nodes);
    }

    private CommandResult ExecuteCd(string[] arguments)
    {
        if (arguments.Length < 2)
        {
            return new CommandResult("cd: path required", isError: true);
        }
        
        string error = vfs.Cd(arguments[1]);
        
        // エラーがあればそれを結果として返す。成功すれば何も表示しない。
        return new CommandResult(error ?? "", isError: error != null);
    }

    private CommandResult ExecuteCat(string[] arguments)
    {
        if (arguments.Length < 2)
        {
            return new CommandResult("cat: filename required", isError: true);
        }

        var (content, error) = vfs.Cat(arguments[1]);

        return new CommandResult(content ?? error, isError: error != null);
    }
    private CommandResult ExecutePwd()
    {
        string path = vfs.Pwd();
        return new CommandResult(path);
    }

}
