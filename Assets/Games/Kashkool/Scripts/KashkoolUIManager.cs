using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using System.Collections;

public class KashkoolUIManager : MonoBehaviour
{
    [Header("--- ASSETS ---")]
    public Sprite iconCorrect;
    public Sprite iconWrong;
    public Sprite iconDefault;

    [Header("--- OPERATOR PANEL ---")]
    public GameObject opSetupPanel;
    public GameObject opSelectionPanel;
    public GameObject opGamePanel;
    
    // The buttons themselves (The code will look inside these for the Title/Text)
    public Button[] opCategoryButtons; 
    public RTLTextMeshPro opQuestionText;
    public RTLTextMeshPro opScoreText;

    [Header("--- WALL PANEL ---")]
    public GameObject wallSelectionPanel;
    public GameObject wallGamePanel;
    public CanvasGroup wallGamePanelCG;
    
    // The wall images (The code looks inside these for Title/Text)
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

        // Reset Wall Images
        foreach (var img in wallCategoryImages)
        {
            CanvasGroup cg = img.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0;
            img.gameObject.SetActive(false);
        }

        // Reset Operator Buttons (Hide them initially)
        foreach (var btn in opCategoryButtons)
        {
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0;
            btn.gameObject.SetActive(false);
        }
    }

    // Helper to fade a CanvasGroup
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
        opQuestionText.text = qText;
        opScoreText.text = "Score: " + score;
        wallQuestionText.text = qText;
        wallScoreText.text = score.ToString();
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
}