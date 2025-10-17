/// <summary>
/// 仮想ファイルシステム内のすべてのオブジェクト（ファイルやディレクトリ）が
/// 共通して持つべき機能を定義するインターフェース。
/// A common interface for all objects (files and directories) in the virtual file system.
/// </summary>
namespace VFS
{
    public interface IVFSNode
    {
        /// <summary>
        /// ファイル名またはディレクトリ名。
        /// The name of the file or directory.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 親ディレクトリへの参照。ルートディレクトリの場合はnullになる。
        /// A reference to the parent directory. Null if it's the root directory.
        /// </summary>
        VirtualDirectory Parent { get; set; }
    }

}