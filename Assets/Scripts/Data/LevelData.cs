using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber = 1;
    public string levelName = "Level 1";
    public int targetScore = 100;
    
    [Header("Grid Settings")]
    public int gridHeight = 15;
    
    [Header("Grid Layout (10x15)")]
    [SerializeField] private BlockColorLine[] levelLayout = new BlockColorLine[15];
    
    [Header("Shooter Blocks")]
    public int shooterBlockCount = 6;
    public ShooterBlockData[] shooterBlocks = new ShooterBlockData[6];
    
    [Header("Platform Settings")]
    public int platformCount = 3;
    
    void OnEnable()
    {
        if (levelLayout == null || levelLayout.Length == 0)
        {
            levelLayout = new BlockColorLine[15];
            for (int i = 0; i < 15; i++)
            {
                levelLayout[i] = new BlockColorLine();
                if (levelLayout[i].blocks == null)
                {
                    levelLayout[i].blocks = new BlockColor[10];
                }
            }
        }
        
        for (int i = 0; i < levelLayout.Length; i++)
        {
            if (levelLayout[i] == null)
            {
                levelLayout[i] = new BlockColorLine();
            }
            if (levelLayout[i].blocks == null)
            {
                levelLayout[i].blocks = new BlockColor[10];
            }
        }
    }
    
    public BlockColor GetBlockAt(int x, int y)
    {
        if (x >= 0 && x < 10 && y >= 0 && y < gridHeight && 
            levelLayout != null && y < levelLayout.Length && 
            levelLayout[y] != null && levelLayout[y].blocks != null && 
            x < levelLayout[y].blocks.Length)
        {
            return levelLayout[y].blocks[x];
        }
        return BlockColor.Yellow;
    }
    
    public void SetBlockAt(int x, int y, BlockColor color)
    {
        if (x >= 0 && x < 10 && y >= 0 && y < gridHeight && 
            levelLayout != null && y < levelLayout.Length && 
            levelLayout[y] != null && levelLayout[y].blocks != null && 
            x < levelLayout[y].blocks.Length)
        {
            levelLayout[y].blocks[x] = color;
        }
    }
}

[System.Serializable]
public class BlockColorLine
{
    public BlockColor[] blocks = new BlockColor[10];
}

[System.Serializable]
public class ShooterBlockData
{
    public BlockColor color = BlockColor.Yellow;
    public int bulletCount = 20;
}
