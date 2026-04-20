using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScore : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText = null;

    [SerializeField]
    private string prefix = "Score: ";

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            UpdateScoreUI(ScoreManager.Instance.GetScore());
            ScoreManager.Instance.OnScoreChanged.AddListener(UpdateScoreUI);
        }
    }

    public void UpdateScoreUI(int currentScore)
    {
        scoreText.SetText(prefix + currentScore.ToString());
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged.RemoveListener(UpdateScoreUI);
        }
    }
}
