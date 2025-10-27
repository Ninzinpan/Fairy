using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections;
using Events; // CommandResult, GameEventsを使うために必要

/// <summary>
/// PowerShell風の単一テキストエリアでコンソールを管理するクラス。
/// イベントシステムからの指示に応じて入力制御やテキスト表示も行う。
/// </summary>
public class ConsoleManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField]
    private TMP_Text consoleText;
    [SerializeField]
    private ScrollRect scrollRect;

    [Header("Console Settings")]
    [SerializeField]
    private string prompt = "> "; // プロンプトの後ろにスペースを追加
    [SerializeField]
    private char cursorChar = '_';
    [SerializeField]
    private float cursorBlinkRate = 0.5f;

    // CommandProcessorのインスタンスを保持
    private readonly CommandProcessor commandProcessor = new CommandProcessor(); // readonlyに変更
    private PlayerControls playerControls;

    // 履歴と現在の入力をStringBuilderで管理
    private readonly StringBuilder consoleHistory = new StringBuilder();
    private readonly StringBuilder currentInput = new StringBuilder();
    public CommandProcessor CommandProcessorInstance => commandProcessor;

    private bool isInputActive = true; // 通常のコマンド入力が有効か
    private float cursorTimer;
    private bool isCursorVisible;
    private bool isWaitingForEventInput = false; // イベント進行のEnter待ちフラグ

    void Awake()
    {
        // commandProcessor は readonly なので Awake で new する
        playerControls = new PlayerControls();
    }

    void OnEnable()
    {
        GameEvents.OnCommandExecuted += OnCommandResultReceived;

        playerControls.Console.Enable();
        playerControls.Console.Submit.performed += OnSubmitPressed; // ハンドラ変更
        playerControls.Console.Backspace.performed += OnBackspace;
        Keyboard.current.onTextInput += OnTextInput;
    }

    void OnDisable()
    {
        GameEvents.OnCommandExecuted -= OnCommandResultReceived;

        // OnEnableで登録したものを解除
        if (playerControls != null)
        {
             playerControls.Console.Submit.performed -= OnSubmitPressed; // ハンドラ変更
             playerControls.Console.Backspace.performed -= OnBackspace;
             playerControls.Console.Disable();
        }

        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
    }

    void Update()
    {
        // 通常入力モードの時だけカーソルを点滅
        if (isInputActive)
        {
            cursorTimer += Time.deltaTime;
            if (cursorTimer >= cursorBlinkRate)
            {
                cursorTimer = 0;
                isCursorVisible = !isCursorVisible;
                UpdateConsoleText(); // 表示更新
            }
        }
    }

    /// <summary>
    /// GameEventsからコマンド実行結果を受け取る
    /// </summary>
    private void OnCommandResultReceived(CommandResult result)
    {
        // 結果が空でなければ履歴に追加
        if (!string.IsNullOrEmpty(result.ConsoleOutput))
        {
            AddToHistory(result.ConsoleOutput.TrimEnd());
        }

        UpdateConsoleText(); // 表示更新
        ScrollToBottom(); // スクロール
    }

    private void OnTextInput(char character)
    {
        if (!isInputActive || char.IsControl(character)) return;

        currentInput.Append(character);
        ResetCursorBlink();
        UpdateConsoleText();
    }

    private void OnBackspace(InputAction.CallbackContext context)
    {
        if (!isInputActive || currentInput.Length <= 0) return;

        currentInput.Length--;
        ResetCursorBlink();
        UpdateConsoleText();
    }

    /// <summary>
    /// Submitアクション（Enterキー）が押されたときの処理を振り分ける
    /// </summary>
    private void OnSubmitPressed(InputAction.CallbackContext context)
    {
        if (isWaitingForEventInput)
        {
            // イベント進行待ちの場合、フラグを折るだけ
            isWaitingForEventInput = false;
        }
        else if (isInputActive)
        {
            // 通常のコマンド入力の場合
            SubmitCommand();
        }
    }

    /// <summary>
    /// 現在の入力内容でコマンドを実行する
    /// </summary>
    private void SubmitCommand()
    {
        string command = currentInput.ToString();
        if (string.IsNullOrWhiteSpace(command)) return;

        // 履歴に自分の入力を追加
        AddToHistory(prompt + command);

        // 入力欄をクリア
        ClearInputLine();

        // CommandProcessorに処理を依頼
        commandProcessor.Process(command); // VFSが結果をイベントで放送する

        // 表示更新とスクロール
        UpdateConsoleText();
        ScrollToBottom();

        ResetCursorBlink();
    }

    /// <summary>
    /// イベントテキストを改行区切りで表示し、Enterキーで次に進めるコルーチン
    /// </summary>
    public IEnumerator ShowEventTextCoroutine(string message)
    {
        UpdateConsoleText(isEvent: true); // 入力行を非表示にする

        string[] lines = message.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            AddToHistory(line); // 1行履歴に追加
            UpdateConsoleText(isEvent: true); // 表示更新 (イベント中)
            ScrollToBottom(); // スクロール

            // Enterキーが押されるまで待機
            isWaitingForEventInput = true;
            yield return new WaitUntil(() => !isWaitingForEventInput);
        }
    }

    /// <summary>
    /// 通常のコマンド入力を有効にする
    /// </summary>
    public void ActivateInput()
    {
        isInputActive = true;
        ResetCursorBlink();
        UpdateConsoleText(); // 入力行を表示
    }

    /// <summary>
    /// 通常のコマンド入力を無効にする
    /// </summary>
    public void DeactivateInput()
    {
        isInputActive = false;
        isCursorVisible = false; // カーソル非表示
        UpdateConsoleText(isEvent: true); // 入力行を非表示
    }

    /// <summary>
    /// コンソール全体のテキスト表示を更新する。
    /// </summary>
    /// <param name="isEvent">イベント表示中か</param>
    private void UpdateConsoleText(bool isEvent = false)
    {
        var displayText = new StringBuilder();
        displayText.Append(consoleHistory); // 履歴

        // 通常入力モードの場合のみプロンプトと入力行を表示
        if (!isEvent && isInputActive)
        {
             displayText.Append(prompt);
             displayText.Append(currentInput);
             if (isCursorVisible) // カーソル
             {
                 displayText.Append(cursorChar);
             }
        }

        consoleText.text = displayText.ToString();
    }


    private void ResetCursorBlink()
    {
        isCursorVisible = true;
        cursorTimer = 0;
    }

    private void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomCoroutine());
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
         // Wait for end of frame ensures layout is updated before scrolling
         yield return new WaitForEndOfFrame();
         if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    // --- 履歴と入力行の内部管理用ヘルパー ---
    private void AddToHistory(string line)
    {
        consoleHistory.Append(line).Append("\n");
    }

    private void ClearInputLine()
    {
        currentInput.Clear();
    }
}

