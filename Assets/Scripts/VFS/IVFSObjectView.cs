using VFS;

/// <summary>
/// シーン上に表示される全てのVFSオブジェクトが実装すべきインターフェース。
/// An interface that all VFS objects displayed in the scene must implement.
/// </summary>
public interface IVFSObjectView
{
    /// <summary>
    /// このオブジェクトに割り当てられたVFSノードのデータ。
    /// The VFS node data assigned to this object.
    /// </summary>
    IVFSNode NodeData { get; }

    /// <summary>
    /// VFSノードのデータを使って、このオブジェクトを初期化します。
    /// Initializes this object using data from a VFS node.
    /// </summary>
    /// <param name="node">The VFS node data.</param>
    void Initialize(IVFSNode node);

    /// <summary>
    /// 'cat'コマンドに相当する振る舞いを実行します。
    /// Executes behavior corresponding to the 'cat' command.
    /// </summary>
    void DisplayContent();

    /// <summary>
    /// 'rm'コマンドに相当する振る舞いを実行します。
    /// Executes behavior corresponding to the 'rm' command.
    /// </summary>
    void DeleteObject();
}
