using System;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class ShowKeyboard : MonoBehaviour
{
    private TMP_InputField inputField;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();

        inputField.onSelect.AddListener(x => OpenKeyboard());

        // Listen for final submitted text
        NonNativeKeyboard.Instance.OnTextSubmitted += OnKeyboardSubmit;
    }

    public void OpenKeyboard()
    {
        // ✅ THIS enables typing into the field
        NonNativeKeyboard.Instance.InputField = inputField;

        NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);

        inputField.ActivateInputField();
    }

    private void OnKeyboardSubmit(object sender, EventArgs e)
    {
        // ✅ Get final text from keyboard
        string text = NonNativeKeyboard.Instance.InputField.text;

        string cleaned = text.Trim();

        Debug.Log("Submitted:" + cleaned );

        // ✅ Ensure field updates visually
        inputField.text = cleaned;
    }

    private void OnDestroy()
    {
        if (NonNativeKeyboard.Instance != null)
        {
            NonNativeKeyboard.Instance.OnTextSubmitted -= OnKeyboardSubmit;
        }
    }
}