using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealth : MonoBehaviour
{
    [SerializeField]
    private Slider healthSlider = null;

    public void Initialize(int livesCount)
    {
        healthSlider.maxValue = livesCount;
        healthSlider.value = livesCount;
    }

    public void UpdateUI(int health)
    {
        healthSlider.value = health;
    }
}
