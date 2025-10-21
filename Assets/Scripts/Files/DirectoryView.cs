using UnityEngine;
using VFS;
using Events;
using TMPro;

[RequireComponent(typeof(Renderer))]
public class DirectoryView : MonoBehaviour, IVFSObjectView
{
    [SerializeField] private TMP_Text nameLabel;
    public IVFSNode NodeData { get; private set; }
    private Renderer objectRenderer;

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    public void Initialize(IVFSNode node)
    {
        this.NodeData = node;
        this.gameObject.name = $"Directory: {node.Name}";
        if (nameLabel != null)
        {
            nameLabel.text = node.Name;
        }
    }
    
    void OnEnable() { GameEvents.OnCommandExecuted += OnCommandResultReceived; }
    void OnDisable() { GameEvents.OnCommandExecuted -= OnCommandResultReceived; }
    
    private void OnCommandResultReceived(CommandResult result)
    {
        if (result.TargetNode != this.NodeData) return;
        // ... (Directory-specific commands)
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

    public void DisplayContent() { /* No action */ }

    public void DeleteObject()
    {
        Destroy(this.gameObject);
    }
}

