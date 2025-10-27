using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ToLower() などを使うために必要
using Events;     // CommandResult, GameEvents を使うために必要

/// <summary>
/// ユーザーからのコマンド入力を受け取り、実行可否を判断するクラス。
/// Receives user command input and determines whether it can be executed.
/// </summary>
public class InputCensor : MonoBehaviour
{
    // ConsoleManagerへの参照をインスペクターから設定
    [SerializeField] private ConsoleManager consoleManager;

    // CommandProcessorのインスタンスを保持する変数
    private CommandProcessor commandProcessorInstance;

    // 現在許可されているコマンド名のリスト（小文字で格納）
    private HashSet<string> allowedCommands = new HashSet<string>();
    // 現在ブロックされているコマンド名、または「コマンド 引数」パターンのリスト（小文字で格納）
    private HashSet<string> blockedCommands = new HashSet<string>();

    void Awake()
    {
        // 基本コマンドの許可
        allowedCommands.Add("echo");
        allowedCommands.Add("ls");
        allowedCommands.Add("cd");
        allowedCommands.Add("cp");
        // 他のコマンドは最初はロックされている想定
    }

    void Start()
    {
        // ConsoleManager経由でCommandProcessorインスタンスを取得
        if (consoleManager == null)
        {
            Debug.LogError("InputCensor: ConsoleManager is not assigned in the inspector!", this);
            return;
        }
        commandProcessorInstance = consoleManager.CommandProcessorInstance;
        if (commandProcessorInstance == null)
        {
             Debug.LogError("InputCensor: Could not get CommandProcessor instance from ConsoleManager!", this);
        }
    }


    public void ProcessInput(string inputLine)
    {
        // commandProcessorInstanceが取得できているかチェック
        if (commandProcessorInstance == null)
        {
             Debug.LogError("InputCensor: CommandProcessor instance is null. Cannot process input.", this);
             GameEvents.TriggerCommandExecuted(new CommandResult("Internal Error: Input processing unavailable.", isError: true));
             return;
        }

        string trimmedInput = inputLine.Trim();
        if (string.IsNullOrEmpty(trimmedInput)) return;

        string[] parts = trimmedInput.Split(' ');
        string command = parts[0].ToLower();
        List<string> arguments = parts.Skip(1).ToList();

        // 1. ブロックリストのチェック
        if (blockedCommands.Contains(command))
        {
            GameEvents.TriggerCommandExecuted(new CommandResult($"Command '{parts[0]}' is currently blocked.", isError: true, commandExecuted: command));
            return;
        }
        if (arguments.Count > 0)
        {
            string pattern = command + " " + arguments[0].ToLower();
            if (blockedCommands.Contains(pattern))
            {
                 GameEvents.TriggerCommandExecuted(new CommandResult($"Command execution blocked: {parts[0]} {arguments[0]}", isError: true, commandExecuted: command));
                 return;
            }
        }

        // 2. 許可リストのチェック
        if (!allowedCommands.Contains(command))
    {
            
            GameEvents.TriggerCommandExecuted(new CommandResult($"command not found: {parts[0]}", isError: true, commandExecuted: command));
            return;
        }

        // 検閲通過: commandProcessorInstance を使用
        commandProcessorInstance.Process(trimmedInput);
    }

    // --- 外部からルールを変更するためのメソッド ---

    /// <summary>
    /// 指定されたコマンドを許可リストに追加します。
    /// Adds the specified command to the allowed list.
    /// </summary>
    /// <param name="command">許可するコマンド名</param>
    public void AddAllowedCommand(string command)
    {
        if (!string.IsNullOrEmpty(command))
        {
            string lowerCommand = command.ToLower();
            if (allowedCommands.Add(lowerCommand)) // Add returns true if the item was added
            {
                Debug.Log($"[InputCensor] Command unlocked: {lowerCommand}");
            }
        }
    }

    /// <summary>
    /// 指定されたコマンドを許可リストから削除します。
    /// Removes the specified command from the allowed list.
    /// </summary>
    /// <param name="command">削除するコマンド名</param>
    public void RemoveAllowedCommand(string command)
    {
         if (!string.IsNullOrEmpty(command))
         {
             string lowerCommand = command.ToLower();
             if (allowedCommands.Remove(lowerCommand)) // Remove returns true if the item was removed
             {
                 Debug.Log($"[InputCensor] Command locked: {lowerCommand}");
             }
         }
    }

    /// <summary>
    /// 指定されたコマンドまたはパターンをブロックリストに追加します。
    /// Adds the specified command or pattern to the blocked list.
    /// </summary>
    /// <param name="commandOrPattern">ブロックするコマンド名または "コマンド 引数" パターン</param>
    public void AddBlockedCommand(string commandOrPattern)
    {
         if (!string.IsNullOrEmpty(commandOrPattern))
         {
             string lowerPattern = commandOrPattern.ToLower();
             if (blockedCommands.Add(lowerPattern))
             {
                 Debug.Log($"[InputCensor] Command blocked: {lowerPattern}");
             }
         }
    }

    /// <summary>
    /// 指定されたコマンドまたはパターンをブロックリストから削除します。
    /// Removes the specified command or pattern from the blocked list.
    /// </summary>
    /// <param name="commandOrPattern">ブロックを解除するコマンド名またはパターン</param>
    public void RemoveBlockedCommand(string commandOrPattern)
    {
         if (!string.IsNullOrEmpty(commandOrPattern))
         {
             string lowerPattern = commandOrPattern.ToLower();
             if (blockedCommands.Remove(lowerPattern))
             {
                 Debug.Log($"[InputCensor] Command unblocked: {lowerPattern}");
             }
         }
    }
}

