using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ユーザー入力を解釈し、VFSに処理を委譲する受付クラス。
/// Interprets user input and delegates processing to the VFS.
/// </summary>
public class CommandProcessor
{
    // readonly にすることで、このクラス内での再代入を防ぎます。
    private readonly VirtualFileSystem vfs;

    public CommandProcessor()
    {
        // CommandProcessorが生成されるときに、VFSも生成します。
        vfs = new VirtualFileSystem();
    }

    /// <summary>
    /// 入力された一行を処理する。
    /// Processes the entered line of text.
    /// </summary>
    /// <param name="inputLine">ユーザーが入力した文字列 (The string entered by the user)</param>
    public void Process(string inputLine)
    {
        // nullや空白のみの場合は何もしません。
        if (string.IsNullOrWhiteSpace(inputLine))
        {
            return;
        }

        // 連続したスペースを一つの区切りとして扱えるように修正。
        // "echo   hello" -> ["echo", "hello"]
        string[] parts = inputLine.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        // コマンド名は常に小文字として扱います。
        string command = parts[0].ToLower();

        // コマンド名を除いた残りの部分を、引数のリストとしてVFSに渡します。
        List<string> arguments = parts.Skip(1).ToList();
        
        // VFSにコマンドと引数を渡し、処理を完全に委任します。
        vfs.ExecuteCommand(command, arguments);
    }
}

