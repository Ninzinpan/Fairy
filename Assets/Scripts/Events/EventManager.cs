using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// イベントキューを管理し、イベントシーケンスの再生を実行するクラス。
/// Manages the event queue and executes event sequence playback.
/// </summary>
public class EventManager : MonoBehaviour
{
    [SerializeField] private ConsoleManager consoleManager; // 入力制御とテキスト表示用

    private Queue<string> eventQueue = new Queue<string>();
    private bool isPlayingEvent = false;

    void Update()
    {
        // イベントが再生中でなく、キューにイベントがあれば再生を開始
        if (!isPlayingEvent && eventQueue.Count > 0)
        {
            string nextEventId = eventQueue.Dequeue();
            StartCoroutine(PlayEventCoroutine(nextEventId));
        }
    }

    /// <summary>
    /// イベントをキューの末尾に追加します。
    /// Enqueues an event to the end of the queue.
    /// </summary>
    /// <param name="eventId">再生したいイベントのID</param>
    public void EnqueueEvent(string eventId)
    {
        eventQueue.Enqueue(eventId);
    }

    /// <summary>
    /// 指定されたイベントIDに対応するイベントシーケンスを再生するコルーチン。
    /// </summary>
    private IEnumerator PlayEventCoroutine(string eventId)
    {
        isPlayingEvent = true;
        consoleManager.DeactivateInput(); // 入力無効化

        // コマンド結果がコンソールに表示されるのを1フレーム待つ
        yield return new WaitForEndOfFrame();

        Debug.Log($"--- Event Start: {eventId} ---");

        // イベントIDに応じて処理を分岐
        switch (eventId)
        {
            // ★★★ EnterHomeイベントのシーケンスを追加 ★★★
            case "EnterHome":
                yield return consoleManager.ShowEventTextCoroutine("ここは /home ディレクトリ...");
                yield return consoleManager.ShowEventTextCoroutine("攻撃者はここにいくつかのファイルを隠したようだ。\n`ls` コマンドで中身を確認してみよう。");
                break;

            case "FindUnlocked":
                yield return consoleManager.ShowEventTextCoroutine("find コマンドが /bin にコピーされ、使用可能になりました！");
                yield return consoleManager.ShowEventTextCoroutine("使用法: find <filename>");
                break;

            case "CatUnlocked":
                yield return consoleManager.ShowEventTextCoroutine("cat コマンドが /bin にコピーされました。");
                yield return consoleManager.ShowEventTextCoroutine("ファイルの中身を読むことができます。");
                break;
                
             case "MvUnlocked":
                yield return consoleManager.ShowEventTextCoroutine("mv コマンドが /bin にコピーされました。");
                yield return consoleManager.ShowEventTextCoroutine("ファイルやディレクトリの名前を変更できます。");
                break;

             case "RmUnlocked":
                yield return consoleManager.ShowEventTextCoroutine("rm コマンドが /bin にコピーされました。");
                 yield return consoleManager.ShowEventTextCoroutine("ファイルやディレクトリを削除できます...注意して使用してください。");
                break;

            default:
                Debug.LogWarning($"EventManager: 未定義のイベントID '{eventId}' が要求されました。");
                break;
        }

        Debug.Log($"--- Event End: {eventId} ---");

        consoleManager.ActivateInput(); // 入力再開
        isPlayingEvent = false;
    }
}

