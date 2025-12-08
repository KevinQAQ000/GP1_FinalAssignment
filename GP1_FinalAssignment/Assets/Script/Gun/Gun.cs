using Cinemachine;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Bullets")]
    public Bullet BulletPrefab;//调用子弹预制体
    public Transform FirePoint;//子弹发射点
    [Header("Reload")]
    public float ReloadTime = 1f;//记录子弹发射后到达下一子弹发射的时间
    public float ReloadTimer = 0f;//每颗子弹的发射间隔

    [Header("Audio")]
    public AudioSource GunAudio;//枪声音频源
    public AudioClip ShootClip;//开火音效   

    [Header("ScreenShake")]
    public CinemachineImpulseSource GunShake;//枪震动 

    [Header("GunType")]
    public GunType gunType;//枪类型

    public enum GunType //枚举枪类型
    {
        Pistol,
        Rifle,
        SubmachineGun,
        Shotgun
    }

    void Update()
    {
        ReloadTime -= Time.deltaTime;//计时器累加 

        if (ReloadTime > 0f) //如果你的子弹倒计时没有归零，是不能发射子弹的
            return;//如果计时器大于0 则返回

        switch (gunType)//根据枪类型选择不同的发射方式
        {
            case GunType.Pistol:
                ReloadTimer = 0.5f;//手枪发射间隔0,5秒
                if (Input.GetMouseButtonDown(0))//鼠标左键按下
                {
                    ReloadTime = ReloadTimer;//重置计时器
                                             //实例化子弹 设置开始位置 和旋转 
                    Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);//实例化子弹 初始化
                    
                }
                break;
            case GunType.Rifle:
                ReloadTimer = 0.1f;//步枪发射间隔0.1秒
                if (Input.GetMouseButton(0))//鼠标左键按下
                {
                    ReloadTime = ReloadTimer;//重置计时器
                                             //实例化子弹 设置开始位置 和旋转 
                    Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);//实例化子弹 初始化

                }
                break;
            case GunType.SubmachineGun:
                ReloadTimer = 0.05f;//冲锋枪发射间隔0.05秒
                if (Input.GetMouseButton(0))//鼠标左键按下
                {
                    ReloadTime = ReloadTimer;//重置计时器
                                             //实例化子弹 设置开始位置 和旋转 
                    Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation);//实例化子弹 初始化

                }
                break;
            case GunType.Shotgun:
                
                break;
        }
        GunAudio.PlayOneShot(ShootClip);//播放开火音效
        GunShake.GenerateImpulse();//枪震动
        //sssss
        //llll

    }
}
