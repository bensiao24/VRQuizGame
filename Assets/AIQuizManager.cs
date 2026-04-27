using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;
using ReadSpeaker;

public class AIQuizManager : MonoBehaviour
{
    public TTSSpeaker speaker;

    public TextMeshProUGUI AIquestionText;
    public Button[] answerButtons;
    public TextMeshProUGUI scoreText;

    public float delay = 0.5f;

    int currentQuestion = 0;
    int score = 0;
    List<Question> questions = new List<Question>();

    int readIndex = -1;
    float timer = 0f;
    bool isReading = false;

    // Voice Recognition
    private KeywordRecognizer keywordRecognizer;

    void Start()
    {
        TTS.Init();
        LoadQuestions();
        ShowQuestion();
        SetupVoiceRecognition();
    }

    //Setup Voice Recognition
    void SetupVoiceRecognition()
    {
        keywordRecognizer = new KeywordRecognizer(new string[]
        {
            "Apple",
            "Banana",
            "Cat",
            "Dog"
        });

        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("Voice recognition started.");
    }

    // When voice is detected
    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Voice command: " + args.text);

        if (args.text == "Apple")
            SelectAnswerByLetter("A");
        else if (args.text == "Banana")
            SelectAnswerByLetter("B");
        else if (args.text == "Cat")
            SelectAnswerByLetter("C");
        else if (args.text == "Dog")
            SelectAnswerByLetter("D");
    }

    void Update()
    {
        if (!isReading) return;

        if (!speaker.audioSource.isPlaying)
        {
            timer += Time.deltaTime;

            if (timer >= delay)
            {
                readIndex++;
                timer = 0f;

                if (readIndex >= answerButtons.Length)
                {
                    isReading = false;
                }
                else
                {
                    SpeakCurrent();
                }
            }
        }
    }

    void LoadQuestions()
    {
        TextAsset txt = Resources.Load<TextAsset>("AIquestions");

        if (txt == null)
        {
            Debug.LogError("AIquestions.txt not found in Resources folder!");
            return;
        }

        string[] lines = txt.text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = line.Split('|');
            if (p.Length < 6) continue;

            questions.Add(new Question(
                p[0], p[1], p[2], p[3], p[4], p[5]
            ));
        }
    }

    void ShowQuestion()
    {
        if (currentQuestion >= questions.Count)
        {
            AIquestionText.text = $"Quiz Complete!\nScore: {score}";
            foreach (Button b in answerButtons)
                b.gameObject.SetActive(false);
            return;
        }

        Question q = questions[currentQuestion];
        AIquestionText.text = q.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;

            TextMeshProUGUI answerText =
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            answerText.text = q.answers[i];

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(q.answers[index]));
        }

        scoreText.text = "Score: " + score;

        StartReading();
    }

    void StartReading()
    {
        if (speaker == null)
        {
            Debug.LogError("Speaker not assigned!");
            return;
        }

        TTS.InterruptAll();

        readIndex = -1;
        timer = 0f;
        isReading = true;

        SpeakCurrent();
    }

    void SpeakCurrent()
    {
        if (readIndex == -1)
        {
            TTS.Say(AIquestionText.text, speaker);
        }
        else
        {
            TextMeshProUGUI answerText =
                answerButtons[readIndex].GetComponentInChildren<TextMeshProUGUI>();

            if (answerText != null)
                TTS.Say(answerText.text, speaker);
        }
    }

    void CheckAnswer(string answer)
    {
        if (answer.Trim() == questions[currentQuestion].correct)
            score++;

        currentQuestion++;
        ShowQuestion();
    }

    public void SelectAnswerByLetter(string letter)
    {
        int index = -1;

        switch (letter)
        {
            case "A": index = 0; break;
            case "B": index = 1; break;
            case "C": index = 2; break;
            case "D": index = 3; break;
        }

        if (index >= 0 && index < answerButtons.Length)
        {
            string selectedAnswer = questions[currentQuestion].answers[index];
            CheckAnswer(selectedAnswer);
        }
    }

    void OnDestroy()
    {
        if (keywordRecognizer != null)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}

public class AIQuestion
{
    public string question;
    public string[] answers;
    public string correct;

    public AIQuestion(string q, string a, string b, string c, string d, string correct)
    {
        question = q;
        answers = new string[] { a, b, c, d };
        this.correct = correct.Trim();
    }
}