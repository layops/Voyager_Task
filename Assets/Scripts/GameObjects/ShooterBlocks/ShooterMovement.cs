using DG.Tweening;
using UnityEngine;

public class ShooterMovement : MonoBehaviour, IShooterComponent
{
    [Header("Movement Settings")]
    public float moveDuration = 0.5f;
    public Ease moveEase = Ease.OutCubic;
    
    private ShooterBlock shooter;
    private bool isMoving = false;
    private Vector3 originalPosition;
    private int reservedSlotIndex = -1;
    
    public void Initialize(ShooterBlock shooterBlock)
    {
        shooter = shooterBlock;
        originalPosition = transform.position;
    }
    
    public void Cleanup()
    {
        transform.DOKill();
        isMoving = false;
    }
    
    public void MoveToSlot(int slotIndex)
    {
        if (isMoving) return;
        
        if (GameManager.Instance?.platformManager == null)
        {
            return;
        }
        
        Vector3 targetPos = GameManager.Instance.platformManager.GetSlotPosition(slotIndex);
        MoveToTargetWithPlatformManager(targetPos, slotIndex);
    }
    
    private void MoveToTargetWithPlatformManager(Vector3 targetPos, int slotIndex)
    {
        
        isMoving = true;
        reservedSlotIndex = slotIndex;
        
        GameManager.Instance.platformManager.MarkSlotAsOccupied(slotIndex, shooter);
        
        transform.DOMove(targetPos, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() => {
                isMoving = false;                
                GameManager.Instance.platformManager.CompleteMovement(slotIndex, shooter);
                reservedSlotIndex = -1;
                
                RepositionRemainingShooters();
                
                shooter.OnMovementCompleted();
            });
    }
    
    private void RepositionRemainingShooters()
    {
        ShooterBlockManager shooterManager = FindFirstObjectByType<ShooterBlockManager>();
        if (shooterManager != null)
        {
            shooterManager.RepositionShooterBlocks();
        }
    }
    
    public bool IsMoving => isMoving;
    public int ReservedSlotIndex => reservedSlotIndex;
}
