using System.Collections.Generic;
using System.Linq;
using VFS; // IVFSNode を使うために必要

/// <summary>
/// コマンド実行の結果を格納し、イベントで渡すためのデータクラス。
/// A data class to hold the result of a command execution, passed via events.
/// </summary>
public class CommandResult
{
    /// <summary>
    /// コンソールに表示するための整形済みテキスト。
    /// The formatted text to be displayed in the console.
    /// </summary>
    public string ConsoleOutput { get; private set; }

    /// <summary>
    /// シーンにオブジェクトを表示するための、生のファイル/ディレクトリ情報のリスト。
    /// A list of raw file/directory nodes for displaying objects in the scene.
    /// </summary>
    public List<IVFSNode> VFSNodes { get; private set; }

    /// <summary>
    /// コマンドがエラーになったかどうか。
    /// A flag indicating if the command resulted in an error.
    /// </summary>
    public bool IsError { get; private set; }

    /// <summary>
    /// CommandResultのコンストラクタ。
    /// Constructor for CommandResult.
    /// </summary>
    /// <param name="consoleOutput">コンソール出力 (Console output text)</param>
    /// <param name="vfsNodes">関連するVFSノード (Associated VFS nodes)</param>
    /// <param name="isError">エラーフラグ (Error flag)</param>
    public CommandResult(string consoleOutput, List<IVFSNode> vfsNodes = null, bool isError = false)
    {
        ConsoleOutput = consoleOutput;
        // vfsNodesがnullの場合は、空のリストを生成する
        VFSNodes = vfsNodes ?? new List<IVFSNode>();
        IsError = isError;
    }
}

