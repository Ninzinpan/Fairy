using UnityEngine;
using Events; // CommandResult, GameEventsを使うために必要
using VFS;   // IVFSNodeを使うために必要

/// <summary>
/// CommandResultイベントを解釈し、ゲームイベントの発生を判定してEventManagerに通知するクラス。
/// Interprets CommandResult events, determines if game events should trigger, and notifies the EventManager.
/// </summary>
public class CommandEventInterpreter : MonoBehaviour
{
    [SerializeField] private EventManager eventManager; // イベント再生を依頼する相手
    [SerializeField] private ConsoleManager consoleManager; // VFSインスタンス取得用 (VFSの状態を見る場合に備えて)

    // --- ゲーム進行状況フラグ ---
    private bool hasEnteredHome = false; // /home に入ったことがあるか
    private bool isFindUnlocked = false;
    private bool isCatUnlocked = false;
    private bool isMvUnlocked = false;
    private bool isRmUnlocked = false;
    // TODO: ゲームの進行に合わせて必要なフラグを追加

    private VirtualFileSystem vfs;

    void Start()
    {
        // ConsoleManagerからVFSインスタンスを取得
        if (consoleManager != null && consoleManager.CommandProcessorInstance != null)
        {
             vfs = consoleManager.CommandProcessorInstance.VfsInstance;
        }
        else
        {
            Debug.LogError("CommandEventInterpreter: ConsoleManager or CommandProcessorInstance is not assigned!", this);
        }

        if (eventManager == null)
        {
             Debug.LogError("CommandEventInterpreter: EventManager is not assigned!", this);
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

    /// <summary>
    /// GameEventsから放送されたコマンド結果を受け取り、イベント発生条件を判定するメソッド。
    /// </summary>
    private void OnCommandResultReceived(CommandResult result)
    {
        if (eventManager == null || vfs == null) return;
        if (result.IsError) return; // エラー時はイベントを発生させない

        // --- 各イベントの条件判定を独立させる ---

        // EnterHomeイベントの条件判定 (最優先)
        if (!hasEnteredHome && result.CommandExecuted == "cd" && vfs.CurrentDirectory.Name == "home")
        {
            hasEnteredHome = true;
            Debug.Log("Event Triggered: EnterHome");
            eventManager.EnqueueEvent("EnterHome");
            return; // EnterHomeが発生したら、このフレームでは他のイベントはチェックしない
        }

        // findコマンド解放イベントの条件判定
        // 条件: findが未解放 & cpコマンド成功 & 対象が"find" & 現在地が/home & EnterHome済み
        if (!isFindUnlocked && hasEnteredHome && result.CommandExecuted == "cp"
            && result.TargetNode != null && result.TargetNode.Name == "find"
            && vfs.CurrentDirectory.Name == "home")
        {
            isFindUnlocked = true;
            Debug.Log("Event Triggered: FindUnlocked");
            eventManager.EnqueueEvent("FindUnlocked");
            return; // イベントが発生したら他はチェックしない
        }

        // catコマンド解放イベントの条件判定
        // 条件: catが未解放 & cpコマンド成功 & 対象が"cat" & 現在地が/core & EnterHome済み
        if (!isCatUnlocked && hasEnteredHome && result.CommandExecuted == "cp"
            && result.TargetNode != null && result.TargetNode.Name == "cat"
            && vfs.CurrentDirectory.Name == "core")
        {
            isCatUnlocked = true;
            Debug.Log("Event Triggered: CatUnlocked");
            eventManager.EnqueueEvent("CatUnlocked");
            return; // イベントが発生したら他はチェックしない
        }

        // mvコマンド解放イベントの条件判定
        // 条件: mvが未解放 & cpコマンド成功 & 対象が"mv" & 現在地が/secret & EnterHome済み
        if (!isMvUnlocked && hasEnteredHome && result.CommandExecuted == "cp"
            && result.TargetNode != null && result.TargetNode.Name == "mv"
            && vfs.CurrentDirectory.Name == "secret")
        {
             isMvUnlocked = true;
             Debug.Log("Event Triggered: MvUnlocked");
             eventManager.EnqueueEvent("MvUnlocked");
             return; // イベントが発生したら他はチェックしない
        }

        // rmコマンド解放イベントの条件判定
        // 条件: rmが未解放 & cpコマンド成功 & 対象が"rm" & 現在地が/rock & EnterHome済み
         if (!isRmUnlocked && hasEnteredHome && result.CommandExecuted == "cp"
            && result.TargetNode != null && result.TargetNode.Name == "rm"
            && vfs.CurrentDirectory.Name == "rock")
        {
             isRmUnlocked = true;
             Debug.Log("Event Triggered: RmUnlocked");
             eventManager.EnqueueEvent("RmUnlocked");
             return; // イベントが発生したら他はチェックしない
        }

         // TODO: 他のゲームイベントの条件判定をここに追加 (それぞれ独立したif文として)
    }
}

