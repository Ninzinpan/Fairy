using System; // Action を使うために必要

/// <summary>
/// ゲーム全体のグローバルなイベントを管理する静的クラス。
/// 「店内放送のスピーカー」の役割を果たす。
/// A static class to manage global game events. Acts as the "broadcasting system".
/// </summary>
public static class GameEvents
{
    /// <summary>
    /// コマンドが実行されたときに発行されるイベント。
    /// CommandResultを購読者（Subscriber）に渡す。
    /// Event triggered when a command is executed. Passes the CommandResult to subscribers.
    /// </summary>
    public static event Action<CommandResult> OnCommandExecuted;

    /// <summary>
    /// OnCommandExecutedイベントを発行（トリガー）する。
    /// Publisher（CommandProcessor）がこのメソッドを呼び出す。
    /// Triggers the OnCommandExecuted event. Called by the publisher (CommandProcessor).
    /// </summary>
    /// <param name="result">放送するコマンド結果 (The command result to broadcast)</param>
    public static void TriggerCommandExecuted(CommandResult result)
    {
        // 誰もイベントを購読していなくてもエラーにならないように、nullチェックを行う
        // Perform a null check to prevent errors if no one is subscribed to the event.
        OnCommandExecuted?.Invoke(result);
    }
}
