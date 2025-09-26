using UnityEngine;
using DG.Tweening;

public class ShooterBlockManager : MonoBehaviour
{
    [Header("Shooter Block Settings")]
    public GameObject shooterBlockPrefab;
    public Transform shooterParent;
    public int shooterBlockCount = 6;
    
    [Header("Layout Settings")]
    public float blockSpacing = 1.2f;
    
    [Header("Animation Settings")]
    public float repositionDuration = 0.5f;
    public Ease repositionEase = Ease.OutCubic;
    
    private ShooterBlock[] shooterBlocks;
    private LevelData currentLevelData;
    
    void Start()
    {
    }
    
    public void CreateShooterBlocksFromLevelData(LevelData levelData)
    {
        currentLevelData = levelData;
        
        DOTween.KillAll();
        
        shooterBlockCount = levelData.shooterBlockCount;
        
        if (shooterBlocks != null)
        {
            foreach (ShooterBlock block in shooterBlocks)
            {
                if (block != null)
                {
                    Destroy(block.gameObject);
                }
            }
        }
        
        CreateShooterBlocks();
    }
    
    void CreateShooterBlocks()
    {
        shooterBlocks = new ShooterBlock[shooterBlockCount];
        
        for (int i = 0; i < shooterBlockCount; i++)
        {
            Vector3 position = shooterParent.position + Vector3.down * (i * blockSpacing);
            
            GameObject blockObj = Instantiate(shooterBlockPrefab, position, Quaternion.identity, shooterParent);
            
            blockObj.transform.rotation = Quaternion.Euler(-90, 0, 0);
            blockObj.transform.localScale = Vector3.one;
            
            ShooterBlock shooterBlock = blockObj.GetComponent<ShooterBlock>();
            
            BlockColor blockColor = BlockColor.Yellow;
            int bulletCount = 20;
            
            if (currentLevelData != null && i < currentLevelData.shooterBlocks.Length)
            {
                blockColor = currentLevelData.shooterBlocks[i].color;
                bulletCount = currentLevelData.shooterBlocks[i].bulletCount;
            }
            else if (currentLevelData != null)
            {
                blockColor = (BlockColor)(i % 3);
                bulletCount = 20;
            }
            
            shooterBlock.Initialize(blockColor, bulletCount);
            shooterBlocks[i] = shooterBlock;
        }
    }
    
    public void RefreshShooterBlocks()
    {
        if (shooterBlocks != null)
        {
            foreach (ShooterBlock block in shooterBlocks)
            {
                if (block != null)
                {
                    Destroy(block.gameObject);
                }
            }
        }
        
        CreateShooterBlocks();
    }
    
    public ShooterBlock GetShooterBlock(int index)
    {
        if (shooterBlocks != null && index >= 0 && index < shooterBlocks.Length)
        {
            return shooterBlocks[index];
        }
        return null;
    }
    
    public void RepositionShooterBlocks()
    {
        if (shooterBlocks == null) return;
        
        int activeShooterCount = 0;
        Sequence repositionSequence = DOTween.Sequence();
        
        for (int i = 0; i < shooterBlocks.Length; i++)
        {
            if (shooterBlocks[i] != null)
            {
                if (IsShooterOnPlatform(shooterBlocks[i]))
                {
                    continue;
                }
                
                Vector3 newPosition = shooterParent.position + Vector3.down * (activeShooterCount * blockSpacing);
                
                Vector3 currentPosition = shooterBlocks[i].transform.position;
                
                if (Vector3.Distance(currentPosition, newPosition) > 0.1f)
                {
                    Tween moveTween = shooterBlocks[i].transform.DOMove(newPosition, repositionDuration)
                        .SetEase(repositionEase);
                    
                    repositionSequence.Join(moveTween);
                }
                
                activeShooterCount++;
            }
        }
    }
    
    bool IsShooterOnPlatform(ShooterBlock shooterBlock)
    {
        if (shooterBlock == null) return false;
        
        if (GameManager.Instance != null && GameManager.Instance.platformManager != null)
        {
            PlatformManager platformManager = GameManager.Instance.platformManager;
            
            for (int i = 0; i < 10; i++)
            {
                Vector3 slotPos = platformManager.GetSlotPosition(i);
                if (slotPos != Vector3.zero)
                {
                    float distance = Vector3.Distance(shooterBlock.transform.position, slotPos);
                    if (distance < 0.5f)
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    public void RemoveShooterBlock(ShooterBlock shooterBlock)
    {
        if (shooterBlocks == null || shooterBlock == null) return;
        
        for (int i = 0; i < shooterBlocks.Length; i++)
        {
            if (shooterBlocks[i] == shooterBlock)
            {
                shooterBlocks[i] = null;
                break;
            }
        }
        
        RepositionShooterBlocks();
    }
    
    void OnDestroy()
    {
        DOTween.KillAll();
    }
}
