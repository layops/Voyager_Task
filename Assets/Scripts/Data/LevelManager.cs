using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level System")]
    public LevelData[] levels;
    public int currentLevelIndex = 0;
    
    public static LevelManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public LevelData GetCurrentLevel()
    {
        if (currentLevelIndex < levels.Length)
        {
            return levels[currentLevelIndex];
        }
        return null;
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < levels.Length)
        {
            currentLevelIndex = levelIndex;
            LevelData levelData = levels[levelIndex];
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CreateLevelFromData(levelData);
            }
        }
    }
    
    public void NextLevel()
    {
        if (currentLevelIndex + 1 < levels.Length)
        {
            LoadLevel(currentLevelIndex + 1);
        }
        else
        {
        }
    }
}
