using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using System.Collections;
using TMPro;

public class KashkoolUIManager : MonoBehaviour
{
    [Header("--- TIMER UI ---")]
    public TMP_InputField timerInputField; 
    public Button timerToggleButton; 
    public RTLTextMeshPro timerStatusText; 
    
    // --- NEW REFERENCE ---
    [Tooltip("Drag your Timer Prefab (with KashkoolTimerUI script) here")]
    public KashkoolTimerUI wallTimerVisual; 

    // Keep old text reference just in case, or for the Operator panel
    public RTLTextMeshPro opTimerText;     

    [Header("--- ASSETS ---")]
    public Sprite iconCorrect;
    public Sprite iconWrong;
    public Sprite iconDefault;

    [Header("--- OPERATOR PANEL ---")]
    public GameObject opSetupPanel;
    public GameObject opSelectionPanel;
    public GameObject opGamePanel;
    public Button[] opCategoryButtons; 
    public RTLTextMeshPro opQuestionText;
    public RTLTextMeshPro opScoreText;
    public Button undoButton;

    [Header("--- WALL PANEL ---")]
    public GameObject wallSelectionPanel;
    public GameObject wallGamePanel;
    public CanvasGroup wallGamePanelCG;
    public Image[] wallCategoryImages; 
    public RTLTextMeshPro wallQuestionText;
    public RTLTextMeshPro wallScoreText;
    public Image[] wallProgressBars;

    public void ResetVisibility()
    {
        opSetupPanel.SetActive(true);
        opSelectionPanel.SetActive(false);
        opGamePanel.SetActive(false);
        
        wallSelectionPanel.SetActive(true);
        wallGamePanel.SetActive(false);
        if (wallGamePanelCG) wallGamePanelCG.alpha = 0;

        foreach (var img in wallCategoryImages)
        {
            CanvasGroup cg = img.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0;
            img.gameObject.SetActive(false);
        }

        foreach (var btn in opCategoryButtons)
        {
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0;
            btn.gameObject.SetActive(false);
        }
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float targetAlpha, float speed)
    {
        if (cg == null) yield break;
        cg.alpha = startAlpha;
        while (Mathf.Abs(cg.alpha - targetAlpha) > 0.01f)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * speed);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    public void UpdateScoreUI(string qText, int score)
    {
        if(opQuestionText) opQuestionText.text = qText;
        if(opScoreText) opScoreText.text = "Score: " + score;
        if(wallQuestionText) wallQuestionText.text = qText;
        if(wallScoreText) wallScoreText.text = score.ToString();
    }

    public void SetProgressBar(int index, bool isCorrect)
    {
        if (index < wallProgressBars.Length)
        {
            wallProgressBars[index].sprite = isCorrect ? iconCorrect : iconWrong;
            wallProgressBars[index].color = Color.white;
        }
    }

    public void ResetProgressBars()
    {
        foreach (var bar in wallProgressBars) 
        { 
            bar.sprite = iconDefault; 
            bar.color = Color.white; 
        }
    }

    // --- UPDATED TIMER LOGIC ---

    public void InitializeVisualTimer(float totalSeconds)
{
    if (wallTimerVisual != null)
    {
        Debug.Log($"[DEBUG] Initializing Wall Timer with Total: {totalSeconds}");
        wallTimerVisual.InitializeTimer(totalSeconds);
    }
    else
    {
        Debug.LogError("[DEBUG] CRITICAL: 'Wall Timer Visual' is NULL in UIManager! Check Inspector.");
    }
}

    public void UpdateTimerDisplay(float timeRemaining)
    {
        // Update Operator Text (Simple)
        int seconds = Mathf.CeilToInt(timeRemaining);
        if (opTimerText) opTimerText.text = seconds.ToString();

        // Update Wall Visuals (New Script)
        if (wallTimerVisual != null)
        {
            wallTimerVisual.UpdateVisuals(timeRemaining);
        }
    }

    public void UpdateTimerStatusUI(bool isRunning)
    {
        if (timerStatusText != null)
            timerStatusText.text = isRunning ? "PAUSE" : "RESUME";
            
        if (timerToggleButton != null)
            timerToggleButton.image.color = isRunning ? Color.red : Color.green;
    }
}