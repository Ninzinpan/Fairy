using System.Collections.Generic;
using VFS; // IVFSNode を使うために必要
namespace Events
{
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
        /// 実行されたコマンド名 ("ls", "cd", "cat" など)。
        /// The name of the executed command (e.g., "ls", "cd", "cat").
        /// </summary>
        public string CommandExecuted { get; private set; }

        /// <summary>
        /// コマンドの対象となった特定のノード (cat や rm で使用)。
        /// The specific node targeted by the command (used by cat, rm, etc.).
        /// </summary>
        public IVFSNode TargetNode { get; private set; }

        /// <summary>
        /// CommandResultのコンストラクタ。
        /// Constructor for CommandResult.
        /// </summary>
        public CommandResult(string consoleOutput, List<IVFSNode> vfsNodes = null, bool isError = false, string commandExecuted = "", IVFSNode targetNode = null)
        {
            ConsoleOutput = consoleOutput;
            VFSNodes = vfsNodes ?? new List<IVFSNode>(); // nullの場合は空のリストを生成
            IsError = isError;
            CommandExecuted = commandExecuted;
            TargetNode = targetNode;
        }
    }

}