using UnityEngine;
using UnityEngine.InputSystem; // Input System を使うために必要
using TMPro;                 // TextMeshPro を使うために必要
using System.Text;           // StringBuilder を使うために必要

/// <summary>
/// UIとコマンド処理を仲介し、自前でキーボード入力を処理するクラス。
/// </summary>
public class ConsoleManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField]
    private TMP_Text consoleOutputText; // 結果表示用のText
    [SerializeField]
    private TMP_Text inputLineText;     // 現在入力中の行を表示するText

    private CommandProcessor commandProcessor;
    private PlayerControls playerControls;
    
    // --- 自作入力欄のための変数 ---
    private StringBuilder currentInput = new StringBuilder(); // 入力中の文字列を保持
    private bool isInputActive = true; // 入力を受け付けるかどうかのフラグ

    void Awake()
    {
        commandProcessor = new CommandProcessor();
        playerControls = new PlayerControls();
    }

    void OnEnable()
    {
        // --- Input Systemのイベント登録 ---
        playerControls.Console.Enable();
        playerControls.Console.Submit.performed += OnSubmitCommand;
        // Backspaceアクションの登録（PlayerControlsアセットに要追加）
        playerControls.Console.Backspace.performed += OnBackspace; 
        
        // 文字入力イベントの登録
        Keyboard.current.onTextInput += OnTextInput;
    }

    void OnDisable()
    {
        // --- 登録したイベントの解除 ---
        playerControls.Console.Submit.performed -= OnSubmitCommand;
        playerControls.Console.Backspace.performed -= OnBackspace;
        playerControls.Console.Disable();

        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
    }

    /// <summary>
    /// 文字キーが入力された時にInput Systemから呼ばれる。
    /// </summary>
    private void OnTextInput(char character)
    {
        if (!isInputActive) return;

        // BackspaceやEnterなどの制御文字は無視する
        if (char.IsControl(character)) return;
        
        currentInput.Append(character);
        UpdateInputLineText();
    }
    
    /// <summary>
    /// Backspaceキーが押された時にInput Systemから呼ばれる。
    /// </summary>
    private void OnBackspace(InputAction.CallbackContext context)
    {
        if (!isInputActive) return;

        if (currentInput.Length > 0)
        {
            currentInput.Length--; // 末尾の1文字を削除
            UpdateInputLineText();
        }
    }

    /// <summary>
    /// Enterキーが押された時にInput Systemから呼ばれる。
    /// </summary>
    private void OnSubmitCommand(InputAction.CallbackContext context)
    {
        if (!isInputActive) return;
        
        string command = currentInput.ToString();

        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        AddToHistory("> " + command);

        string result = commandProcessor.Process(command);

        if (!string.IsNullOrEmpty(result))
        {
            AddToHistory(result);
        }

        ClearInputLine();
    }

    /// <summary>
    /// 外部から呼び出して入力を無効化する。
    /// </summary>
    public void DeactivateInput()
    {
        isInputActive = false;
    }
    
    /// <summary>
    /// 外部から呼び出して入力を有効化する。
    /// </summary>
    public void ActivateInput()
    {
        isInputActive = true;
    }

    private void AddToHistory(string line)
    {
        consoleOutputText.text += line + "\n";
    }



    private void ClearInputLine()
    {
        currentInput.Clear();
        UpdateInputLineText();
    }
    
    private void UpdateInputLineText()
    {
        // 入力中のテキストを画面に反映する
        inputLineText.text = currentInput.ToString();
    }
}

