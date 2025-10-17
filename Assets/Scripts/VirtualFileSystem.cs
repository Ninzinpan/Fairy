using System.Collections.Generic;
using System.Linq; // For LINQ methods like .Select
using VFS; // IVFSNode, VirtualDirectory, VirtualFile を使うために必要

/// <summary>
/// 仮想ファイルシステム全体を管理し、「現在地」を記憶するクラス。
/// Manages the entire virtual file system and keeps track of the current location.
/// </summary>
public class VirtualFileSystem
{
    private readonly VirtualDirectory root;
    public VirtualDirectory CurrentDirectory { get; private set; }

    /// <summary>
    /// VFSを初期化し、ゲームの世界（ディレクトリ構造）を構築する。
    /// Initializes the VFS and builds the game world (directory structure).
    /// </summary>
    public VirtualFileSystem()
    {
        // 1. ルートディレクトリを作成
        root = new VirtualDirectory("/");
        
        // 2. 初期ディレクトリとファイルを作成
        var homeDir = new VirtualDirectory("home");
        var forestDir = new VirtualDirectory("forest");
        
        var diaryFile = new VirtualFile("diary.txt", "It was a lonely day...\nI decided to create a new friend.");
        var fairyExe = new VirtualFile("fairy.exe", "This seems to be the core program of this world's guide.");

        // 3. ディレクトリ構造を組み立てる (Add nodes to build the tree)
        root.AddNode(homeDir);
        root.AddNode(forestDir);
        homeDir.AddNode(diaryFile);
        homeDir.AddNode(fairyExe);

        // 4. 開始時の現在地を設定
        CurrentDirectory = homeDir;
    }

    /// <summary>
    /// 現在のディレクトリの内容を取得する。
    /// Gets the contents of the current directory.
    /// </summary>
    /// <returns>現在のディレクトリに含まれるノードのリスト (A list of nodes in the current directory)</returns>
    public List<IVFSNode> Ls()
    {
        return CurrentDirectory.Children.Values.ToList();
    }

    /// <summary>
    /// 指定されたパスにディレクトリを移動する。
    /// Changes the current directory to the specified path.
    /// </summary>
    /// <param name="path">移動先のパス (e.g., "forest", "..")</param>
    /// <returns>エラーメッセージ。成功した場合はnull。 (Error message, or null on success)</returns>
    public string Cd(string path)
    {
        if (path == "..")
        {
            if (CurrentDirectory.Parent != null)
            {
                CurrentDirectory = CurrentDirectory.Parent;
                return null; // 成功
            }
            return "cd: already at root directory";
        }
        
        if (path == "/" || path == "~")
        {
            CurrentDirectory = root;
            return null; // 成功
        }
        
        if (CurrentDirectory.Children.TryGetValue(path.ToLower(), out IVFSNode node))
        {
            if (node is VirtualDirectory targetDir)
            {
                CurrentDirectory = targetDir;
                return null; // 成功
            }
            return $"cd: not a directory: {path}";
        }
        
        return $"cd: no such file or directory: {path}";
    }

    /// <summary>
    /// 指定されたファイルの内容を取得する。
    /// Gets the content of the specified file.
    /// </summary>
    /// <param name="filename">ファイル名</param>
    /// <returns>ファイルの中身、またはエラーメッセージ (Tuple of content and error message)</returns>
    public (string content, string error) Cat(string filename)
    {
        if (CurrentDirectory.Children.TryGetValue(filename.ToLower(), out IVFSNode node))
        {
            if (node is VirtualFile targetFile)
            {
                return (targetFile.Content, null); // 成功
            }
            return (null, $"cat: {filename} is a directory");
        }

        return (null, $"cat: no such file: {filename}");
    }
    public string Pwd()
    {
        List<string> pathParts = new List<string>();
        VirtualDirectory current = CurrentDirectory;

        while (current != null && current != root)
        {
            pathParts.Add(current.Name);
            current = current.Parent;
        }

        pathParts.Reverse();
        string path = "/" + string.Join("/", pathParts);
        return path;
    }
}
