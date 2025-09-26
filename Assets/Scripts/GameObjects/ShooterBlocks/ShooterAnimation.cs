using DG.Tweening;
using UnityEngine;

public class ShooterAnimation : MonoBehaviour, IShooterComponent
{
    [Header("Rotation Settings")]
    public float rotationDuration = 0.3f;
    public Ease rotationEase = Ease.OutQuart;
    public bool useLocalRotation = true;
    public bool lockXAxis = true;
    public bool lockZAxis = true;
    
    [Header("Pulse Animation Settings")]
    public float pulseScale = 1.1f;
    public float pulseDuration = 0.2f;
    public Ease pulseEase = Ease.OutBack;
    
    private ShooterBlock shooter;
    private bool isAnimating = false;
    private bool isRotating = false;
    private Vector3 originalShooterRotation;
    
    public void Initialize(ShooterBlock shooterBlock)
    {
        shooter = shooterBlock;
        
        if (shooter.meshRenderer != null)
        {
            if (useLocalRotation && shooter.meshRenderer.transform.parent != null)
            {
                originalShooterRotation = shooter.meshRenderer.transform.localRotation.eulerAngles;
            }
            else
            {
                originalShooterRotation = shooter.meshRenderer.transform.rotation.eulerAngles;
            }
        }
    }
    
    public void Cleanup()
    {
        if (shooter.meshRenderer != null)
        {
            shooter.meshRenderer.transform.DOKill();
        }
        transform.DOKill();
        isAnimating = false;
        isRotating = false;
    }
    
    public void RotateTowardsTargetAndShoot(Transform targetTransform, Block targetBlock)
    {
        if (targetBlock == null || shooter.meshRenderer == null)
        {
            return;
        }

        if (isAnimating || isRotating)
        {
            shooter.meshRenderer.transform.DOKill();
            isAnimating = false;
            isRotating = false;
        }

        isAnimating = true;
        isRotating = true;

        Vector3 globalDirectionToTarget = (targetTransform.position - shooter.meshRenderer.transform.position).normalized;
        
        bool shouldUseLocal = useLocalRotation && shooter.meshRenderer.transform.parent != null;
        
        if (shouldUseLocal)
        {
            Vector3 localDirectionToTarget = shooter.meshRenderer.transform.parent.InverseTransformDirection(globalDirectionToTarget);
            
            float targetYRotation = Mathf.Atan2(localDirectionToTarget.x, localDirectionToTarget.z) * Mathf.Rad2Deg;
            
            Vector3 currentLocalEuler = shooter.meshRenderer.transform.localRotation.eulerAngles;
            
            Vector3 targetLocalEuler = new Vector3(
                lockXAxis ? currentLocalEuler.x : 0,
                targetYRotation,
                lockZAxis ? currentLocalEuler.z : 0
            );
            
            shooter.meshRenderer.transform.DOLocalRotate(targetLocalEuler, rotationDuration)
                .SetEase(rotationEase)
                .OnComplete(() => {
                    isRotating = false;
                    shooter.FireBulletAfterRotation(targetBlock);
                });
        }
        else
        {
            Quaternion targetGlobalRotation = Quaternion.LookRotation(globalDirectionToTarget);
            
            Vector3 targetEulerAngles = targetGlobalRotation.eulerAngles;
            Vector3 currentGlobalEuler = shooter.meshRenderer.transform.rotation.eulerAngles;
            
            if (lockXAxis) targetEulerAngles.x = currentGlobalEuler.x;
            if (lockZAxis) targetEulerAngles.z = currentGlobalEuler.z;
            
            targetGlobalRotation = Quaternion.Euler(targetEulerAngles);

            shooter.meshRenderer.transform.DORotateQuaternion(targetGlobalRotation, rotationDuration)
                .SetEase(rotationEase)
                .OnComplete(() => {
                    isRotating = false;
                    shooter.FireBulletAfterRotation(targetBlock);
                });
        }
    }
    
    public void StartPulseAnimationAndFire(Vector3 bulletStartPos, Vector3 bulletTargetPos, Block targetBlock)
    {
        if (targetBlock == null)
        {
            return;
        }
        
        Vector3 originalScale = transform.localScale;
        Vector3 pulseScaleVector = originalScale * pulseScale;
        
        transform.DOKill();
        
        Sequence pulseSequence = DOTween.Sequence();
        pulseSequence.Append(transform.DOScale(pulseScaleVector, pulseDuration * 0.5f).SetEase(pulseEase))
                    .Append(transform.DOScale(originalScale, pulseDuration * 0.5f).SetEase(Ease.InOutQuad))
                    .OnComplete(() => {
                        isAnimating = false;
                        isRotating = false;
                    });
        
        shooter.FireBulletWithDelay(bulletStartPos, bulletTargetPos, pulseDuration * 0.3f);
    }
    
    public void ReturnToOriginalRotation()
    {
        if (shooter.meshRenderer == null)
        {
            return;
        }

        if (isAnimating || isRotating)
        {
            shooter.meshRenderer.transform.DOKill();
            isAnimating = false;
            isRotating = false;
        }

        Vector3 targetRotation = originalShooterRotation;
        
        isAnimating = true;
        isRotating = true;
        
        if (useLocalRotation && shooter.meshRenderer.transform.parent != null)
        {
            shooter.meshRenderer.transform.DOLocalRotate(targetRotation, rotationDuration)
                .SetEase(rotationEase)
                .OnComplete(() => {
                    isAnimating = false;
                    isRotating = false;
                });
        }
        else
        {
            shooter.meshRenderer.transform.DORotate(targetRotation, rotationDuration)
                .SetEase(rotationEase)
                .OnComplete(() => {
                    isAnimating = false;
                    isRotating = false;
                });
        }
    }
    
    public bool IsAnimating => isAnimating;
}
