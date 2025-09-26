using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Settings")]
    public BlockColor blockColor;
    public int gridX;
    public int gridY;
    
    [Header("Visual")]
    public MeshRenderer meshRenderer;
    public Material[] colorMaterials; // Farklı renkler için material'lar
    
    [Header("Animation")]
    public float destroyAnimationTime = 0.3f;
    
    private bool isDestroyed = false;
    
    public void Initialize(int x, int y)
    {
        gridX = x;
        gridY = y;
        
        blockColor = (BlockColor)Random.Range(0, System.Enum.GetValues(typeof(BlockColor)).Length);
        SetVisualColor();
    }
    
    public void InitializeWithColor(int x, int y, BlockColor color)
    {
        gridX = x;
        gridY = y;
        blockColor = color;
        SetVisualColor();
    }
    
    void SetVisualColor()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
            
        if (meshRenderer != null && colorMaterials != null && colorMaterials.Length > 0)
        {
            switch (blockColor)
            {
                case BlockColor.Yellow:
                    if (colorMaterials.Length > 0)
                        meshRenderer.material = colorMaterials[0];
                    break;
                case BlockColor.Blue:
                    if (colorMaterials.Length > 1)
                        meshRenderer.material = colorMaterials[1];
                    break;
                case BlockColor.Red:
                    if (colorMaterials.Length > 2)
                        meshRenderer.material = colorMaterials[2];
                    break;
            }
        }
    }
    
    public void DestroyBlock()
    {
        if (isDestroyed) 
        {
            return;
        }
        
        isDestroyed = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBlockDestroyed(gridX, gridY);
        }
        
        StartCoroutine(DestroyAnimation());
    }
    
    System.Collections.IEnumerator DestroyAnimation()
    {
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        
        while (elapsedTime < destroyAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / destroyAnimationTime;
            
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            
            if (meshRenderer != null)
            {
                Color color = meshRenderer.material.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                meshRenderer.material.color = color;
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    public bool IsSameColor(BlockColor otherColor)
    {
        return blockColor == otherColor;
    }
}

public enum BlockColor
{
    Yellow,
    Blue,
    Red
}