using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// ShooterBlock ana class - çok daha kısa ve temiz
/// </summary>
public class ShooterBlock : MonoBehaviour
{
    [Header("Block Settings")]
    public BlockColor blockColor;
    public int bulletCount = 20;
    public int maxBullets = 20;
    
    [Header("Visual")]
    public MeshRenderer meshRenderer;
    public TextMeshProUGUI bulletCountText;
    
    [Header("Material References")]
    public Material yellowMaterial;
    public Material blueMaterial;
    public Material redMaterial;
    
    [Header("Shooter System")]
    public float shootDelay = 0.1f;
    
    [Header("Bullet System")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    // shooter artık meshRenderer.transform olarak kullanılacak
    
    // Component referansları
    private ShooterMovement shooterMovement;
    public ShooterTargeting shooterTargeting;
    private ShooterAnimation shooterAnimation;
    
    public bool isShooting = false;
    private int wrongHitCount = 0;
    private const int MAX_WRONG_HITS = 3;
    
    private static bool isAnyShooterMoving = false;
    
    void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        shooterMovement = GetComponent<ShooterMovement>();
        shooterTargeting = GetComponent<ShooterTargeting>();
        shooterAnimation = GetComponent<ShooterAnimation>();
        
        if (shooterMovement == null) shooterMovement = gameObject.AddComponent<ShooterMovement>();
        if (shooterTargeting == null) shooterTargeting = gameObject.AddComponent<ShooterTargeting>();
        if (shooterAnimation == null) shooterAnimation = gameObject.AddComponent<ShooterAnimation>();
        
        shooterMovement.Initialize(this);
        shooterTargeting.Initialize(this);
        shooterAnimation.Initialize(this);
        
        UpdateVisual();
    }
    
    public void Initialize(BlockColor color, int bullets)
    {
        blockColor = color;
        bulletCount = bullets;
        maxBullets = bullets;
        
        ChangeToOriginalColor();
        UpdateVisual();
    }
    
    public void UpdateVisual()
    {
        if (bulletCountText != null)
        {
            bulletCountText.text = bulletCount.ToString();
        }
    }
    
    void OnMouseDown()
    {
        
        if (isAnyShooterMoving)
        {
            return;
        }
        
        if (GameManager.Instance?.platformManager == null)
        {
            return;
        }
        
        if (!shooterMovement.IsMoving && bulletCount > 0 && !isShooting)
        {            
            PlatformManager platformManager = GameManager.Instance.platformManager;
            
            int emptySlotIndex = -1;
            for (int i = 0; i < platformManager.platforms.Length; i++)
            {
                if (platformManager.CanMoveToSlot(i))
                {
                    emptySlotIndex = i;
                    break;
                }
            }
            
            if (emptySlotIndex != -1)
            {
                isAnyShooterMoving = true;
                shooterMovement.MoveToSlot(emptySlotIndex);
            }
            else
            {
                ShowOccupiedFeedback();
            }
        }
    }
    public void OnMovementCompleted()
    {
        isAnyShooterMoving = false;
        
        if (SequentialShootingSystem.Instance != null)
        {
            SequentialShootingSystem.Instance.FireAllPlatformShooters();
        }
    }
    
    public void StartShooting()
    {
        StartCoroutine(ShootBullets());
    }
    
    public void StopShooting()
    {
        isShooting = false;
        StopAllCoroutines();
        
        if (shooterTargeting != null)
        {
            shooterTargeting.Cleanup();
        }
        
        if (shooterAnimation != null)
        {
            shooterAnimation.Cleanup();
        }
        
        ChangeToOriginalColor();
    }
    
    private IEnumerator ShootBullets()
    {
        isShooting = true;
        
        while (bulletCount > 0 && isShooting)
        {
            shooterTargeting.CleanupDestroyedTargets();
            
            Block targetBlock = shooterTargeting.FindTargetBlock();

            if (targetBlock != null)
            {
                if (!targetBlock.IsSameColor(blockColor) || !targetBlock.gameObject.activeInHierarchy)
                {
                    shooterTargeting.RemoveTarget(targetBlock);
                    yield return new WaitForSeconds(0.2f);
                    continue;
                }
                
                ShootBullet(targetBlock);
                UpdateVisual();

                yield return new WaitForSeconds(shootDelay + 0.3f);
            }
            else
            {
                ChangeToOriginalColor();
                shooterAnimation.ReturnToOriginalRotation();
                yield return new WaitForSeconds(0.5f);
            }
        }

        if (bulletCount <= 0)
        {
            isShooting = false;
            DestroyShooterBlock();
        }
        else if (!isShooting)
        {
        }
    }
    
