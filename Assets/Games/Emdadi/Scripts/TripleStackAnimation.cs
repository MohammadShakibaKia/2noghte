using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class TripleStackAnimation : MonoBehaviour
{
    [Header("--- GLOBAL CONTROLS ---")]
    [Range(0.1f, 5f)] public float globalSpeedMultiplier = 1.0f;
    
    [Header("--- BLENDING (0 = Sequential, 1 = Instant Start) ---")]
    [Range(0f, 1f)] public float blendIntro2 = 0.5f; 
    [Range(0f, 1f)] public float blendFinal = 0.5f;

    [System.Serializable]
    public class AnimData
    {
        public Image targetImage;
        public Sprite[] frames;
        public float fps = 24f;
    }

    [Header("--- LAYER SETTINGS ---")]
    public AnimData intro1;
    public AnimData intro2;
    public AnimData finalLayer;

    [Header("--- UI TRANSITION SETTINGS ---")]
    public CanvasGroup timerCanvasGroup;
    public CanvasGroup wordPanelCanvasGroup;
    public float uiFadeDuration = 0.5f;

    [Header("--- EVENTS ---")]
    public UnityEvent onSequenceComplete;

    private bool isPlaying = false;

    void Start()
    {
        ResetUIAndAnimations();
        PlayFullSequence();
    }

    public void PlayFullSequence()
    {
        if (isPlaying) return;
        StartCoroutine(ExecuteSequence());
    }

    private void ResetUIAndAnimations()
    {
        StopAllCoroutines();
        isPlaying = false;
        
        // Hide Images
        if (intro1.targetImage) intro1.targetImage.gameObject.SetActive(false);
        if (intro2.targetImage) intro2.targetImage.gameObject.SetActive(false);
        if (finalLayer.targetImage) finalLayer.targetImage.gameObject.SetActive(false);

        // Reset UI Alphas to 0
        if (timerCanvasGroup) timerCanvasGroup.alpha = 0f;
        if (wordPanelCanvasGroup) wordPanelCanvasGroup.alpha = 0f;
    }

    IEnumerator ExecuteSequence()
    {
        isPlaying = true;

        // --- PHASE 1: INTRO 1 ---
        if (IsValid(intro1))
        {
            intro1.targetImage.gameObject.SetActive(true);
            StartCoroutine(PlayLayer(intro1));
            
            float duration = intro1.frames.Length / (intro1.fps * globalSpeedMultiplier);
            yield return new WaitForSeconds(duration * (1f - blendIntro2));
        }

        // --- PHASE 2: INTRO 2 ---
        if (IsValid(intro2))
        {
            intro2.targetImage.gameObject.SetActive(true);
            StartCoroutine(PlayLayer(intro2));

            float duration = intro2.frames.Length / (intro2.fps * globalSpeedMultiplier);
            yield return new WaitForSeconds(duration * (1f - blendFinal));
        }

        // --- PHASE 3: FINAL LAYER ---
        if (IsValid(finalLayer))
        {
            finalLayer.targetImage.gameObject.SetActive(true);
            // Wait for this one to finish completely
            yield return StartCoroutine(PlayLayer(finalLayer));
        }

        // --- PHASE 4: UI FADE IN ---
        yield return StartCoroutine(FadeInUI());

        // --- FINISHED ---
        isPlaying = false;
        onSequenceComplete?.Invoke();
        Debug.Log("Sequence and UI Fade Complete");
    }

    IEnumerator PlayLayer(AnimData data)
    {
        float waitTime = 1f / (data.fps * globalSpeedMultiplier);
        
        for (int i = 0; i < data.frames.Length; i++)
        {
            data.targetImage.sprite = data.frames[i];
            yield return new WaitForSeconds(waitTime);
        }
        
        data.targetImage.sprite = data.frames[data.frames.Length - 1];
    }

    IEnumerator FadeInUI()
    {
        float elapsed = 0f;
        while (elapsed < uiFadeDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, 1f, elapsed / uiFadeDuration);
            
            if (timerCanvasGroup) timerCanvasGroup.alpha = newAlpha;
            if (wordPanelCanvasGroup) wordPanelCanvasGroup.alpha = newAlpha;
            
            yield return null;
        }

        // Ensure they are exactly 1 at the end
        if (timerCanvasGroup) timerCanvasGroup.alpha = 1f;
        if (wordPanelCanvasGroup) wordPanelCanvasGroup.alpha = 1f;
    }

    bool IsValid(AnimData d)
    {
        return d.targetImage != null && d.frames != null && d.frames.Length > 0;
    }
}