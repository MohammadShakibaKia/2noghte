using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StackableAnimation : MonoBehaviour
{
    [Header("--- GLOBAL CONTROLS ---")]
    [Range(0.1f, 5f)] public float globalSpeedMultiplier = 1.0f;
    [Range(0f, 1f)] public float introBlendFactor = 0.5f;

    [System.Serializable]
    public class AnimLayer { public float baseFps = 24f; }

    [Header("--- ANIMATION SETTINGS ---")]
    public AnimLayer s_Intro1;
    public AnimLayer s_Intro2;
    public AnimLayer s_HiddenTransIn;
    public AnimLayer s_HiddenLoop;
    public AnimLayer s_RevealTransIn;
    public AnimLayer s_RevealedLoop;

    [Header("--- UI IMAGES ---")]
    public Image img_Intro1;
    public Image img_Intro2;
    public Image img_HiddenTransIn;
    public Image img_HiddenLoop;
    public Image img_RevealTransIn;
    public Image img_RevealedLoop;

    [Header("--- SPRITE SEQUENCES ---")]
    public Sprite[] seq_Intro1;
    public Sprite[] seq_Intro2;
    public Sprite[] seq_HiddenTransIn;
    public Sprite[] seq_HiddenLoop;
    public Sprite[] seq_RevealTransIn;
    public Sprite[] seq_RevealedLoop;

    private enum State { Start, IntroPlaying, Hidden, Revealed }
    private State currentState = State.Start;

    // Safety property for external scripts
    public bool IsAnimating { get; private set; }

    void Start()
    {
        HideAllLayers();
        TriggerForward();
    }

    // =========================================================
    // 1. FORWARD LOGIC
    // =========================================================

    public void TriggerForward()
    {
        if (IsAnimating) return; // Prevent overlapping triggers
        StopAllCoroutines();

        if (currentState == State.Start) StartCoroutine(RunIntroSequence());
        else if (currentState == State.Hidden) StartCoroutine(RunRevealSequence());
    }

    IEnumerator RunIntroSequence()
    {
        IsAnimating = true;
        currentState = State.IntroPlaying;

        // Intro 1
        float duration1 = 0f;
        if (IsValid(img_Intro1, seq_Intro1))
        {
            img_Intro1.gameObject.SetActive(true);
            duration1 = seq_Intro1.Length / (s_Intro1.baseFps * globalSpeedMultiplier);
            StartCoroutine(PlayOneShot(img_Intro1, seq_Intro1, s_Intro1.baseFps));
        }

        float waitTime = duration1 * (1.0f - introBlendFactor);
        if (waitTime > 0) yield return new WaitForSeconds(waitTime);

        // Intro 2
        if (IsValid(img_Intro2, seq_Intro2))
        {
            img_Intro2.gameObject.SetActive(true);
            yield return StartCoroutine(PlayOneShot(img_Intro2, seq_Intro2, s_Intro2.baseFps));
        }

        // Hidden Trans
        if (IsValid(img_HiddenTransIn, seq_HiddenTransIn))
        {
            img_HiddenTransIn.gameObject.SetActive(true);
            yield return StartCoroutine(PlayOneShot(img_HiddenTransIn, seq_HiddenTransIn, s_HiddenTransIn.baseFps));
        }

    IsAnimating = false; // Transition finished
        // Hidden Loop
        if (IsValid(img_HiddenLoop, seq_HiddenLoop))
        {
            if (img_HiddenTransIn) img_HiddenTransIn.gameObject.SetActive(false);
            img_HiddenLoop.gameObject.SetActive(true);
            currentState = State.Hidden;
            StartCoroutine(PlayLoop(img_HiddenLoop, seq_HiddenLoop, s_HiddenLoop.baseFps));
        }
    }

    IEnumerator RunRevealSequence()
    {
        IsAnimating = true;
        currentState = State.Revealed;
        if (img_HiddenLoop) img_HiddenLoop.gameObject.SetActive(false);

        if (IsValid(img_RevealTransIn, seq_RevealTransIn))
        {
            img_RevealTransIn.gameObject.SetActive(true);
            yield return StartCoroutine(PlayOneShot(img_RevealTransIn, seq_RevealTransIn, s_RevealTransIn.baseFps));
        }
                IsAnimating = false;
        if (IsValid(img_RevealedLoop, seq_RevealedLoop))
        {
            if (img_RevealTransIn) img_RevealTransIn.gameObject.SetActive(false);
            img_RevealedLoop.gameObject.SetActive(true);
            StartCoroutine(PlayLoop(img_RevealedLoop, seq_RevealedLoop, s_RevealedLoop.baseFps));
        }
    }

    // =========================================================
    // 2. BACKWARD LOGIC
    // =========================================================

    public Coroutine TriggerBackAll()
    {
        StopAllCoroutines();
        return StartCoroutine(RunBackAllSequence());
    }

    IEnumerator RunBackAllSequence()
    {
        IsAnimating = true;

        if (currentState == State.Revealed)
        {
            yield return StartCoroutine(ReverseRevealLayer());
        }

        yield return StartCoroutine(ReverseHiddenLayer());

        // Stop here: Intro 2 is visible
        currentState = State.IntroPlaying; 
        IsAnimating = false;
    }

    // =========================================================
    // REVERSE HELPERS
    // =========================================================

    IEnumerator ReverseRevealLayer()
    {
        if (img_RevealedLoop) img_RevealedLoop.gameObject.SetActive(false);
        if (IsValid(img_RevealTransIn, seq_RevealTransIn))
        {
            img_RevealTransIn.gameObject.SetActive(true);
            yield return StartCoroutine(PlayReverseOneShot(img_RevealTransIn, seq_RevealTransIn, s_RevealTransIn.baseFps));
            img_RevealTransIn.gameObject.SetActive(false);
        }
        
        if (IsValid(img_HiddenLoop, seq_HiddenLoop))
        {
            img_HiddenLoop.gameObject.SetActive(true);
            currentState = State.Hidden;
            StartCoroutine(PlayLoop(img_HiddenLoop, seq_HiddenLoop, s_HiddenLoop.baseFps));
        }
    }

    IEnumerator ReverseHiddenLayer()
    {
        if (img_HiddenLoop) img_HiddenLoop.gameObject.SetActive(false);
        if (IsValid(img_HiddenTransIn, seq_HiddenTransIn))
        {
            img_HiddenTransIn.gameObject.SetActive(true);
            yield return StartCoroutine(PlayReverseOneShot(img_HiddenTransIn, seq_HiddenTransIn, s_HiddenTransIn.baseFps));
            img_HiddenTransIn.gameObject.SetActive(false);
        }
    }

    // =========================================================
    // ENGINES
    // =========================================================

    void HideAllLayers()
    {
        Image[] imgs = { img_Intro1, img_Intro2, img_HiddenTransIn, img_HiddenLoop, img_RevealTransIn, img_RevealedLoop };
        foreach (var i in imgs) if (i) i.gameObject.SetActive(false);
    }

    bool IsValid(Image i, Sprite[] s) => i != null && s != null && s.Length > 0;

    IEnumerator PlayOneShot(Image target, Sprite[] frames, float fps)
    {
        float wait = 1f / (fps * globalSpeedMultiplier);
        for (int i = 0; i < frames.Length; i++)
        {
            target.sprite = frames[i];
            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator PlayReverseOneShot(Image target, Sprite[] frames, float fps)
    {
        float wait = 1f / (fps * globalSpeedMultiplier);
        for (int i = frames.Length - 1; i >= 0; i--)
        {
            target.sprite = frames[i];
            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator PlayLoop(Image target, Sprite[] frames, float fps)
    {
        float wait = 1f / (fps * globalSpeedMultiplier);
        int i = 0;
        while (target != null)
        {
            target.sprite = frames[i];
            i = (i + 1) % frames.Length;
            yield return new WaitForSeconds(wait);
        }
    }
}