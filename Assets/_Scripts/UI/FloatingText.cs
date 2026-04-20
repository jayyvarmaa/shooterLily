using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float duration = 1.5f;

    public void Setup(string message, Color? color = null)
    {
        text.text = message;
        if (color.HasValue) text.color = color.Value;

        // NEW: Add random offset to prevent "stacking" look
        Vector3 randomOffset = new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(-0.5f, 0.5f), 0);
        transform.position += randomOffset;

        // Animate: Move up and fade out
        transform.DOMoveY(transform.position.y + moveDistance, duration).SetEase(Ease.OutSine);
        text.DOFade(0, duration).SetEase(Ease.InSine).OnComplete(() => Destroy(gameObject));
        
        // Punch scale for a little "pop"
        transform.DOPunchScale(Vector3.one * 0.5f, 0.3f);
    }
}
