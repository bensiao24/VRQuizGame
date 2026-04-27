using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestQuiz : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI scoreText;

    private int score = 0;

    void Start()
    {
        // Display a hardcoded question
        questionText.text = "What is 2 + 2?";
        scoreText.text = "Score: 0";

        string[] answers = { "3", "4", "5" };

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = answers[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => CheckAnswer(answers[index]));
        }
    }

    void CheckAnswer(string selected)
    {
        if (selected == "4")
        {
            score++;
            scoreText.text = "Score: " + score;
            questionText.text = "Correct!";
        }
        else
        {
            questionText.text = "Wrong!";
        }

        // Disable buttons after answer
        foreach (Button b in answerButtons)
            b.interactable = false;
    }
}
