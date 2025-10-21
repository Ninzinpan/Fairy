using UnityEngine;

/// <summary>
/// シーン上のGameObjectが、VFS内のどのノードに対応するかを保持するためのデータコンポーネント。
/// A data component for scene GameObjects to hold a reference to their corresponding node name in the VFS.
/// </summary>
public class VFSObjectData : MonoBehaviour
{
    [Tooltip("このGameObjectが対応するVFSノードの名前（拡張子含む）。VFS内の名前と完全に一致させる必要があります。")]
    public string NodeName;

    [Tooltip("このGameObjectが所属する親ディレクトリの名前。ルートディレクトリの場合は \"/\" を使用します。")]
    public string ParentDirectoryName;
}

