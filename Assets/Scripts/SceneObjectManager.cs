using UnityEngine;
using System.Collections.Generic;
using VFS;
using Events;

public class SceneObjectManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject filePrefab;
    [SerializeField] private GameObject directoryPrefab;

    [Header("Scene References")]
    [SerializeField] private Transform objectContainer;
    [SerializeField] private ConsoleManager consoleManager;

    [Header("Object Layout Settings")]
    [SerializeField] private int itemsPerRow = 5;
    [SerializeField] private Vector2 itemSpacing = new Vector2(2.0f, -2.0f);

    private VirtualFileSystem vfs;
    private readonly Dictionary<IVFSNode, IVFSObjectView> sceneObjects = new Dictionary<IVFSNode, IVFSObjectView>();

    void Start()
    {
        if (consoleManager == null)
        {
            Debug.LogError("SceneObjectManager: ConsoleManager is not assigned in the inspector!", this);
            return;
        }
        vfs = consoleManager.CommandProcessorInstance.VfsInstance;
        
        if (vfs != null)
        {
            SetStage(vfs.CurrentDirectory);
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
        if (vfs == null || result.IsError) return;

        switch (result.CommandExecuted)
        {
            case "cd":
                SetStage(vfs.CurrentDirectory);
                break;
            case "ls":
                ShowObjects(result.VFSNodes);
                break;
            case "rm":
                RemoveObject(result.TargetNode);
                break;
        }
    }

    private void SetStage(VirtualDirectory directory)
    {
        foreach (var view in sceneObjects.Values)
        {
            if (view as MonoBehaviour != null)
                Destroy((view as MonoBehaviour).gameObject);
        }
        sceneObjects.Clear();

        if (directory == null || directory.Children == null) return;

        int itemCount = 0;
        foreach (var node in directory.Children.Values)
        {
            GameObject prefabToInstantiate = (node is VirtualDirectory) ? directoryPrefab : filePrefab;

            if (prefabToInstantiate != null)
            {
                float xPos = (itemCount % itemsPerRow) * itemSpacing.x;
                float yPos = (itemCount / itemsPerRow) * itemSpacing.y;
                Vector2 spawnPosition = new Vector2(xPos, yPos);

                GameObject newInstance = Instantiate(prefabToInstantiate, objectContainer);
                newInstance.transform.localPosition = spawnPosition;
                
                IVFSObjectView view = newInstance.GetComponent<IVFSObjectView>();
                if (view != null)
                {
                    view.Initialize(node);
                    sceneObjects.Add(node, view);

                    // ★★★ ここが重要 ★★★
                    // 生成直後に、マネージャーが非表示を命令する
                    // IVFSObjectViewにはHide()がないため、具体的な型にキャストして呼び出す
                    if (view is FileView fileView) fileView.Hide();
                    else if (view is DirectoryView dirView) dirView.Hide();
                }
                itemCount++;
            }
        }
    }
    
    private void ShowObjects(List<IVFSNode> nodesToShow)
    {
        foreach(var node in nodesToShow)
        {
            if (sceneObjects.TryGetValue(node, out IVFSObjectView view))
            {
                // ここでも同様に、具体的な型にキャストしてShow()を呼び出す
                if (view is FileView fileView) fileView.Show();
                else if (view is DirectoryView dirView) dirView.Show();
            }
        }
    }

    private void RemoveObject(IVFSNode nodeToRemove)
    {
        if (nodeToRemove != null && sceneObjects.TryGetValue(nodeToRemove, out IVFSObjectView view))
        {
            view.DeleteObject();
            sceneObjects.Remove(nodeToRemove);
        }
    }
}

