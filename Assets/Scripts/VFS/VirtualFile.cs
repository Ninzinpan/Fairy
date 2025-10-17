/// <summary>
/// テキストファイルを表すクラス。文字列のコンテンツを持つ。
/// Represents a text file with string content.
/// </summary>
namespace VFS
{
    public class VirtualFile : IVFSNode
    {
        // --- IVFSNode Interface Implementation ---
        public string Name { get; private set; }
        public VirtualDirectory Parent { get; set; }

        // --- File-Specific Functionality ---

        /// <summary>
        /// ファイルの中身。
        /// The content of the file.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 新しいファイルを作成するコンストラクタ。
        /// Constructor to create a new file.
        /// </summary>
        /// <param name="name">ファイル名 (The name of the file)</param>
        /// <param name="content">ファイルの中身（任意） (Optional content of the file)</param>
        public VirtualFile(string name, string content = "")
        {
            Name = name;
            Parent = null; // 親は後から設定される (Parent is set later)
            Content = content;
        }
    }

}