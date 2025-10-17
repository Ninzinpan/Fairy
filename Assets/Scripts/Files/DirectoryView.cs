using UnityEngine;
using VFS;
using Events;

/// <summary>
/// シーン上の「ディレクトリ」オブジェクトの振る舞いを定義するクラス。
/// Defines the behavior for "directory" objects in the scene.
/// </summary>
public class DirectoryView : MonoBehaviour, IVFSObjectView
{
    public IVFSNode NodeData { get; private set; }
    
    // オブジェクトの表示/非表示を制御するためのレンダラー
    private Renderer objectRenderer; 

    void Awake()
    {
        // 自身のレンダラーコンポーネントを取得
        // 2Dの場合はSpriteRenderer、3Dの場合はMeshRendererなどが対象になります
        objectRenderer = GetComponent<Renderer>();
        // 最初は非表示状態にする
        Hide();
    }

    public void Initialize(IVFSNode node)
    {
        this.NodeData = node;
        this.gameObject.name = $"Directory: {node.Name}";
    }
    
    void OnEnable()
    {
        GameEvents.OnCommandExecuted += OnCommandResultReceived;
    }

    void OnDisable()
    {
        GameEvents.OnCommandExecuted -= OnCommandResultReceived;
    }

    private void OnCommandResultReceived(CommandResult result)
    {
        // --- lsコマンドに対する振る舞い ---
        if (result.CommandExecuted == "ls")
        {
            // lsコマンドの結果リストに自分が含まれていれば、表示する
            if (result.VFSNodes.Contains(this.NodeData))
            {
                Show();
            }
        }
        
        // --- ターゲット指定コマンドに対する振る舞い ---
        if (result.TargetNode != this.NodeData)
        {
            return;
        }

        // コマンド名に応じた振る舞い
        switch (result.CommandExecuted)
        {
            // TODO: ディレクトリに対するrmコマンドの処理などをここに追加
            // case "rm":
            //     DeleteObject();
            //     break;
        }
    }
    
    /// <summary>
    /// オブジェクトを表示状態にします。
    /// Makes the object visible.
    /// </summary>
    public void Show()
    {
        if (objectRenderer != null)
        {
            objectRenderer.enabled = true;
        }
    }

    /// <summary>
    /// オブジェクトを非表示状態にします。
    /// Makes the object hidden.
    /// </summary>
    public void Hide()
    {
        if (objectRenderer != null)
        {
            objectRenderer.enabled = false;
        }
    }

    public void DisplayContent()
    {
        // ディレクトリはcatできないので、通常は何もしない
    }

    public void DeleteObject()
    {
        Debug.Log($"Directory '{NodeData.Name}' is being deleted!");
        Destroy(this.gameObject);
    }
}

