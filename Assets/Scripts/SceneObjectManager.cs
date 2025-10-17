using UnityEngine;
using System.Collections.Generic;
using VFS;
using Events;
/// <summary>
/// 'cd'コマンドを検知し、現在のディレクトリに合わせてシーン上のオブジェクトを管理するクラス。
/// Detects the 'cd' command and manages scene objects corresponding to the current directory.
/// </summary>
public class SceneObjectManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject filePrefab;
    [SerializeField] private GameObject directoryPrefab;

    [Header("Scene References")]
    [SerializeField] private Transform objectContainer; // 生成したオブジェクトを格納する親オブジェクト
    [SerializeField] private ConsoleManager consoleManager; 

    // --- Add settings for object layout ---
    [Header("Object Layout Settings")]
    [SerializeField] private int itemsPerRow = 5; // How many items to place in a single row
    [SerializeField] private Vector2 itemSpacing = new Vector2(2.0f, -2.0f); // Spacing between items (X, Y)

    private VirtualFileSystem vfs;
    // シーン上のオブジェクトの名簿
    private readonly Dictionary<IVFSNode, IVFSObjectView> sceneObjects = new Dictionary<IVFSNode, IVFSObjectView>();

    void Start()
    {
        if (consoleManager == null)
        {
            Debug.LogError("SceneObjectManager: ConsoleManager is not assigned in the inspector!", this);
            return;
        }
        
        // ConsoleManagerを経由してVFSインスタンスへの参照を取得
        vfs = consoleManager.CommandProcessorInstance.VfsInstance;
        
        // ゲーム開始時に、ルートディレクトリの状態で舞台を初期設定
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
        
        // cdコマンドが成功した時だけ反応する
        if (result.CommandExecuted == "cd" && !result.IsError)
        {
            SetStage(vfs.CurrentDirectory);
        }
    }

    /// <summary>
    /// 指定されたディレクトリの状態に合わせて、シーン上のオブジェクトを再構築する。
    /// Rebuilds the scene objects to match the state of the specified directory.
    /// </summary>
    private void SetStage(VirtualDirectory directory)
    {
        // 1. 古いオブジェクトを全て片付ける
        foreach (var view in sceneObjects.Values)
        {
            if (view as MonoBehaviour != null)
                Destroy((view as MonoBehaviour).gameObject);
        }
        sceneObjects.Clear();

        // 2. 新しいオブジェクトを配置する
        if (directory == null || directory.Children == null) return;
        
        int itemCount = 0;
        foreach (var node in directory.Children.Values)
        {
            GameObject prefabToInstantiate = null;

            if (node is VirtualDirectory)
            {
                prefabToInstantiate = directoryPrefab;
            }
            else if (node is VirtualFile)
            {
                prefabToInstantiate = filePrefab;
            }

            if (prefabToInstantiate != null)
            {
                // --- Calculate 2D grid position ---
                float xPos = (itemCount % itemsPerRow) * itemSpacing.x;
                float yPos = (itemCount / itemsPerRow) * itemSpacing.y;
                Vector2 spawnPosition = new Vector2(xPos, yPos);

                // objectContainerを親としてインスタンス化
                GameObject newInstance = Instantiate(prefabToInstantiate, objectContainer);
                // --- Set local position relative to the container ---
                newInstance.transform.localPosition = spawnPosition;
                
                IVFSObjectView view = newInstance.GetComponent<IVFSObjectView>();
                if (view != null)
                {
                    view.Initialize(node);
                    sceneObjects.Add(node, view);
                }
                
                itemCount++;
            }
        }
    }
}

