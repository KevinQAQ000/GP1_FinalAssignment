using System.Collections;
using UnityEngine;

public class AutomaticGun : Weapon
{
    public Transform ShootPoint; // 射击点 射线打出的位置
    public Transform BulletShootPoint; // 子弹特效打出的位置
    public Transform CasingBulletSpawnPoint; // 弹壳生成的位置

    [Header("枪械属性")]
    public float range; // 射程
    public float fireRate; // 射速
    public float damage; // 伤害
    private float originRate; // 原始射速 用于瞄准时修改射速
    private float SpreadFactor;//射击的时候的扩散因子 偏移量
    private float fireTimer; // 射击计时器 用于控制射速
    private float bulletForce; // 子弹的力 用于推动子弹特效
    public int bulletMag;//弹匣容量
    public int currentBullets;//当前弹匣子弹数量
    public int BulletLeft;//当前备用子弹数量

    [Header("特效")]
    public Light muzzleFlashLight;//枪口光
    private float lightDuraing;//枪huo光持续时间
    public ParticleSystem muzzlePatic;//枪口粒子特效
    public ParticleSystem sparkPatic;//火花粒子特效2
    public int minSparkEmission = 1;//最小火花粒子发射数量
    public int maxSparkEmission = 7;//最大火花粒子发射数量

    private void Start()//初始化
    {
        muzzleFlashLight.enabled = false;//枪口光初始关闭
        lightDuraing = 0.03f;//枪口光持续时间
        range = 300f;
        BulletLeft = bulletMag * 5;//初始备用子弹数量为弹匣容量的5倍
        currentBullets = bulletMag;//初始当前弹匣子弹数量为弹匣容量

    }

    private void Update()
    {
        //计时器
        if (fireTimer < fireRate)//如果计时器小于射速
        {
            fireTimer += Time.deltaTime;//计时器增加时间
        }

        if (Input.GetMouseButton(0))//按住鼠标左键射击
        {
            GunFire();//调用射击方法
        }
        
    }

    public override void GunFire()//是枪械射击的实现方法
    {

        if (fireTimer < fireRate || currentBullets <= 0)//如果计时器小于射速 或者 当前弹匣子弹数量小于等于0
        {
            return;//返回 不执行下面的代码
        }

        //调用携程
        StartCoroutine(MuzzleFlashLight());//开启枪口光携程
        muzzlePatic.Emit(1);//发射1个枪口粒子特效
        sparkPatic.Emit(Random.Range(minSparkEmission, maxSparkEmission));//发射随机数量的火花粒子特效
        RaycastHit hit;//射线检测的返回信息
        Vector3 shootDirection = ShootPoint.forward;
        //下面的代码是实现射击的散射效果
        shootDirection = shootDirection + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
        if (Physics.Raycast(ShootPoint.position, shootDirection, out hit,range))
        {
            Debug.Log("Hit " + hit.collider.name);
        }
        //画出射线 仅在Scene视图中可见
        Debug.DrawRay(ShootPoint.position, shootDirection * range, Color.red, 1f);
        fireTimer = 0f;//重置计时器
        currentBullets--;//当前弹匣子弹数量减少1
    }

    /// <summary>
    /// 设置开火灯光
    /// </summary>
    public IEnumerator MuzzleFlashLight()
    {
        muzzleFlashLight.enabled = true;
        yield return new WaitForSeconds(lightDuraing);//等待一段时间
        muzzleFlashLight.enabled = false;
    }

    public override void AimIn()
    {
        
    }

    public override void AimOut()
    {
        
    }

    public override void DoReloadAnimation()
    {
        
    }

    public override void ExpaningCrossUpdate()
    {
        
    }

    public override void Reload()
    {
        
    }
}
