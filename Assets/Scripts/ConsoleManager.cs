using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections;
using System.Linq; // Splitなどを使うために必要
using Events; // CommandResultを使うために必要

/// <summary>
/// PowerShell風の単一テキストエリアでコンソールを管理するクラス。
/// Manages the console using a single text area like PowerShell.
/// </summary>
public class ConsoleManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Text consoleText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Console Settings")]
    [SerializeField] private string prompt = ">";
    [SerializeField] private char cursorChar = '_';
    [SerializeField] private float cursorBlinkRate = 0.5f;

    // ★★★ InputCensorへの参照を追加 ★★★
    [Header("Dependencies")]
    [SerializeField] private InputCensor inputCensor;

    // CommandProcessorは内部で持ち、外部公開用プロパティを用意
    private readonly CommandProcessor commandProcessor = new CommandProcessor();
    public CommandProcessor CommandProcessorInstance => commandProcessor;

    private PlayerControls playerControls;
    private readonly StringBuilder consoleHistory = new StringBuilder();
    private readonly StringBuilder currentInput = new StringBuilder();
    private bool isInputActive = true;
    private float cursorTimer;
    private bool isCursorVisible;

    // イベント中にEnterキー入力を待つためのフラグ
    private bool waitingForEventInput = false;

    void Awake()
    {
        playerControls = new PlayerControls();
        // InputCensorがnullでないか確認 (インスペクター設定忘れ防止)
        if (inputCensor == null)
        {
            Debug.LogError("ConsoleManager: InputCensor is not assigned in the inspector!", this);
        }
    }

    void OnEnable()
    {
        playerControls.Console.Enable();
        playerControls.Console.Submit.performed += OnSubmitCommand;
        playerControls.Console.Backspace.performed += OnBackspace;
        Keyboard.current.onTextInput += OnTextInput;
        GameEvents.OnCommandExecuted += OnCommandResultReceived; // 結果表示のために購読
    }

    void OnDisable()
    {
        playerControls.Console.Submit.performed -= OnSubmitCommand;
        playerControls.Console.Backspace.performed -= OnBackspace;
        playerControls.Console.Disable();

        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
        GameEvents.OnCommandExecuted -= OnCommandResultReceived;
    }

    void Update()
    {
        // 通常の入力時のみカーソル点滅
        if (isInputActive)
        {
            cursorTimer += Time.deltaTime;
            if (cursorTimer >= cursorBlinkRate)
            {
                cursorTimer = 0;
                isCursorVisible = !isCursorVisible;
                UpdateInputLineDisplay(); // カーソル点滅のために再描画
            }
        }
    }

    private void OnTextInput(char character)
    {
        // イベント中は通常の文字入力を無視
        if (!isInputActive || waitingForEventInput || char.IsControl(character)) return;
        currentInput.Append(character);
        ResetCursorBlink();
        UpdateInputLineDisplay();
    }

    private void OnBackspace(InputAction.CallbackContext context)
    {
         // イベント中はBackspaceを無視
        if (!isInputActive || waitingForEventInput || currentInput.Length <= 0) return;
        currentInput.Length--;
        ResetCursorBlink();
        UpdateInputLineDisplay();
    }

    /// <summary>
    /// Enterキーが押された時の処理。
    /// イベント入力待ちか、通常のコマンド入力かで処理を分岐。
    /// </summary>
    private void OnSubmitCommand(InputAction.CallbackContext context)
    {
        // イベント入力待機中なら、フラグを解除するだけ
        if (waitingForEventInput)
        {
            waitingForEventInput = false;
            return;
        }

        // 通常のコマンド入力処理
        if (!isInputActive || inputCensor == null) return;

        string commandLine = currentInput.ToString();

        consoleHistory.Append(prompt).Append(commandLine).Append("\n");
        ClearInputLine();
        UpdateConsoleHistoryDisplay();

        if (string.IsNullOrWhiteSpace(commandLine))
        {
             StartCoroutine(ScrollToBottom());
             ResetCursorBlink();
             UpdateInputLineDisplay();
            return;
        }

        inputCensor.ProcessInput(commandLine);
    }

    private void OnCommandResultReceived(CommandResult result)
    {
        // イベント再生中はコマンド結果を追加しない（イベントテキスト表示に任せる）
        if (waitingForEventInput) return;

        if (!string.IsNullOrEmpty(result.ConsoleOutput))
        {
            consoleHistory.Append(result.ConsoleOutput).Append("\n");
        }
        ResetCursorBlink();
        UpdateConsoleHistoryDisplay();
        UpdateInputLineDisplay();
        StartCoroutine(ScrollToBottom());
    }

    private void UpdateConsoleHistoryDisplay()
    {
         consoleText.text = consoleHistory.ToString();
    }

    private void UpdateInputLineDisplay()
    {
        var displayText = new StringBuilder(consoleHistory.ToString());
        displayText.Append(prompt);
        displayText.Append(currentInput);
        // 通常入力時のみカーソル表示
        if (isInputActive && isCursorVisible && !waitingForEventInput)
        {
            displayText.Append(cursorChar);
        }
        consoleText.text = displayText.ToString();
    }

    private void ClearInputLine()
    {
        currentInput.Clear();
    }

    private void ResetCursorBlink()
    {
        isCursorVisible = true;
        cursorTimer = 0;
    }

    private IEnumerator ScrollToBottom()
    {
        // UIレイアウトが更新されるのを待つ
        yield return null; // 1フレーム待つだけで十分な場合が多い
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // --- EventManagerから呼ばれるメソッド ---
    public void ActivateInput()
    {
        isInputActive = true;
        waitingForEventInput = false; // イベント入力待機も解除
        ResetCursorBlink();
        UpdateInputLineDisplay();
        StartCoroutine(ScrollToBottom()); // ★★★ イベント終了後にもスクロールを実行 ★★★
    }

    public void DeactivateInput()
    {
        isInputActive = false;
        waitingForEventInput = false; // 念のため
        UpdateInputLineDisplay(); // カーソルを消す
    }

    /// <summary>
    /// イベントテキストを1行ずつ表示し、Enterキー入力を待つコルーチン。
    /// </summary>
    public IEnumerator ShowEventTextCoroutine(string message)
    {
        // 入力行を消して履歴だけ表示
        UpdateConsoleHistoryDisplay();

        // メッセージを改行で分割
        string[] lines = message.Split('\n');

        foreach (string line in lines)
        {
            // 1行追加して表示
            consoleHistory.Append(line).Append("\n");
            consoleText.text = consoleHistory.ToString();
            yield return ScrollToBottom(); // スクロール完了を待つ

            // Enterキー入力を待つ
            waitingForEventInput = true;
            // waitingForEventInputがfalseになるまで待機
            yield return new WaitUntil(() => !waitingForEventInput);
        }
        // イベントテキスト表示後は、自動で入力は再開しない
        // EventManagerがActivateInput()を呼ぶ責任を持つ
    }

    // EventManagerからの会話表示用 (待機なし、プロンプトなし)
    public void ShowDialogueText(string text)
    {
         consoleHistory.Append(text).Append("\n");
         UpdateConsoleHistoryDisplay();
         // イベント中は入力行を表示しないのでUpdateInputLineDisplayは呼ばない
         StartCoroutine(ScrollToBottom());
    }
}

