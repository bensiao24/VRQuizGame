using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ReadSpeaker;

public class CanvasReader : MonoBehaviour
{
    public TTSSpeaker speaker;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public float delay = 0.5f;

    private int readIndex = -1;
    private float timer = 0f;
    private bool isReading = false;

    void Start()
    {
        TTS.Init();
    }

    public void StartReading()
    {
        if (speaker == null || questionText == null)
        {
            Debug.LogWarning("Missing references in CanvasReader");
            return;
        }

        TTS.InterruptAll();

        readIndex = -1;   // -1 means question
        isReading = true;
        timer = 0f;

        SpeakCurrent();
    }

    void Update()
    {
        if (!isReading) return;

        // Wait until audio finishes
        if (!speaker.audioSource.isPlaying)
        {
            timer += Time.deltaTime;

            if (timer >= delay)
            {
                readIndex++;
                timer = 0f;

                if (readIndex >= answerButtons.Length)
                {
                    isReading = false; // Done reading
                }
                else
                {
                    SpeakCurrent();
                }
            }
        }
    }

    void SpeakCurrent()
    {
        if (readIndex == -1)
        {
            // Speak Question
            if (!string.IsNullOrEmpty(questionText.text))
            {
                TTS.Say(questionText.text, speaker);
            }
        }
        else
        {
            // Speak Answer
            TextMeshProUGUI answerText =
                answerButtons[readIndex].GetComponentInChildren<TextMeshProUGUI>();

            if (answerText != null && !string.IsNullOrEmpty(answerText.text))
            {
                TTS.Say(answerText.text, speaker);
            }
        }
    }
}
