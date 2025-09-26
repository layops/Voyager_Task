using System.Collections.Generic;
using UnityEngine;

public class ShooterTargeting : MonoBehaviour, IShooterComponent
{
    private ShooterBlock shooter;
    
    private HashSet<Block> targetedBlocks = new HashSet<Block>();
    
    private List<Block> availableTargets = new List<Block>();
    
    public void Initialize(ShooterBlock shooterBlock)
    {
        shooter = shooterBlock;
    }
    
    public void Cleanup()
    {
        targetedBlocks.Clear();
        availableTargets.Clear();
    }
    
    public Block FindTargetBlock()
    {
        if (GameManager.Instance == null)
        {
            return null;
        }

        CleanupDestroyedTargets();
        
        UpdateAvailableTargets();
        
        if (availableTargets.Count == 0)
        {
            return FindExistingTarget();
        }
        
        if (availableTargets.Count > 0)
        {
            int randomIndex = Random.Range(0, availableTargets.Count);
            Block selectedTarget = availableTargets[randomIndex];
            
            if (selectedTarget != null && selectedTarget.gameObject.activeInHierarchy && selectedTarget.IsSameColor(shooter.blockColor))
            {
                targetedBlocks.Add(selectedTarget);
                shooter.ChangeToTargetColor(selectedTarget.blockColor);
                
                return selectedTarget;
            }
            else
            {
                availableTargets.RemoveAt(randomIndex);
                return FindTargetBlock();
            }
        }
        
        return null;
    }
    private void UpdateAvailableTargets()
    {
        availableTargets.Clear();
        
        for (int x = 0; x < 10; x++)
        {
            Block block = GameManager.Instance.GetBlockAt(x, 0);
            if (block != null && block.IsSameColor(shooter.blockColor) && !targetedBlocks.Contains(block))
            {
                availableTargets.Add(block);
            }
        }
    }
    
    private Block FindExistingTarget()
    {
        List<Block> aliveTargets = new List<Block>();
        
        foreach (Block target in targetedBlocks)
        {
            if (target != null && target.gameObject.activeInHierarchy)
            {
                if (target.IsSameColor(shooter.blockColor))
                {
                    aliveTargets.Add(target);
                }
            }
        }
        
        if (aliveTargets.Count > 0)
        {
            int randomIndex = Random.Range(0, aliveTargets.Count);
            Block selectedTarget = aliveTargets[randomIndex];
            return selectedTarget;
        }
        
        shooter.ChangeToOriginalColor();
        return null;
    }
    
    public void CleanupDestroyedTargets()
    {
        var targetsToRemove = new List<Block>();
        
        foreach (Block target in targetedBlocks)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                targetsToRemove.Add(target);
            }
            else if (!target.IsSameColor(shooter.blockColor))
            {
                targetsToRemove.Add(target);
            }
        }
        
        foreach (Block target in targetsToRemove)
        {
            targetedBlocks.Remove(target);
        }
    }
    
    public void RemoveTarget(Block target)
    {
        targetedBlocks.Remove(target);
    }
    
    public void RefreshTargets()
    {
        UpdateAvailableTargets();
    }
}
