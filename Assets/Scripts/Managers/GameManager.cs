using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    
    [Header("Game State")]
    public GameState currentState = GameState.Playing;
    
    [Header("Gold System")]
    public int gold = 0;
    public int levelGoldReward = 50;
    
    [Header("Level System")]
    public int totalBlocks;
    public int destroyedBlocks;
    
    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI levelText;
    public UIManager uiManager;
    
    [Header("Grid References")]
    public Transform gridParent;
    public GameObject blockPrefab;
    
    [Header("Level System")]
    public LevelManager levelManager;
    public PlatformManager platformManager;
    public ShooterBlockManager shooterBlockManager;
    
    private Block[,] grid;
    
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
        
        if (levelManager != null && levelManager.GetCurrentLevel() != null)
        {
            CreateLevelFromData(levelManager.GetCurrentLevel());
        }
    }
    
    void InitializeGame()
    {
        gold = 0;
        destroyedBlocks = 0;
        currentState = GameState.Playing;
        
        UpdateUI();
    }
    
    
    public void OnBlockDestroyed(int destroyedX, int destroyedY)
    {
        destroyedBlocks++;
        
        if (grid != null && destroyedX >= 0 && destroyedX < 10 && destroyedY >= 0 && destroyedY < grid.GetLength(1))
        {
            grid[destroyedX, destroyedY] = null;
        }
        
        ApplyGravityToColumn(destroyedX);
        
        if (destroyedBlocks >= totalBlocks)
        {
            CompleteLevel();
        }
    }
    
    void ApplyGravityToColumn(int columnX)
    {
        if (grid == null || columnX < 0 || columnX >= 10) return;
        
        int gridHeight = grid.GetLength(1);
        
        for (int y = 0; y < gridHeight - 1; y++)
        {
            if (grid[columnX, y] == null)
            {
                for (int upperY = y + 1; upperY < gridHeight; upperY++)
                {
                    if (grid[columnX, upperY] != null)
                    {
                        Block blockToMove = grid[columnX, upperY];
                        grid[columnX, y] = blockToMove;
                        grid[columnX, upperY] = null;
                        
                        blockToMove.gridX = columnX;
                        blockToMove.gridY = y;
                        
                        Vector3 newPosition = new Vector3(columnX, y, 0);
                        StartCoroutine(MoveBlockToPosition(blockToMove, newPosition));
                        
                        break;
                    }
                }
            }
        }
    }
    
    System.Collections.IEnumerator MoveBlockToPosition(Block block, Vector3 targetPosition)
    {
        if (block == null) yield break;
        
        Vector3 startPosition = block.transform.position;
        float moveDuration = 0.3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveDuration;
            
            block.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            
            yield return null;
        }
        
        if (block != null)
        {
            block.transform.position = targetPosition;
        }
    }
    
    
    void CompleteLevel()
    {
        currentState = GameState.LevelComplete;
        
        gold += levelGoldReward;
        
        UpdateUI();
        
        if (uiManager != null)
        {
            uiManager.ShowLevelComplete(levelGoldReward, levelManager != null ? levelManager.currentLevelIndex + 1 : 1);
        }
        else
        {
            if (levelManager != null)
            {
                levelManager.NextLevel();
            }
            Invoke(nameof(ResumeGame), 1.5f);
        }
    }
    
    void ResumeGame()
    {
        currentState = GameState.Playing;
    }
    
    public void OnLevelLoaded()
    {
        currentState = GameState.Playing;
        destroyedBlocks = 0;
        
        UpdateUI();
    }
    
    void OnDestroy()
    {
        DOTween.KillAll();
    }
    
    void UpdateUI()
    {
        if (goldText != null)
            goldText.text = gold.ToString();
            
        if (levelText != null && levelManager != null)
            levelText.text = "Level " + (levelManager.currentLevelIndex + 1);
    }
    
    public Block GetBlockAt(int x, int y)
    {
        if (x >= 0 && x < 10 && y >= 0 && y < 15)
        {
            return grid[x, y];
        }
        return null;
    }
    
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < 10 && y >= 0 && y < 15;
    }
    
    public void CreateLevelFromData(LevelData levelData)
    {
        DOTween.KillAll();
        
        if (gridParent != null)
        {
            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        grid = new Block[10, levelData.gridHeight];
        totalBlocks = 0;
        destroyedBlocks = 0;
        
        for (int y = 0; y < levelData.gridHeight; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                BlockColor blockColor = levelData.GetBlockAt(x, y);
                
                CreateBlockWithColor(x, y, blockColor);
                totalBlocks++;
            }
        }
        
        if (platformManager != null)
        {
            platformManager.CreatePlatforms(levelData.platformCount);
        }
        
        if (shooterBlockManager != null)
        {
            shooterBlockManager.CreateShooterBlocksFromLevelData(levelData);
        }
        
        UpdateUI();
    }
    
    void CreateBlockWithColor(int x, int y, BlockColor color)
    {
        if (blockPrefab == null) 
        {
            return;
        }
        
        if (gridParent == null)
        {
            return;
        }
        
        Vector3 position = new Vector3(x, y, 0);
        GameObject blockObj = Instantiate(blockPrefab, position, Quaternion.identity, gridParent);
        
        if (blockObj == null)
        {
            return;
        }
        
        Block block = blockObj.GetComponent<Block>();
        if (block != null)
        {
            block.InitializeWithColor(x, y, color);
            grid[x, y] = block;
        }
        else
        {
            Destroy(blockObj);
        }
    }
    
    [ContextMenu("Fire All Platform Shooters")]
    public void FireAllPlatformShooters()
    {
        if (SequentialShootingSystem.Instance != null)
        {
            SequentialShootingSystem.Instance.FireAllPlatformShooters();
        }
    }
}

public enum GameState
{
    Playing,
    LevelComplete,
    GameOver
}
