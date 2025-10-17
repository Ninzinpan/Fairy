using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections;
using Events;
/// <summary>
/// PowerShell風の単一テキストエリアでコンソールを管理するクラス。
/// GameEventsを購読し、結果を画面に表示する（Subscriber）。
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
    private string prompt = ">";
    [SerializeField]
    private char cursorChar = '_';
    [SerializeField]
    private float cursorBlinkRate = 0.5f;

    // CommandProcessorのインスタンスを保持するように修正
    private CommandProcessor commandProcessor;
    private PlayerControls playerControls;

    private readonly StringBuilder currentInput = new StringBuilder();
    public CommandProcessor CommandProcessorInstance => commandProcessor;

    private bool isInputActive = true;
    private float cursorTimer;
    private bool isCursorVisible;

    void Awake()
    {
        // CommandProcessorはゲーム開始時に一度だけ生成する
        commandProcessor = new CommandProcessor();
        playerControls = new PlayerControls();
    }

    void OnEnable()
    {
        // --- GameEventsの放送を購読する ---
        GameEvents.OnCommandExecuted += OnCommandResultReceived;

        playerControls.Console.Enable();
        playerControls.Console.Submit.performed += OnSubmitCommand;
        playerControls.Console.Backspace.performed += OnBackspace; 
        Keyboard.current.onTextInput += OnTextInput;
    }

    void OnDisable()
    {
        // --- 必ず購読を解除する ---
        GameEvents.OnCommandExecuted -= OnCommandResultReceived;

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
        if (isInputActive)
        {
            cursorTimer += Time.deltaTime;
            if (cursorTimer >= cursorBlinkRate)
            {
                cursorTimer = 0;
                isCursorVisible = !isCursorVisible;
                UpdateInputLineDisplay(); 
            }
        }
    }
    
    /// <summary>
    /// GameEventsからコマンド実行結果が放送されてきたときに呼び出されるメソッド。
    /// </summary>
    private void OnCommandResultReceived(CommandResult result)
    {
        /*
        // 現在のテキストから最後の行（入力行）を一旦削除
        int lastNewLine = consoleText.text.LastIndexOf('\n');
        if (lastNewLine != -1)
        {
            consoleText.text = consoleText.text.Substring(0, lastNewLine + 1);
        }
        else
        {
            consoleText.text = ""; // 履歴がない場合
        }
        */
        
        // 結果が空でなければ、現在のテキストの末尾に追記する
        if (!string.IsNullOrEmpty(result.ConsoleOutput))
        {
            // 末尾の改行が重複しないようにTrimEnd()する
            consoleText.text += result.ConsoleOutput.TrimEnd() + "\n";
        }
        
        // ★修正点: 新しい入力行の表示準備は、結果を受け取った後に行う
        UpdateInputLineDisplay();
        
        // 新しい行が追加されたので、一番下までスクロールする
        StartCoroutine(ScrollToBottom());
    }

    private void OnTextInput(char character)
    {
        if (!isInputActive || char.IsControl(character)) return;
        
        currentInput.Append(character);
        ResetCursorBlink();
        UpdateInputLineDisplay();
    }
    
    private void OnBackspace(InputAction.CallbackContext context)
    {
        if (!isInputActive || currentInput.Length <= 0) return;
        
        currentInput.Length--;
        ResetCursorBlink();
        UpdateInputLineDisplay();
    }

    private void OnSubmitCommand(InputAction.CallbackContext context)
    {
        if (!isInputActive) return;
        
        string command = currentInput.ToString();

        // ★修正点: 表示されている入力行を、履歴として確定させる
        // 現在のテキストから最後の行（入力行）を一旦削除
        int lastNewLine = consoleText.text.LastIndexOf('\n');
        if (lastNewLine != -1)
        {
            consoleText.text = consoleText.text.Substring(0, lastNewLine + 1);
        }
        else
        {
            consoleText.text = "";
        }
        // 入力したコマンド自体を画面に表示する
        consoleText.text += prompt + command + "\n";
        
        // 入力欄をクリア
        currentInput.Clear();
        
        // ★修正点: 毎回生成するのではなく、保持しているインスタンスのメソッドを呼ぶ
        commandProcessor.Process(command);
        
        ResetCursorBlink();
        // ★修正点: ここでは入力行の更新は行わない (OnCommandResultReceivedに任せる)
        // UpdateInputLineDisplay();
        // StartCoroutine(ScrollToBottom());
    }

    /// <summary>
    /// 現在入力中の行の表示だけを更新する。
    /// </summary>
    private void UpdateInputLineDisplay()
    {
        // 現在のテキストから最後の行（入力行）を一旦削除
        int lastNewLine = consoleText.text.LastIndexOf('\n');
        if (lastNewLine != -1)
        {
            // 最後のプロンプト行より前の部分を取得
            consoleText.text = consoleText.text.Substring(0, lastNewLine + 1);
        }
        else
        {
            consoleText.text = ""; // 履歴がない場合
        }

        // 新しい入力行を組み立てて追加
        var inputLine = new StringBuilder();
        inputLine.Append(prompt);
        inputLine.Append(currentInput);
        
        if (isInputActive && isCursorVisible)
        {
            inputLine.Append(cursorChar);
        }
        
        consoleText.text += inputLine.ToString();
    }


    private void ResetCursorBlink()
    {
        isCursorVisible = true;
        cursorTimer = 0;
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void DeactivateInput() => isInputActive = false;
    public void ActivateInput() => isInputActive = true;
}

