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
    [Range(0f, 1f)] public float blendUIFade = 0.5f; // <--- NEW: 1.0 means UI starts fading exactly when Final Layer starts

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
        
        if (intro1.targetImage) intro1.targetImage.gameObject.SetActive(false);
        if (intro2.targetImage) intro2.targetImage.gameObject.SetActive(false);
        if (finalLayer.targetImage) finalLayer.targetImage.gameObject.SetActive(false);

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

        // --- PHASE 3 & 4: FINAL LAYER & UI FADE (OVERLAPPING) ---
        if (IsValid(finalLayer))
        {
            finalLayer.targetImage.gameObject.SetActive(true);
            
            // 1. Start the Final Animation but DO NOT "yield return" (don't wait yet)
            Coroutine finalAnimTask = StartCoroutine(PlayLayer(finalLayer));
            
            // 2. Calculate when the UI should start fading
            float animDuration = finalLayer.frames.Length / (finalLayer.fps * globalSpeedMultiplier);
            float waitBeforeFade = animDuration * (1f - blendUIFade);

            // 3. Wait for the partial duration
            if (waitBeforeFade > 0) yield return new WaitForSeconds(waitBeforeFade);

            // 4. Start UI Fade (This happens while animation is still playing)
            yield return StartCoroutine(FadeInUI());

            // 5. Safety: Make sure the final animation is actually finished before ending
            yield return finalAnimTask;
        }
        else
        {
            // If no final layer, just fade UI
            yield return StartCoroutine(FadeInUI());
        }

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

        if (timerCanvasGroup) timerCanvasGroup.alpha = 1f;
        if (wordPanelCanvasGroup) wordPanelCanvasGroup.alpha = 1f;
    }

    bool IsValid(AnimData d)
    {
        return d.targetImage != null && d.frames != null && d.frames.Length > 0;
    }
}