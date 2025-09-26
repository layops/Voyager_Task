using UnityEngine;

public interface IShooterComponent
{
    void Initialize(ShooterBlock shooter);
    void Cleanup();
}
