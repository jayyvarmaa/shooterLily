using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIComboBar : MonoBehaviour
{
    [SerializeField] private Slider comboSlider;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private GameObject comboParent; // To hide/show the bar

    private Vector3 initialParentScale;
    private Vector3 initialTextScale;

    private void Start()
    {
        initialParentScale = comboParent.transform.localScale;
        initialTextScale = multiplierText.transform.localScale;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnComboTimerChanged.AddListener(UpdateComboBar);
            ScoreManager.Instance.OnMultiplierChanged.AddListener(UpdateMultiplierText);
        }
        comboParent.SetActive(false);
    }

    private void UpdateComboBar(float percent)
    {
        comboSlider.value = percent;
        
        if (percent > 0 && !comboParent.activeSelf)
        {
            comboParent.SetActive(true);
            comboParent.transform.DOKill();
            comboParent.transform.DOScale(initialParentScale, 0.2f).From(Vector3.zero);
        }
        else if (percent <= 0 && comboParent.activeSelf)
        {
            comboParent.transform.DOKill();
            comboParent.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => comboParent.SetActive(false));
        }
    }

    private void UpdateMultiplierText(int multiplier)
    {
        multiplierText.text = "x" + multiplier;
        
        // Use DOKill and reset to initial scale to prevent "size drift"
        multiplierText.transform.DOKill();
        multiplierText.transform.localScale = initialTextScale;
        
        // Punch scale on upgrade (lower intensity)
        multiplierText.transform.DOPunchScale(initialTextScale * 0.2f, 0.2f);
        
        // NEW: Load intensity at 5x (clamped)
        float intensity = Mathf.InverseLerp(1, 5, multiplier);
        multiplierText.color = Color.Lerp(Color.white, Color.red, intensity);
    }
}
