using System.Linq; // LINQを扱うために必要

/// <summary>
/// ユーザーからのコマンド文字列を解釈し、結果を返すクラス。
/// Unityの機能（MonoBehaviour）には依存しない。
/// </summary>
public class CommandProcessor
{
    /// <summary>
    /// 入力されたコマンドラインを処理する。
    /// </summary>
    /// <param name="inputLine">ユーザーが入力した一行の文字列</param>
    /// <returns>コマンドの実行結果</returns>
    public string Process(string inputLine)
    {
        // nullや空白のみの場合は何もせずに空文字を返す
        if (string.IsNullOrWhiteSpace(inputLine))
        {
            return "";
        }

        // 入力文字列の前後の空白を削除し、スペースで分割する
        // 例: "echo  hello world " -> ["echo", "hello", "world"]
        string[] parts = inputLine.Trim().Split(' ');

        // 最初の部分をコマンドとして解釈する
        string command = parts[0].ToLower(); // コマンドは大文字・小文字を区別しないように小文字に変換

        // コマンドに応じて処理を分岐する
        switch (command)
        {
            case "echo":
                return ExecuteEcho(parts);

            // --- 今後、ここに新しいコマンドのcaseを追加していく ---
            // case "ls":
            //     return ExecuteLs(parts);
            
            default:
                return $"command not found: {command}";
        }
    }

    /// <summary>
    /// echoコマンドを実行する。
    /// </summary>
    /// <param name="arguments">コマンド名を含む、分割された文字列配列</param>
    /// <returns>echoの結果</returns>
    private string ExecuteEcho(string[] arguments)
    {
        // arguments配列からコマンド名("echo")を除いた部分を取得し、
        // もう一度スペースで連結して一つの文字列に戻す。
        // 例: ["echo", "hello", "world"] -> "hello world"
        string message = string.Join(" ", arguments.Skip(1));
        
        return message;
    }
}