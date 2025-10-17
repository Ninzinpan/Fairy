using System.Collections.Generic;
using UnityEngine; // Debug.LogWarningを使用するため

/// <summary>
/// ディレクトリを表すクラス。内部に他の IVFSNode を持つことができる。
/// Represents a directory, which can contain other IVFSNodes.
/// </summary>
/// 
namespace VFS
{
    public class VirtualDirectory : IVFSNode
    {
        // --- IVFSNode Interface Implementation ---
        public string Name { get; private set; }
        public VirtualDirectory Parent { get; set; }

        // --- Directory-Specific Functionality ---

        /// <summary>
        /// このディレクトリに含まれる子要素のリスト。
        /// ファイル名（キー）で高速にアクセスできるようDictionaryを使用する。
        /// A dictionary of child nodes contained within this directory for fast lookup by name.
        /// </summary>
        public Dictionary<string, IVFSNode> Children { get; private set; }

        /// <summary>
        /// 新しいディレクトリを作成するコンストラクタ。
        /// Constructor to create a new directory.
        /// </summary>
        /// <param name="name">ディレクトリ名 (The name of the directory)</param>
        public VirtualDirectory(string name)
        {
            Name = name;
            Parent = null; // 親は後から設定される (Parent is set later)
            Children = new Dictionary<string, IVFSNode>();
        }

        /// <summary>
        /// このディレクトリに新しいノード（ファイル or ディレクトリ）を追加する。
        /// Adds a new node (file or directory) to this directory.
        /// </summary>
        /// <param name="node">追加するノード (The node to add)</param>
        public void AddNode(IVFSNode node)
        {
            if (!Children.ContainsKey(node.Name.ToLower())) // 名前は小文字で統一して管理
            {
                node.Parent = this;
                Children.Add(node.Name.ToLower(), node);
            }
            else
            {
                Debug.LogWarning($"A node with the name '{node.Name}' already exists in '{this.Name}'.");
            }
        }
    }
}