    public void ShootBullet(Block targetBlock)
    {
        if (targetBlock != null && bulletPrefab != null)
        {            
            shooterAnimation.RotateTowardsTargetAndShoot(targetBlock.transform, targetBlock);
        }
    }
    
    public void FireBulletAfterRotation(Block targetBlock)
    {
        if (targetBlock == null)
        {
            return;
        }

        if (!targetBlock.gameObject.activeInHierarchy || !targetBlock.IsSameColor(blockColor))
        {
            return;
        }

        Vector3 bulletStartPos = firePoint != null ? firePoint.position : (meshRenderer != null ? meshRenderer.transform.position : transform.position);
        Vector3 bulletTargetPos = targetBlock.transform.position;
        
        shooterAnimation.StartPulseAnimationAndFire(bulletStartPos, bulletTargetPos, targetBlock);
    }
    
    public void FireBulletWithDelay(Vector3 bulletStartPos, Vector3 bulletTargetPos, float delay)
    {
        StartCoroutine(FireBulletWithDelayCoroutine(bulletStartPos, bulletTargetPos, delay));
    }
    
    private IEnumerator FireBulletWithDelayCoroutine(Vector3 bulletStartPos, Vector3 bulletTargetPos, float delay)
    {
        yield return new WaitForSeconds(delay);
                
        GameObject bulletObj = Instantiate(bulletPrefab, bulletStartPos, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        
        if (bullet != null)
        {
            bullet.Initialize(bulletStartPos, bulletTargetPos, OnBulletHit, blockColor, OnBulletWrongHit);
            bulletCount--;
            UpdateVisual();
        }
    }
    
    public void OnBulletHit(Block hitBlock)
    {
        if (hitBlock != null)
        {
            wrongHitCount = 0;
            shooterTargeting.RemoveTarget(hitBlock);
            
            if (hitBlock.gameObject.activeInHierarchy)
            {
                hitBlock.DestroyBlock();
            }
            
            shooterTargeting.RefreshTargets();
        }
    }
    
    public void OnBulletWrongHit()
    {
        wrongHitCount++;
        
        if (wrongHitCount >= MAX_WRONG_HITS)
        {
            return;
        }
        
        bulletCount++;
        if (bulletCount > maxBullets) bulletCount = maxBullets;
        
        UpdateVisual();
    }
    
    public void ChangeToTargetColor(BlockColor targetColor)
    {        
        if (meshRenderer != null)
        {
            Material targetMaterial = GetMaterialForColor(targetColor);
            if (targetMaterial != null)
            {
                meshRenderer.material = targetMaterial;
            }
        }
    }
    
    public void ChangeToOriginalColor()
    {        
        if (meshRenderer != null)
        {
            Material originalMaterial = GetMaterialForColor(blockColor);
            if (originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
        }
    }
    
    private Material GetMaterialForColor(BlockColor color)
    {
        switch (color)
        {
            case BlockColor.Yellow: return yellowMaterial;
            case BlockColor.Blue: return blueMaterial;
            case BlockColor.Red: return redMaterial;
            default: return null;
        }
    }
    
    private void ShowOccupiedFeedback()
    {
        if (meshRenderer != null)
        {
            Color originalColor = meshRenderer.material.color;
            meshRenderer.material.color = Color.red;
            Invoke(nameof(ResetColor), 1f);
        }
        
        transform.DOShakePosition(0.5f, 0.1f, 10, 90, false, true);
    }
    
    private void ResetColor()
    {
        if (meshRenderer != null)
        {
            Material originalMaterial = GetMaterialForColor(blockColor);
            if (originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
        }
    }
    
    private void DestroyShooterBlock()
    {        if (bulletCount > 0 && isShooting)
        {
            return;
        }
        
        shooterMovement?.Cleanup();
        shooterTargeting?.Cleanup();
        shooterAnimation?.Cleanup();
        
        transform.DOKill();
        
        transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                Destroy(gameObject);
            });
    }
    
    public bool IsShooting => isShooting;
    public static bool IsAnyShooterMoving => isAnyShooterMoving;
}
