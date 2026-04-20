using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Values")]
    [SerializeField] private int score = 0;
    [SerializeField] private int currentMultiplier = 1;
    [SerializeField] private float comboTimer = 0f;
    [SerializeField] private float maxComboTime = 5f;

    [Header("Style Thresholds")]
    [SerializeField] private float closeShaveDistance = 1.8f;
    [SerializeField] private int sharpshooterStreakGoal = 5;

    private int currentHitStreak = 0;
    private int multikillCount = 0;
    private float multikillWindow = 0.5f;
    private float lastKillTime = -1f;

    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int> OnMultiplierChanged;
    public UnityEvent<float> OnComboTimerChanged; // 0 to 1
    public UnityEvent<string, Vector3> OnBonusEarned; // Message, Position

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private float gameTimer = 0f;
    private float comboWindowStartTime = -1f;
    private float maxComboDurationInWindow = 6f;
    private bool comboUsedInCurrentWindow = false;

    private void Update()
    {
        if (Player.Instance != null && Player.Instance.IsDead) return;

        // Update Game Loop Timer (0 - 60s)
        gameTimer = (gameTimer + Time.deltaTime) % 60f;

        // Reset the "Used" flag when we are outside of any combo window
        if (!IsComboWindowActive())
        {
            comboUsedInCurrentWindow = false;
        }

        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            OnComboTimerChanged?.Invoke(comboTimer / maxComboTime);

            // Check for 6-second max duration limit
            if (Time.time - comboWindowStartTime > maxComboDurationInWindow)
            {
                ResetCombo();
            }
            // Check if we exited the valid window
            else if (!IsComboWindowActive())
            {
                ResetCombo();
            }

            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }

        // Proximity Bonus Logic
        CheckProximityBonus();
    }

    private bool IsComboWindowActive()
    {
        // Active windows: 20-30s and 50-60s
        bool inWindowOne = gameTimer >= 20f && gameTimer <= 30f;
        bool inWindowTwo = gameTimer >= 50f && gameTimer <= 60f;
        return inWindowOne || inWindowTwo;
    }

    private void CheckProximityBonus()
    {
        if (Player.Instance == null || Player.Instance.IsDead) return;
        
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(Player.Instance.transform.position, 3f, LayerMask.GetMask("Enemy"));
        if (nearbyEnemies.Length >= 3)
        {
            // PROXIMITY BONUS: Only valid during combo windows? 
            // The user didn't specify, but I'll leave it always active for "base" survival points 
            // but at a very low rate as previously set.
            AddScore(Mathf.RoundToInt(Time.deltaTime * 3 * nearbyEnemies.Length), false);
        }
    }

    public void AddScore(int amount, bool useMultiplier = true, string bonusMessage = "", Vector3? position = null)
    {
        if (Player.Instance != null && Player.Instance.IsDead) return;

        int totalAdd = amount;
        
        if (useMultiplier)
        {
            // Health-based multiplier
            float healthPercent = (float)Player.Instance.Health / Player.Instance.MaxHealth;
            float riskMultiplier = Mathf.Lerp(2.0f, 1.0f, healthPercent);
            
            totalAdd = Mathf.RoundToInt(amount * currentMultiplier * riskMultiplier);
        }

        score += totalAdd;
        OnScoreChanged?.Invoke(score);
        
        if (!string.IsNullOrEmpty(bonusMessage) && position.HasValue)
        {
            OnBonusEarned?.Invoke(bonusMessage, position.Value);
        }
    }

    public void RegisterKill(Enemy enemy)
    {
        if (Player.Instance != null && Player.Instance.IsDead) return;

        // 1. Momentum & Combo (Restricted to Windows & One Use)
        if (IsComboWindowActive())
        {
            if (comboTimer <= 0 && !comboUsedInCurrentWindow) // Starting a NEW combo
            {
                comboWindowStartTime = Time.time;
                comboUsedInCurrentWindow = true;
                
                comboTimer = maxComboTime; // maxComboTime should be 6s in Inspector
                IncrementMultiplier();
            }
            else if (comboTimer > 0) // Maintaining existing combo
            {
                // NO comboTimer refill! The bar continues to drain independently.
                IncrementMultiplier();
            }
        }
        else
        {
            ResetCombo(); // Just in case
        }

        // 2. Multikill Check (NEW: Lower values)
        if (Time.time - lastKillTime < multikillWindow)
        {
            multikillCount++;
            if (multikillCount >= 2)
            {
                AddScore(5 * multikillCount, false, "MULTIKILL x" + multikillCount, enemy.transform.position);
            }
        }
        else
        {
            multikillCount = 1;
        }
        lastKillTime = Time.time;

        // 3. Close Shave Check (NEW: Lower values)
        float dist = Vector2.Distance(Player.Instance.transform.position, enemy.transform.position);
        if (dist <= closeShaveDistance)
        {
            AddScore(20, false, "CLOSE SHAVE!", enemy.transform.position + Vector3.up);
        }

        // Base Score for Kill (NEW: Lower baseline)
        // We'll use 10% of the defined ScorePoints to scale down existing assets
        int basePoints = Mathf.Max(1, enemy.EnemyData.ScorePoints / 10);
        AddScore(basePoints, true, "+" + basePoints, enemy.transform.position);
    }

    public void RegisterShot(bool hit)
    {
        if (Player.Instance != null && Player.Instance.IsDead) return;

        if (hit)
        {
            currentHitStreak++;
            if (currentHitStreak >= sharpshooterStreakGoal)
            {
                AddScore(30, false, "SHARPSHOOTER!", Player.Instance.transform.position + Vector3.up);
                currentHitStreak = 0; 
            }
        }
        else
        {
            currentHitStreak = 0;
        }
    }

    private void IncrementMultiplier()
    {
        currentMultiplier = Mathf.Min(currentMultiplier + 1, 10);
        OnMultiplierChanged?.Invoke(currentMultiplier);
    }

    private void ResetCombo()
    {
        currentMultiplier = 1;
        comboTimer = 0;
        OnMultiplierChanged?.Invoke(currentMultiplier);
        OnComboTimerChanged?.Invoke(0);
    }

    public int GetScore() => score;
}
