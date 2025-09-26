using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialShootingSystem : MonoBehaviour
{
    private static SequentialShootingSystem _instance;
    public static SequentialShootingSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SequentialShootingSystem");
                _instance = go.AddComponent<SequentialShootingSystem>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    [Header("Shooting Settings")]
    [SerializeField] private float shootingInterval = 0.01f;
    [SerializeField] private float maxWaitTime = 0.5f;
    
    private Queue<ShooterBlock> shootingQueue = new Queue<ShooterBlock>();
    private bool isProcessingQueue = false;
    private Coroutine shootingCoroutine;
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void AddToShootingQueue(ShooterBlock shooter)
    {
        if (shooter == null)
        {
            return;
        }
        
        shootingQueue.Enqueue(shooter);
        
        if (!isProcessingQueue)
        {
            StartProcessingQueue();
        }
    }
    
    private void StartProcessingQueue()
    {
        if (isProcessingQueue) return;
        
        isProcessingQueue = true;
        shootingCoroutine = StartCoroutine(ProcessShootingQueue());
    }
    
    private IEnumerator ProcessShootingQueue()
    {
        float totalWaitTime = 0f;
        
        while (shootingQueue.Count > 0)
        {
            ShooterBlock shooter = shootingQueue.Dequeue();
            
            if (shooter != null && shooter.gameObject.activeInHierarchy)
            {
                shooter.StartShooting();
                
                if (totalWaitTime >= maxWaitTime)
                {
                    break;
                }
                
                yield return new WaitForSeconds(shootingInterval);
                totalWaitTime += shootingInterval;
            }
        }
        
        isProcessingQueue = false;
        shootingCoroutine = null;
    }
    
    public void ClearQueue()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
        
        shootingQueue.Clear();
        isProcessingQueue = false;
    }
    
    public bool IsQueueEmpty => shootingQueue.Count == 0 && !isProcessingQueue;
    
    public int QueueLength => shootingQueue.Count;
    
    public void SetShootingInterval(float interval)
    {
        shootingInterval = Mathf.Max(0.01f, interval);
    }
    
    public void SetMaxWaitTime(float maxTime)
    {
        maxWaitTime = Mathf.Max(0.1f, maxTime);
    }
    
    public void FireAllPlatformShooters()
    {
        if (GameManager.Instance?.platformManager == null)
        {
            return;
        }
        
        var platformShooters = new List<ShooterBlock>();
        
        for (int i = 0; i < GameManager.Instance.platformManager.platforms.Length; i++)
        {
            ShooterBlock shooter = GameManager.Instance.platformManager.GetShooterAtSlot(i);
            
            if (shooter != null && shooter.bulletCount > 0 && !shooter.isShooting)
            {
                platformShooters.Add(shooter);
            }
        }
        
        if (platformShooters.Count == 0)
        {
            return;
        }
        
        StartCoroutine(FireShootersSequentially(platformShooters));
    }
    
    
    private IEnumerator FireShootersSequentially(List<ShooterBlock> shooters)
    {
        float totalWaitTime = 0f;
        
        foreach (ShooterBlock shooter in shooters)
        {
            if (shooter != null && shooter.gameObject.activeInHierarchy)
            {
                if (totalWaitTime >= maxWaitTime)
                {
                    break;
                }
                
                shooter.StartShooting();
                
                yield return new WaitForSeconds(shootingInterval);
                totalWaitTime += shootingInterval;
            }
        }
    }
    
    [ContextMenu("Debug Queue Status")]
    public void DebugQueueStatus()
    {
    }
}
