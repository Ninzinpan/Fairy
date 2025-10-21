using UnityEngine;
using VFS;
using Events;
using TMPro;

[RequireComponent(typeof(Renderer))]
public class FileView : MonoBehaviour, IVFSObjectView
{
    [SerializeField] private TMP_Text nameLabel;
    public IVFSNode NodeData { get; private set; }
    private Renderer objectRenderer;

    void Awake()
    {
        // 自身の表示コンポーネントを準備するが、ここでは何もしない。
        // 表示/非表示の命令はSceneObjectManagerから来る。
        objectRenderer = GetComponent<Renderer>();
    }

    public void Initialize(IVFSNode node)
    {
        this.NodeData = node;
        this.gameObject.name = $"File: {node.Name}";
        if (nameLabel != null)
        {
            nameLabel.text = node.Name;
        }
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
        if (result.TargetNode != this.NodeData) return;
        
        switch (result.CommandExecuted)
        {
            case "cat":
                DisplayContent();
                break;
        }
    }

    public void Show()
    {
        if (objectRenderer != null) objectRenderer.enabled = true;
        if (nameLabel != null) nameLabel.enabled = true;
    }

    public void Hide()
    {
        if (objectRenderer != null) objectRenderer.enabled = false;
        if (nameLabel != null) nameLabel.enabled = false;
    }

    public void DisplayContent()
    {
        Debug.Log($"'{NodeData.Name}' is displaying its content!");
    }

    public void DeleteObject()
    {
        Destroy(this.gameObject);
    }
}

