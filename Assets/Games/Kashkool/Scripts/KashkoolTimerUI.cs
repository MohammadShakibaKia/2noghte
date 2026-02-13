using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

public class KashkoolTimerUI : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;         // The Radial 360 Image
    public Transform birdPivot;     // The pivot that rotates the bird
    public RTLTextMeshPro timeText; // The text displaying the number

    private float totalDuration = 60f;

    /// <summary>
    /// Call this when the game starts or timer is set to define the "100%" mark.
    /// </summary>
    public void InitializeTimer(float duration)
    {
        totalDuration = Mathf.Max(duration, 1f); // Prevent divide by zero
        UpdateVisuals(duration); // Start full
    }

    /// <summary>
    /// Updates the Fill, Rotation, and Text based on current time.
    /// </summary>
    public void UpdateVisuals(float currentTime)
    {
        // 1. Update Text
        if (timeText != null)
        {
            timeText.text = Mathf.CeilToInt(currentTime).ToString();
        }

        // 2. Calculate Percentage (0 to 1)
        float progress = Mathf.Clamp01(currentTime / totalDuration);

        // 3. Update Radial Fill
        if (fillImage != null)
        {
            fillImage.fillAmount = progress;
        }

        // 4. Update Bird Rotation
        // Assumes "Fill Origin" is Top and "Clockwise" is ticked.
        // -360 * progress rotates the pivot to match the edge of the fill.
        if (birdPivot != null)
        {
            // We rotate on Z axis. Negative because Unity angles are CCW.
            float zAngle = -360f * progress;
            birdPivot.localRotation = Quaternion.Euler(0, 0, zAngle);
        }
    }
}