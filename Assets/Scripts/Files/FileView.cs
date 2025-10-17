using UnityEngine;
using VFS;
using Events;

/// <summary>
/// シーン上の「ファイル」オブジェクトの振る舞いを定義するクラス。
/// Defines the behavior for "file" objects in the scene.
/// </summary>
public class FileView : MonoBehaviour, IVFSObjectView
{
    public IVFSNode NodeData { get; private set; }

    // オブジェクトの表示/非表示を制御するためのレンダラー
    private Renderer objectRenderer;

    void Awake()
    {
        // 自身のレンダラーコンポーネントを取得
        objectRenderer = GetComponent<Renderer>();
        // 最初は非表示状態にする
        Hide();
    }

    public void Initialize(IVFSNode node)
    {
        this.NodeData = node;
        this.gameObject.name = $"File: {node.Name}";
        
        // TODO: ここでファイルの種類(.txt, .exeなど)に応じて見た目を変える処理を追加できます。
    }

    void OnEnable()
    {
        // コマンド実行イベントの購読を開始
        GameEvents.OnCommandExecuted += OnCommandResultReceived;
    }

    void OnDisable()
    {
        // オブジェクトが破棄される際に、イベントの購読を解除
        GameEvents.OnCommandExecuted -= OnCommandResultReceived;
    }

    /// <summary>
    /// GameEventsから放送されたコマンド結果を受け取るメソッド。
    /// </summary>
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
            return; // 自分宛でなければ、以降の処理はしない
        }

        // 自分宛の命令だったので、コマンド名に応じて振る舞う
        switch (result.CommandExecuted)
        {
            case "cat":
                DisplayContent();
                break;
            
            // TODO: rmコマンドに対する振る舞いをここに追加します
            // case "rm":
            //     DeleteObject();
            //     break;
        }
    }

    /// <summary>
    /// オブジェクトを表示状態にします。
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
        // catコマンドが実行された時の振る舞いをここに書く
        Debug.Log($"'{NodeData.Name}' is displaying its content!");
        // TODO: オブジェクトが光る、巻物が開くなどのアニメーションを再生する処理
    }

    public void DeleteObject()
    {
        // rmコマンドが実行された時の振る舞いをここに書く
        Debug.Log($"'{NodeData.Name}' is being deleted!");
        // TODO: オブジェクトが砕け散るなどのエフェクトを再生する処理
        
        // 最後に自分自身をシーンから削除
        Destroy(this.gameObject);
    }
}

