using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    // Abstract method for handling the weapon firing logic
    public abstract void GunFire();

    // Abstract method for handling the weapon reloading logic
    public abstract void Reload();

    // Abstract method to update the crosshair expansion based on a given degree
    public abstract void ExpaningCrossUpdate(float expanDegree);

    // Abstract method to trigger the reload animation
    public abstract void DoReloadAnimation();

    // Abstract method for entering the Aim-Down-Sights (ADS) state
    public abstract void AimIn();

    // Abstract method for exiting the Aim-Down-Sights (ADS) state
    public abstract void AimOut();
}