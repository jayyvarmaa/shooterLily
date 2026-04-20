using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    [SerializeField] private GameObject floatingTextPrefab;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnBonusEarned.AddListener(SpawnText);
        }
    }

    private void SpawnText(string message, Vector3 position)
    {
        GameObject textObj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        textObj.GetComponent<FloatingText>().Setup(message);
    }
}
