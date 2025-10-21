using UnityEngine;
using System.Collections.Generic;
using VFS;
using Events;

public class SceneObjectManager : MonoBehaviour
{
    [Header("UI Prefabs")]
    [SerializeField] private GameObject filePrefabUI; // UI版のファイルプレハブ
    [SerializeField] private GameObject directoryPrefabUI; // UI版のディレクトリプレハブ

    [Header("Scene References")]
    // TransformからRectTransformに変更
    [SerializeField] private RectTransform objectContainer; // DesktopPanelをここに設定
    [SerializeField] private ConsoleManager consoleManager;

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
        if (vfs == null) return;
        
        if (result.CommandExecuted == "cd" && !result.IsError)
        {
            SetStage(vfs.CurrentDirectory);
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

        foreach (var node in directory.Children.Values)
        {
            GameObject prefabToInstantiate = (node is VirtualDirectory) ? directoryPrefabUI : filePrefabUI;

            if (prefabToInstantiate != null)
            {
                // 1. プレハブをインスタンス化
                GameObject newInstance = Instantiate(prefabToInstantiate);
                // 2. 親をDesktopPanelに設定（これで自動で整列される！）
                // 第2引数をfalseにすることで、UIのスケールが崩れるのを防ぐ
                newInstance.transform.SetParent(objectContainer, false);
                
                IVFSObjectView view = newInstance.GetComponent<IVFSObjectView>();
                if (view != null)
                {
                    view.Initialize(node);
                    sceneObjects.Add(node, view);
                }
            }
        }
    }
}

