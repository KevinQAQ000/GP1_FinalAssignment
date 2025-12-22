using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract void GunFire();//是枪械射击的抽象方法
    public abstract void Reload();//是枪械装弹的抽象方法
    public abstract void ExpaningCrossUpdate();//是枪械开火后十字准星扩展的抽象方法
    public abstract void DoReloadAnimation();//是枪械装弹动画的抽象方法
    public abstract void AimIn();//是枪械进入瞄准的抽象方法
    public abstract void AimOut();//是枪械退出瞄准的抽象方法
}
