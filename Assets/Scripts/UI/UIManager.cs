using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject levelCompletePanel;
    public TextMeshProUGUI levelCompleteText;
    
    [Header("Animation")]
    public float fadeInDuration = 0.5f;
    public float scaleAnimationDuration = 0.3f;
    
    private CanvasGroup panelCanvasGroup;
    
    void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
            panelCanvasGroup = levelCompletePanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = levelCompletePanel.AddComponent<CanvasGroup>();
            }
        }
    }
    
    public void ShowLevelComplete(int goldEarned, int currentLevel)
    {
        if (levelCompletePanel == null) return;
        
        if (levelCompleteText != null)
        {
            levelCompleteText.text = $"Level {currentLevel} Complete!";
        }
        
        levelCompletePanel.SetActive(true);
        StartCoroutine(AnimatePanelIn());
    }
    
    public void HideLevelComplete()
    {
        if (levelCompletePanel == null) return;
        
        StartCoroutine(AnimatePanelOut());
    }
    
    private System.Collections.IEnumerator AnimatePanelIn()
    {
        if (panelCanvasGroup == null) yield break;
        
        panelCanvasGroup.alpha = 0f;
        levelCompletePanel.transform.localScale = Vector3.zero;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeInDuration;
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            yield return null;
        }
        
        elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        
        while (elapsedTime < scaleAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / scaleAnimationDuration;
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            levelCompletePanel.transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }
        
        panelCanvasGroup.alpha = 1f;
        levelCompletePanel.transform.localScale = Vector3.one;
    }
    
    private System.Collections.IEnumerator AnimatePanelOut()
    {
        if (panelCanvasGroup == null) 
        {
            levelCompletePanel.SetActive(false);
            yield break;
        }
        
        float elapsedTime = 0f;
        float startAlpha = panelCanvasGroup.alpha;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeInDuration;
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }
        
        levelCompletePanel.SetActive(false);
    }
    
    public void OnNextLevelButtonClicked()
    {
        HideLevelComplete();
        
        if (GameManager.Instance != null && GameManager.Instance.levelManager != null)
        {
            GameManager.Instance.levelManager.NextLevel();
            GameManager.Instance.OnLevelLoaded();
        }
    }
    
}
