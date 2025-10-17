using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // ScrollRect を使うために必要
using TMPro;
using System.Text;
using System.Collections; // Coroutine を使うために必要

/// <summary>
/// PowerShell風の単一テキストエリアでコンソールを管理するクラス。
/// </summary>
public class ConsoleManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField]
    private TMP_Text consoleText; // 全テキスト表示用のText
    [SerializeField]
    private ScrollRect scrollRect; // 自動スクロール用のScrollRect

    [Header("Console Settings")]
    [SerializeField]
    private string prompt = ">"; // プロンプト記号
    [SerializeField]
    private char cursorChar = '_'; // カーソル文字
    [SerializeField]
    private float cursorBlinkRate = 0.5f; // カーソルの点滅速度

    private CommandProcessor commandProcessor;
    private PlayerControls playerControls;
    
    // --- 内部状態 ---
    private readonly StringBuilder consoleHistory = new StringBuilder();
    private readonly StringBuilder currentInput = new StringBuilder();
    private bool isInputActive = true;
    private float cursorTimer;
    private bool isCursorVisible;

    void Awake()
    {
        commandProcessor = new CommandProcessor();
        playerControls = new PlayerControls();
    }

    void OnEnable()
    {
        playerControls.Console.Enable();
        playerControls.Console.Submit.performed += OnSubmitCommand;
        playerControls.Console.Backspace.performed += OnBackspace; 
        Keyboard.current.onTextInput += OnTextInput;
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
    }
    
    void Update()
    {
        // カーソルの点滅処理
        if (isInputActive)
        {
            cursorTimer += Time.deltaTime;
            if (cursorTimer >= cursorBlinkRate)
            {
                cursorTimer = 0;
                isCursorVisible = !isCursorVisible;
                UpdateConsoleText(); // 点滅のために再描画
            }
        }
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

    private void OnSubmitCommand(InputAction.CallbackContext context)
    {
        if (!isInputActive) return;
        
        string command = currentInput.ToString();
        
        // 入力内容を履歴に追加
        consoleHistory.Append(prompt).Append(command).Append("\n");
        
        // 入力欄をクリア
        currentInput.Clear();
        
        // コマンドが空でなければ処理を実行
        if (!string.IsNullOrWhiteSpace(command))
        {
            string result = commandProcessor.Process(command);
            if (!string.IsNullOrEmpty(result))
            {
                consoleHistory.Append(result).Append("\n");
            }
        }
        
        ResetCursorBlink();
        UpdateConsoleText();
        StartCoroutine(ScrollToBottom());
    }

    /// <summary>
    /// 表示されているテキスト全体を更新する。
    /// </summary>
    private void UpdateConsoleText()
    {
        var displayText = new StringBuilder();
        displayText.Append(consoleHistory);
        displayText.Append(prompt);
        displayText.Append(currentInput);
        
        // カーソルが点滅表示中の場合のみカーソル文字を追加
        if (isInputActive && isCursorVisible)
        {
            displayText.Append(cursorChar);
        }
        
        consoleText.text = displayText.ToString();
    }

    /// <summary>
    /// カーソルの点滅をリセットし、表示状態にする。
    /// </summary>
    private void ResetCursorBlink()
    {
        isCursorVisible = true;
        cursorTimer = 0;
    }

    /// <summary>
    /// 次のフレームでスクロールビューを一番下に移動させる。
    /// </summary>
    private IEnumerator ScrollToBottom()
    {
        // UIのレイアウトが更新されるのを1フレーム待つ
        yield return new WaitForEndOfFrame();
        
        // 垂直スクロールバーの位置を一番下(0)に設定
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void DeactivateInput() => isInputActive = false;
    public void ActivateInput() => isInputActive = true;
}

