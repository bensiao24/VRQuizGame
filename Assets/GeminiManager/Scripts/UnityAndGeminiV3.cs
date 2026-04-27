using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

[System.Serializable]
public class UnityAndGeminiKey
{
    public string key;
}

[System.Serializable]
public class TextPart
{
    public string text;
}

[System.Serializable]
public class TextContent
{
    public string role;
    public TextPart[] parts;
}

[System.Serializable]
public class TextCandidate
{
    public TextContent content;
}

[System.Serializable]
public class TextResponse
{
    public TextCandidate[] candidates;
}

[System.Serializable]
public class ChatRequest
{
    public TextContent[] contents;
}

public class UnityAndGeminiV3 : MonoBehaviour
{
    [Header("API Config")]
    public TextAsset jsonApi;
    private string apiKey = "";
    private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    [Header("UI")]
    public TMP_InputField inputField;
    public TMP_Text uiText;

    void Start()
    {
        // Load API key
        UnityAndGeminiKey jsonApiKey = JsonUtility.FromJson<UnityAndGeminiKey>(jsonApi.text);
        apiKey = jsonApiKey.key;

        // Ensure input field exists
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();

        // Open keyboard on select
        inputField.onSelect.AddListener(x => OpenKeyboard());

        // Wait for keyboard instance
        StartCoroutine(SetupKeyboard());
    }

    private IEnumerator SetupKeyboard()
    {
        while (NonNativeKeyboard.Instance == null)
        {
            yield return null;
        }

        NonNativeKeyboard.Instance.OnTextSubmitted += OnKeyboardSubmit;
    }

    public void OpenKeyboard()
    {
        if (NonNativeKeyboard.Instance == null)
        {
            Debug.LogError("Keyboard instance not found in scene.");
            return;
        }

        NonNativeKeyboard.Instance.InputField = inputField;
        NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);

        inputField.ActivateInputField();
    }

    private void OnKeyboardSubmit(object sender, EventArgs e)
    {
        string userInput = NonNativeKeyboard.Instance.InputField.text.Trim();

        Debug.Log("User input: " + userInput);

        if (string.IsNullOrEmpty(userInput))
        {
            Debug.LogWarning("Input is empty!");
            return;
        }

        string prompt = "Generate exactly 10 multiple choice questions about " + userInput + ". " +
"Each question must be on ONE LINE using this EXACT format:\n" +
"Question|Option1|Option2|Option3|Option4|CorrectAnswer\n\n" +
"Rules:\n" +
"- Do NOT number the questions\n" +
"- Do NOT add extra text or explanations\n" +
"- Do NOT add blank lines\n" +
"- The correct answer MUST match one of the options exactly\n" +
"- Output ONLY the questions in this format\n\n" +
"Example:\n" +
"What is 2+2?|1|2|3|4|4";

        StartCoroutine(SendChatRequestToGemini(prompt));
    }

    private IEnumerator SendChatRequestToGemini(string promptText)
    {
        string url = $"{apiEndpoint}?key={apiKey}";

        TextContent userContent = new TextContent
        {
            role = "user",
            parts = new TextPart[]
            {
                new TextPart { text = promptText }
            }
        };

        ChatRequest chatRequest = new ChatRequest
        {
            contents = new TextContent[] { userContent }
        };

        string jsonData = JsonUtility.ToJson(chatRequest);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Request failed: " + www.error);
                yield break;
            }

            Debug.Log("FULL RESPONSE: " + www.downloadHandler.text);

            TextResponse response = JsonUtility.FromJson<TextResponse>(www.downloadHandler.text);

            if (response == null)
            {
                Debug.LogError("Response is NULL (JSON parse failed)");
                yield break;
            }

            if (response.candidates == null || response.candidates.Length == 0)
            {
                Debug.LogError("No candidates returned from API");
                yield break;
            }

            if (response.candidates[0].content == null ||
                response.candidates[0].content.parts == null ||
                response.candidates[0].content.parts.Length == 0)
            {
                Debug.LogError("Invalid content structure in response");
                yield break;
            }

            string reply = response.candidates[0].content.parts[0].text;

            Debug.Log("AI Reply: " + reply);

            // UI output
            if (uiText != null)
                uiText.text = reply;

            // ✅ Save to Assets/Resources/questions.txt
            string filePath = Path.Combine(Application.dataPath, "Resources/AIquestions.txt");
            File.WriteAllText(filePath, reply);

            Debug.Log("Saved to: " + filePath);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }

    private void OnDestroy()
    {
        if (NonNativeKeyboard.Instance != null)
        {
            NonNativeKeyboard.Instance.OnTextSubmitted -= OnKeyboardSubmit;
        }
    }
}