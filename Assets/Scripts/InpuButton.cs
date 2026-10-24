using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputButton : MonoBehaviour
{
    [Header("Компоненты")]
    public UnityEngine.UI.Button button;
    public TMP_InputField inputField;

    void Start()
    {
        
        button.onClick.AddListener(OnButtonClick);

     
        inputField.onSubmit.AddListener(OnSubmit);
    }

    void OnButtonClick()
    {
        inputField.ActivateInputField();
    }

    void OnSubmit(string value)
    {
        // Заглушка
        Debug.Log($"[BUTTON] Отправлено: {value}");

    }
}


