using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    private int score = 0;
    private Text scoreText;

    private void Start()
    {
        scoreText = GetComponent<Text>();
    }

    public int GetScoreAsNumber()
    {
        return score;
    }

    public void Increment(int amount)
    {
        if (amount > 0)
        {
            score += amount;
            scoreText.text = "SCORE: " + score;
        }
    }

    public void set(string s)
    {
        scoreText.text = s;
    }
}
