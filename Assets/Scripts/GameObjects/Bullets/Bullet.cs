using UnityEngine;
using DG.Tweening;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 10f;
    public float lifetime = 3f;
    public float detectionDistance = 0.8f;
    
    private Vector3 targetPosition;
    private System.Action<Block> onHitCallback;
    private System.Action onWrongHitCallback;
    private bool hasHit = false;
    private BlockColor shooterColor;
    
    public void Initialize(Vector3 startPos, Vector3 targetPos, System.Action<Block> onHit, BlockColor shooterBlockColor, System.Action onWrongHit = null)
    {
        transform.position = startPos;
        targetPosition = targetPos;
        onHitCallback = onHit;
        onWrongHitCallback = onWrongHit;
        shooterColor = shooterBlockColor;
        
        MoveToTarget();
        Destroy(gameObject, lifetime);
    }
    
    void MoveToTarget()
    {
        float distance = Vector3.Distance(transform.position, targetPosition);
        float duration = distance / speed;
        
        transform.DOMove(targetPosition, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                if (!hasHit)
                {
                    CheckForNearbyBlocks();
                    
                    if (!hasHit)
                    {
                        DestroyBullet();
                    }
                }
            });
    }
    
    void Update()
    {
        if (!hasHit && Time.frameCount % 3 == 0)
        {
            CheckForNearbyBlocks();
        }
    }
    
    void CheckForNearbyBlocks()
    {
        if (GameManager.Instance == null) 
        {
            return;
        }
        
        Vector3 currentPos = transform.position;
        int currentGridX = Mathf.RoundToInt(currentPos.x);
        int currentGridY = Mathf.RoundToInt(currentPos.y);
        
        if (currentGridX >= 0 && currentGridX < 10 && currentGridY >= 0 && currentGridY < 15)
        {
            Block nearbyBlock = GameManager.Instance.GetBlockAt(currentGridX, currentGridY);
            
            if (nearbyBlock != null)
            {
                Vector3 blockWorldPos = nearbyBlock.transform.position;
                float distance = Vector3.Distance(transform.position, blockWorldPos);
                
                    if (distance <= detectionDistance)
                    {
                        bool isSameColor = nearbyBlock.IsSameColor(shooterColor);
                        
                        if (isSameColor)
                        {
                            hasHit = true;
                            
                            if (onHitCallback != null)
                            {
                                onHitCallback(nearbyBlock);
                            }
                            
                            DestroyBullet();
                            return;
                        }
                        else
                        {
                            if (onWrongHitCallback != null)
                            {
                                onWrongHitCallback();
                            }
                            
                            DestroyBullet();
                            return;
                        }
                    }
            }
        }
    }

    void DestroyBullet()
    {
        if (!hasHit)
        {
            hasHit = true;
            transform.DOKill();
            Destroy(gameObject);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(targetPosition, Vector3.one * 0.2f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, targetPosition);
    }
}
