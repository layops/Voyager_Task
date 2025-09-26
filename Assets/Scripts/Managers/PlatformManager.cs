using UnityEngine;
using DG.Tweening;

public class PlatformManager : MonoBehaviour
{
    [Header("Platform Settings")]
    public GameObject platformPrefab;
    public Transform platformParent;
    
    [Header("Platform Layout")]
    public Transform centerPoint;
    public float platformSpacing = 1f;
    
    [Header("Merge Animation Settings")]
    public float mergeDuration = 1f;
    public float mergeScale = 1.5f;
    public Ease mergeEase = Ease.OutBack;
    
    public GameObject[] platforms;
    
    // Slot rezervasyon sistemi
    private ShooterBlock[] reservedSlots; // Hangi slot'u hangi ShooterBlock rezerve etmiş
    
    // Merge işlemi kontrolü
    private bool isMergeInProgress = false;
    
    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.levelManager != null)
        {
            LevelData currentLevel = GameManager.Instance.levelManager.GetCurrentLevel();
            if (currentLevel != null)
            {
                CreatePlatforms(currentLevel.platformCount);
            }
        }
    }
    
    public void CreatePlatforms(int platformCount)
    {
        if (platforms != null)
        {
            foreach (GameObject platform in platforms)
            {
                if (platform != null)
                {
                    Destroy(platform);
                }
            }
        }
        
        platforms = new GameObject[platformCount];
        reservedSlots = new ShooterBlock[platformCount];
        
        if (centerPoint == null)
        {
            return;
        }
        
        Vector3 centerPos = centerPoint.position;
        
        for (int i = 0; i < platformCount; i++)
        {
            Vector3 position;
            
            if (platformCount % 2 == 1)
            {
                int centerIndex = platformCount / 2;
                int offset = i - centerIndex;
                position = centerPos + Vector3.right * (offset * platformSpacing);
            }
            else
            {
                int centerOffset = platformCount / 2;
                int offset = i - centerOffset;
                if (offset >= 0)
                {
                    offset += 1;
                }
                else
                {
                    offset -= 1;
                }
                position = centerPos + Vector3.right * (offset * platformSpacing * 0.5f);
            }
            GameObject platformObj = Instantiate(platformPrefab, position, Quaternion.identity, platformParent);
            
            platforms[i] = platformObj;
        }
        
    }
    
    public void ClearPlatforms()
    {
        if (platforms != null)
        {
            foreach (GameObject platform in platforms)
            {
                if (platform != null)
                {
                    Destroy(platform);
                }
            }
            platforms = null;
        }
    }
    
    public bool IsSlotOccupied(int slotIndex)
    {
        if (platforms == null)
        {
            return true;
        }
        
        if (slotIndex < 0 || slotIndex >= platforms.Length)
        {
            return true;
        }
            
        GameObject platform = platforms[slotIndex];
        if (platform == null) 
        {
            return true;
        }
        
        // Platform'un child'larında ShooterBlock var mı kontrol et
        int shooterBlockCount = 0;
        foreach (Transform child in platform.transform)
        {
            if (child.GetComponent<ShooterBlock>() != null)
            {
                shooterBlockCount++;
            }
        }
        
        bool isOccupied = shooterBlockCount > 0;
        return isOccupied;
    }
    
    public int GetFirstEmptySlot()
    {
        if (platforms == null) 
        {
            return -1;
        }
        
        for (int i = 0; i < platforms.Length; i++)
        {
            bool isOccupied = IsSlotOccupied(i);
            
            if (!isOccupied)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    public Vector3 GetSlotPosition(int slotIndex)
    {
        if (platforms == null || slotIndex < 0 || slotIndex >= platforms.Length)
            return Vector3.zero;
            
        GameObject platform = platforms[slotIndex];
        if (platform == null) return Vector3.zero;
        
        return platform.transform.position;
    }
    
    // ShooterBlock hareket kontrolü için yeni metodlar
    public bool CanMoveToSlot(int slotIndex)
    {
        if (platforms == null)
        {
            return false;
        }
        
        if (slotIndex < 0 || slotIndex >= platforms.Length)
        {
            return false;
        }
        
        GameObject platform = platforms[slotIndex];
        if (platform == null) 
        {
            return false;
        }
        
        // Platform'un child'larında ShooterBlock var mı kontrol et
        int shooterBlockCount = 0;
        foreach (Transform child in platform.transform)
        {
            if (child.GetComponent<ShooterBlock>() != null)
            {
                shooterBlockCount++;
            }
        }
        
        bool isOccupied = shooterBlockCount > 0;
        bool canMove = !isOccupied;
        
        return canMove;
    }
    
    public bool ValidateMovement(int fromSlot, int toSlot)
    {
        if (!CanMoveToSlot(toSlot))
        {
            return false;
        }
        
        if (fromSlot == toSlot)
        {
            return false;
        }
        
        return true;
    }
    
    public int ReserveSlot(int slotIndex, ShooterBlock shooterBlock)
    {
        if (!CanMoveToSlot(slotIndex))
        {
            return -1;
        }
        
        if (reservedSlots != null && slotIndex >= 0 && slotIndex < reservedSlots.Length)
        {
            reservedSlots[slotIndex] = shooterBlock;
            return slotIndex;
        }
        
        return -1;
    }
    
    public void ReleaseSlot(int slotIndex)
    {        
        if (platforms == null || slotIndex < 0 || slotIndex >= platforms.Length)
        {
            return;
        }
        
        if (reservedSlots != null && slotIndex >= 0 && slotIndex < reservedSlots.Length)
        {
            if (reservedSlots[slotIndex] != null)
            {
                reservedSlots[slotIndex] = null;
            }
        }
    }
    
    public void CompleteMovement(int slotIndex, ShooterBlock shooterBlock)
    {
        if (platforms == null || slotIndex < 0 || slotIndex >= platforms.Length)
            return;
            
        GameObject platform = platforms[slotIndex];
        if (platform != null)
        {
            ReleaseSlot(slotIndex);
            
            shooterBlock.transform.SetParent(platform.transform);
            shooterBlock.transform.position = platform.transform.position;
            
            CheckForMerge(shooterBlock.blockColor);
        }
    }
    
    public void MarkSlotAsOccupied(int slotIndex, ShooterBlock shooterBlock)
    {
        if (platforms == null || slotIndex < 0 || slotIndex >= platforms.Length)
        {
            return;
        }
        
        if (reservedSlots != null && slotIndex >= 0 && slotIndex < reservedSlots.Length)
        {
            reservedSlots[slotIndex] = shooterBlock;
        }
    }
    
    public void CheckForMerge(BlockColor color)
    {
        if (isMergeInProgress)
        {
            return;
        }
        
        System.Collections.Generic.List<ShooterBlock> sameColorShooters = new System.Collections.Generic.List<ShooterBlock>();
        
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] != null)
            {
                Transform platformTransform = platforms[i].transform;
                for (int j = 0; j < platformTransform.childCount; j++)
                {
                    ShooterBlock shooter = platformTransform.GetChild(j).GetComponent<ShooterBlock>();
                    if (shooter != null && shooter.blockColor == color)
                    {
                        sameColorShooters.Add(shooter);
                    }
                }
            }
        }
        
        if (sameColorShooters.Count >= 3)
        {
            isMergeInProgress = true;
            StartMerge(sameColorShooters);
        }
    }
    
    private void StartMerge(System.Collections.Generic.List<ShooterBlock> shooters)
    {
        foreach (ShooterBlock shooter in shooters)
        {
            if (shooter != null)
            {
                if (shooter.isShooting)
                {
                    shooter.StopShooting();
                }
                
                if (shooter.shooterTargeting != null)
                {
                    shooter.shooterTargeting.Cleanup();
                }
            }
        }
        
        StartCoroutine(WaitAndStartMerge(shooters));
    }
    
    private System.Collections.IEnumerator WaitAndStartMerge(System.Collections.Generic.List<ShooterBlock> shooters)
    {
        yield return new WaitForSeconds(0.2f);
        
        foreach (ShooterBlock shooter in shooters)
        {
            if (shooter != null && shooter.isShooting)
            {
                shooter.StopShooting();
            }
        }
        
        ShooterBlock middleShooter = FindMiddleShooter(shooters);
        
        if (middleShooter == null)
        {
            yield break;
        }
        
        int totalBullets = 0;
        foreach (ShooterBlock shooter in shooters)
        {
            totalBullets += shooter.bulletCount;
        }
        
        StartMergeAnimation(shooters, middleShooter, totalBullets);
    }
    
    private void StartMergeAnimation(System.Collections.Generic.List<ShooterBlock> shooters, ShooterBlock middleShooter, int totalBullets)
    {
        if (middleShooter == null)
        {
            return;
        }
        
        Vector3 middlePosition = middleShooter.transform.position;
        
        System.Collections.Generic.List<ShooterBlock> sideShooters = new System.Collections.Generic.List<ShooterBlock>();
        foreach (ShooterBlock shooter in shooters)
        {
            if (shooter != null && shooter != middleShooter)
            {
                sideShooters.Add(shooter);
            }
        }
        
        foreach (ShooterBlock sideShooter in sideShooters)
        {
            if (sideShooter == null) continue;
            
            sideShooter.transform.DOMove(middlePosition, mergeDuration * 0.7f)
                .SetEase(Ease.InQuart)
                .OnComplete(() => {
                    if (sideShooter != null)
                    {
                        DestroyShooterBlock(sideShooter);
                    }
                });
            
            sideShooter.transform.DOScale(Vector3.zero, mergeDuration * 0.7f)
                .SetEase(Ease.InBack);
        }
        
        Sequence middleSequence = DOTween.Sequence();
        middleSequence.Append(middleShooter.transform.DOScale(Vector3.one * mergeScale, mergeDuration * 0.3f).SetEase(Ease.OutBack))
                     .Append(middleShooter.transform.DOScale(Vector3.one, mergeDuration * 0.4f).SetEase(Ease.InOutQuad))
                     .OnComplete(() => {
                         middleShooter.transform.localScale = Vector3.one;
                         
                         middleShooter.bulletCount = totalBullets;
                         middleShooter.maxBullets = totalBullets;
                         middleShooter.UpdateVisual();
                         
                         StartCoroutine(ResumeShootingAfterMerge());
                     });
    }
    
    private System.Collections.IEnumerator ResumeShootingAfterMerge()
    {
        yield return new WaitForSeconds(0.3f);
        
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] != null)
            {
                Transform platformTransform = platforms[i].transform;
                for (int j = 0; j < platformTransform.childCount; j++)
                {
                    ShooterBlock shooter = platformTransform.GetChild(j).GetComponent<ShooterBlock>();
                    if (shooter != null)
                    {
                        shooter.transform.localScale = Vector3.one;
                    }
                }
            }
        }
        
        isMergeInProgress = false;
        
        if (SequentialShootingSystem.Instance != null)
        {
            SequentialShootingSystem.Instance.FireAllPlatformShooters();
        }
    }
    
    private ShooterBlock FindMiddleShooter(System.Collections.Generic.List<ShooterBlock> shooters)
    {
        if (shooters.Count == 0) return null;
        
        shooters.RemoveAll(shooter => shooter == null);
        
        if (shooters.Count == 0) return null;
        
        shooters.Sort((a, b) => {
            if (a == null || b == null) return 0;
            
            int slotA = GetSlotIndexForShooter(a);
            int slotB = GetSlotIndexForShooter(b);
            return slotA.CompareTo(slotB);
        });
        
        int middleIndex = shooters.Count / 2;
        return shooters[middleIndex];
    }
    
    private int GetSlotIndexForShooter(ShooterBlock shooter)
    {
        if (shooter == null) return -1;
        
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] != null)
            {
                if (platforms[i].transform == shooter.transform.parent)
                {
                    return i;
                }
            }
        }
        return -1;
    }
    
    private void DestroyShooterBlock(ShooterBlock shooter)
    {
        if (shooter != null)
        {
            int slotIndex = GetSlotIndexForShooter(shooter);
            if (slotIndex >= 0)
            {
                ReleaseSlot(slotIndex);
            }
            
            Destroy(shooter.gameObject);
        }
    }
    
    public ShooterBlock GetShooterAtSlot(int slotIndex)
    {
        if (platforms == null || slotIndex < 0 || slotIndex >= platforms.Length)
        {
            return null;
        }
        
        GameObject platform = platforms[slotIndex];
        if (platform == null) return null;
        
        for (int i = 0; i < platform.transform.childCount; i++)
        {
            ShooterBlock shooter = platform.transform.GetChild(i).GetComponent<ShooterBlock>();
            if (shooter != null)
            {
                return shooter;
            }
        }
        
        return null;
    }
    
    [ContextMenu("Test Merge System")]
    public void TestMergeSystem()
    {
        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] != null)
            {
                Transform platformTransform = platforms[i].transform;
                for (int j = 0; j < platformTransform.childCount; j++)
                {
                    ShooterBlock shooter = platformTransform.GetChild(j).GetComponent<ShooterBlock>();
                    if (shooter != null)
                    {
                    }
                }
            }
        }
        
        CheckForMerge(BlockColor.Yellow);
        CheckForMerge(BlockColor.Blue);
        CheckForMerge(BlockColor.Red);
    }
}
