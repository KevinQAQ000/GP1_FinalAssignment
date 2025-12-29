using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 武器内部音效
/// </summary>
[System.Serializable]
public class SoundClips
{
    public AudioClip shootSound;//开火音效
    public AudioClip silencerShootASound;//消音开火音效
    public AudioClip reloadSoundAmmotLeft;//换弹音效1
    public AudioClip reloadSoundOutfAmmo;//弹夹音效2
    public AudioClip aimSound;//瞄准音效
}

public class AutomaticGun : Weapon
{
    private PlayerController playerController;//玩家控制器
    private Camera mainCamera;//主摄像机
    public float normalFOV = 60f;// 正常视野
    public float aimFOV = 40f;// 瞄准时的视野（越小越近）
    public float fovSpeed = 10f;// FOV 变化速度

    public bool IS_AUTORRIFLE;//是否为自动步枪
    public bool IS_SEMIGUN;//是否为半自动手枪


    public Transform ShootPoint; // 射击点 射线打出的位置
    public Transform BulletShootPoint; // 子弹特效打出的位置
    public Transform CasingBulletSpawnPoint; // 弹壳生成的位置

    [Header("子弹预制体和特效")]
    public Transform bulletPrefab; // 子弹预制体
    public Transform casingPrefab; // 弹壳预制体
    public GameObject bulletHolePrefab; // 在Inspector面板拖入弹孔预制体
    public float bulletHoleLifeTime = 5f; // 弹孔存在时间

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
    public float boltShootOffset; // 射击时拉栓后退的距离（根据模型轴向调整）
    public float boltReturnSpeed;    // 拉栓复位的速度
    private Vector3 boltOriginalLocalPos;  // 记录拉栓初始位置
    public float returnTime = 0.05f;      // 回位花费的时间（秒）

    [Header("换弹动画组件")]
    public Transform magazineObj;//弹匣物体
    public Transform boltObj;//扳机或拉栓物体
    public Vector3 magRelPos = new Vector3(0.25f, -0.7f, 0);//弹匣下移的目标相对位置
    public Vector3 magRelRot = new Vector3(0, 30, 0);//弹匣旋转的目标角度
    public float moveDuration = 0.3f; //动画移动的时长

    [Header("特效")]
    public Light muzzleFlashLight;//枪口光
    private float lightDuraing;//枪huo光持续时间
    public ParticleSystem muzzlePatic;//枪口粒子特效
    public ParticleSystem sparkPatic;//火花粒子特效2
    public int minSparkEmission = 1;//最小火花粒子发射数量
    public int maxSparkEmission = 7;//最大火花粒子发射数量

    [Header("音效")]
    public AudioSource mainAudioSource;//主枪械音频源
    public SoundClips soundClips;//
                                 //

    [Header("UI")]
    public Image[] crossQuarterIgms;//准星四个角的图片
    public float currentExpandingDegree;//当前准心扩展值
    private float crossExpandDegree;//每帧准心扩展值
    public float maxExpandingDegree;//最大准心扩展值
    public Text ammoTextUI;//弹药UI文本
    public Text shootModeTextUI;//射击模式UI文本

    public PlayerController.MovementState state;//玩家移动状态
    private bool isAiming;//是否在瞄准状态
    private Vector3 sniperingRiflePosition; //枪的初始位置
    public Vector3 sniperingRifleOnPosition; //瞄准时枪的位置
    public float aimSpeed = 10f; // 瞄准速度，数值越大越快


    [Header("键位设置")]
    public KeyCode reloadInputName = KeyCode.R;//换弹键位
    public KeyCode inspectInputName = KeyCode.I;//检视键位
    public KeyCode GunShootModeInputName = KeyCode.B;//射击模式切换键位,自动 半自动
    public ShootMode shootingName;//射击模式名称
    private bool GunShootInput;//射击模式切换输入
    private int modeNum;//射击模式编号
    private string shootModeName;//射击模式名称显示

    private bool isReloading = false; // 是否正在换弹中

    public void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();//获取玩家控制器组件
        mainAudioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;//获取主摄像机
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) normalFOV = mainCamera.fieldOfView;
    }

    private void Start()//初始化
    {
        sniperingRiflePosition = transform.localPosition;//记录枪的初始位置
        muzzleFlashLight.enabled = false;//枪口光初始关闭
        crossExpandDegree = 20f;//初始准心扩展值
        maxExpandingDegree = 60f;//最大准心扩展值
        lightDuraing = 0.03f;//枪口光持续时间
        range = 300f;
        bulletForce = 800f;
        BulletLeft = bulletMag * 5;//初始备用子弹数量为弹匣容量的5倍
        currentBullets = bulletMag;//初始当前弹匣子弹数量为弹匣容量
        boltOriginalLocalPos = boltObj.localPosition;//记录拉栓初始位置
        fireTimer = fireRate;

        //根据不同枪械类型设置射击模式
        if (IS_AUTORRIFLE)
        {
            modeNum = 1;
            shootModeName = "Automatic";
            shootingName = ShootMode.AutoRifle;
            UpdateAmmoUI();

        }
        else if (IS_SEMIGUN)
        {
            modeNum = 0;
            shootModeName = "SemiAuto";
            shootingName = ShootMode.SemiGun;
            UpdateAmmoUI();
        }

    }

    private void Update()
    {
        if(IS_AUTORRIFLE)
        {
            //自动步枪射击模式切换
            if (Input.GetKeyDown(GunShootModeInputName) && modeNum != 1)//按下射击模式切换键
            {
                modeNum = 1;
                shootModeName = "Automatic";
                shootingName = ShootMode.AutoRifle;
                UpdateAmmoUI();
            }
            else if (Input.GetKeyDown(GunShootModeInputName) && modeNum != 0)
            {
                modeNum = 0;
                shootModeName = "SemiAuto";
                shootingName = ShootMode.SemiGun;
                UpdateAmmoUI();
            }
            switch (shootingName)
            {
                case ShootMode.AutoRifle://自动步枪模式
                    GunShootInput = Input.GetMouseButton(0);//按住鼠标左键射击
                    fireRate = 0.13f;
                    break;
                case ShootMode.SemiGun://半自动模式
                    GunShootInput = Input.GetMouseButtonDown(0);//按下鼠标左键射击
                    fireRate = 0.2f;
                    break;
            }
        }

        else
        {
            GunShootInput = Input.GetMouseButtonDown(0);
        }

        state = playerController.state;//获取玩家当前移动状态
        if(state == PlayerController.MovementState.Walking 
            && Vector3.SqrMagnitude(playerController.moveDirection)>0
            && state != PlayerController.MovementState.Running
            && state != PlayerController.MovementState.Crouching)//行走状态
        {
            ExpaningCrossUpdate(crossExpandDegree);
        }
        else if(state != PlayerController.MovementState.Walking 
                && state == PlayerController.MovementState.Running
                && state != PlayerController.MovementState.Crouching)//奔跑状态
        {
            ExpaningCrossUpdate(maxExpandingDegree*2);
        }
        else//静止或蹲伏状态
        {
            ExpaningCrossUpdate(crossExpandDegree);
        }

        if(GunShootInput && currentBullets > 0 && !isReloading)
        {
            GunFire();//调用射击方法
        }
        
        if (fireTimer < fireRate)//如果计时器小于射速
        {
            fireTimer += Time.deltaTime;//计时器增加时间
        }

        if (Input.GetKeyDown(reloadInputName) && currentBullets < bulletMag && BulletLeft > 0 && !isReloading)//按下换弹键
        {
            StartCoroutine(ReloadRoutine());//调用换弹携程
        }

        //右键瞄准逻辑
        if (Input.GetMouseButton(1) && !isReloading && !playerController.isRun)//按下检视键
        {
            isAiming = true;
            AimIn();
            transform.localPosition = Vector3.Lerp(transform.localPosition, sniperingRifleOnPosition, Time.deltaTime * aimSpeed);//设置枪的位置为瞄准位置
        }
        else
        {
            isAiming = false;
            AimOut();
            transform.localPosition = Vector3.Lerp(transform.localPosition, sniperingRiflePosition, Time.deltaTime * aimSpeed);//设置枪的位置为初始位置
        }
        //
        SpreadFactor = isAiming ? 0.001f : 0.05f;//根据是否瞄准设置射击扩散因子

        if (mainCamera != null)
        {
            float targetFOV = isAiming ? aimFOV : normalFOV;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovSpeed);
        }

        if (Input.GetKeyDown(inspectInputName))
        {
            
        }

    }

    public override void GunFire()//是枪械射击的实现方法
    {

        if (fireTimer < fireRate || currentBullets <= 0)//如果计时器小于射速 或者 当前弹匣子弹数量小于等于0
        {
            return;//返回 不执行下面的代码
        }
        //触发拉栓射击动画 ---
        StopCoroutine("BoltFireRoutine"); // 如果射速极快，先停止上一次的复位动作
        StartCoroutine(BoltFireRoutine());
        //调用携程
        StartCoroutine(MuzzleFlashLight());//开启枪口光携程
        muzzlePatic.Emit(1);//发射1个枪口粒子特效
        sparkPatic.Emit(Random.Range(minSparkEmission, maxSparkEmission));//发射随机数量的火花粒子特效

        StartCoroutine(Shoot_Cross());//调用射击准心扩展携程

        RaycastHit hit;//射线检测的返回信息
                       //Vector3 shootDirection = ShootPoint.forward;
                       //下面的代码是实现射击的散射效果
                       //shootDirection = shootDirection + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
                       // --- 修改后的射击偏移逻辑 ---
                       // 生成一个基于 SpreadFactor 的随机偏移量
                       //float xSpread = Random.Range(-SpreadFactor, SpreadFactor);
                       //float ySpread = Random.Range(-SpreadFactor, SpreadFactor);

        // 使用 Quaternion 旋转 ShootPoint 的正前方，这样可以确保偏移永远是相对于枪口准心的
        //Vector3 shootDirection = Quaternion.Euler(xSpread * 100f, ySpread * 100f, 0) * ShootPoint.forward;
        //Vector3 offset = ShootPoint.right * Random.Range(-SpreadFactor, SpreadFactor) +
        //         ShootPoint.up * Random.Range(-SpreadFactor, SpreadFactor);

        //// 将偏移累加到前方向量，并归一化
        //Vector3 shootDirection = (ShootPoint.forward + offset).normalized;
        // 计算随机的角度偏移（数值根据 SpreadFactor 调整，这里乘以一个系数来控制扩散弧度）
        float xSpread = Random.Range(-SpreadFactor, SpreadFactor) * 50f;
        float ySpread = Random.Range(-SpreadFactor, SpreadFactor) * 50f;

        // 创建一个旋转增量
        Quaternion spreadRotation = Quaternion.Euler(xSpread, ySpread, 0);

        // 将这个旋转应用到 ShootPoint 的 forward 方向上
        // 注意：顺序必须是 spreadRotation * ShootPoint.forward
        Vector3 shootDirection = spreadRotation * ShootPoint.forward;
        // -----------------------
        Debug.DrawRay(ShootPoint.position, shootDirection * 20f, Color.red, 2f);
        //发出射线 检测是否击中物体
        if (Physics.Raycast(ShootPoint.position, shootDirection, out hit,range))
        {
            GameObject hole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
            hole.transform.SetParent(hit.collider.transform);
            Destroy(hole, bulletHoleLifeTime);

            Transform bullet;

            bullet = Instantiate(bulletPrefab, BulletShootPoint.position, BulletShootPoint.rotation);//实例化子弹特效
            //bullet.GetComponent<Rigidbody>().linearVelocity = (bullet.transform.forward + shootDirection) * bulletForce;//给子弹特效一个速度
            bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletForce;

            //销毁子弹
            Destroy(bullet.gameObject, 0.05f);

            Target target = hit.collider.GetComponent<Target>();//尝试获取被击中物体上的 Target 脚本组件
            if (target != null)
            {
                // 调用之前在 Target 脚本里写好的 TakeDamage 方法
                // 伤害值使用你脚本中定义的 damage 变量，并传入击中点 hit.point
                target.TakeDamage(damage, hit.point);
            }

            Debug.Log("Hit " + hit.collider.name);
        }

        //实例化弹壳特效
        Instantiate(casingPrefab, CasingBulletSpawnPoint.position, CasingBulletSpawnPoint.rotation);


        //画出射线 仅在Scene视图中可见
        //Debug.DrawRay(ShootPoint.position, shootDirection * range, Color.red, 1f);
        mainAudioSource.clip = soundClips.shootSound;//设置开火音效
        mainAudioSource.Play();//播放开火音效
        fireTimer = 0f;//重置计时器
        currentBullets--;//当前弹匣子弹数量减少1
        UpdateAmmoUI();//更新弹药UI
        ExpendCross(30f);//调用准心扩展方法 
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
        for (int i = 0; i < crossQuarterIgms.Length; i++)
        {
            if (crossQuarterIgms[i].gameObject.activeSelf)
                crossQuarterIgms[i].gameObject.SetActive(false);
        }
        //瞄准的时候，摄像机视野变近

    }

    public override void AimOut()
    {
        // 恢复准心图片显示
        for (int i = 0; i < crossQuarterIgms.Length; i++)
        {
            if (!crossQuarterIgms[i].gameObject.activeSelf)
                crossQuarterIgms[i].gameObject.SetActive(true);
        }
    }

    public override void DoReloadAnimation()
    {
        
    }

    public override void Reload()
    {
        if (currentBullets == bulletMag || BulletLeft <= 0)//如果当前弹匣子弹数量等于弹匣容量 或者 当前备用子弹数量小于等于0
        {
            return;//返回 不执行下面的代码
        }
        int bulletsToLoad = bulletMag - currentBullets;//计算需要补充的子弹数量
        int bulletsToReduce= BulletLeft >= bulletsToLoad ? bulletsToLoad : BulletLeft;//计算备弹减少的数量
        BulletLeft -= bulletsToReduce;//减少备用子弹数量
        currentBullets += bulletsToReduce;//增加当前弹匣子弹数量
        UpdateAmmoUI ();//更新弹药UI

    }

    public override void ExpaningCrossUpdate(float expanDegree)
    {
        if (currentExpandingDegree < expanDegree-5)
        {
            ExpendCross(150* Time.deltaTime);
        }
        else if (currentExpandingDegree > expanDegree + 5)
        {
            ExpendCross(-300 * Time.deltaTime);
        }
    }
    /// <summary>
    /// 改变准心开合度，并且记录当前开合度
    /// </summary>
    public void ExpendCross(float add)
    {
        currentExpandingDegree += add;//记录开合度
        currentExpandingDegree = Mathf.Clamp(currentExpandingDegree, 0, maxExpandingDegree);//限制开合度在0到最大值之间

        crossQuarterIgms[0].transform .localPosition = new Vector3(-currentExpandingDegree, 0, 0);//左
        crossQuarterIgms[1].transform.localPosition = new Vector3(currentExpandingDegree, 0, 0);//右
        crossQuarterIgms[2].transform.localPosition = new Vector3(0, currentExpandingDegree, 0);//上
        crossQuarterIgms[3].transform.localPosition = new Vector3(0, -currentExpandingDegree, 0);//下

        
    }

    public void UpdateAmmoUI()//更新弹药UI
    {
        ammoTextUI.text = currentBullets.ToString() + " / " + BulletLeft.ToString();
        shootModeTextUI.text = shootModeName;
    }

    /// <summary>
    /// 调用准心开合度携程 1帧执行5次
    /// 只负责射击时瞬间增加准心开合度
    /// </summary>
    /// <returns></returns>
    public IEnumerator Shoot_Cross()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            ExpendCross(Time.deltaTime * 500);
        }
    }
    private IEnumerator ReloadRoutine()
    {
        if (isReloading) yield break;
        isReloading = true;

        //记录初始位置，确保动画结束后能精准复位
        Vector3 originalMagPos = magazineObj.localPosition;
        Quaternion originalMagRot = magazineObj.localRotation;
        Vector3 originalBoltPos = boltObj.localPosition;

        //播放音效 1
        mainAudioSource.PlayOneShot(soundClips.reloadSoundAmmotLeft);
        yield return new WaitForSeconds(1.0f);

        //弹匣移出
        float elapsed = 0;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            // 数学优化：SmoothStep 曲线（慢入慢出）
            t = t * t * (3f - 2f * t);

            magazineObj.localPosition = Vector3.Lerp(originalMagPos, originalMagPos + magRelPos, t);
            magazineObj.localRotation = Quaternion.Lerp(originalMagRot, Quaternion.Euler(magRelRot), t);
            yield return null;
        }

        yield return new WaitForSeconds(0.15f);

        //弹匣回位
        elapsed = 0;
        float returnDuration = moveDuration * 0.8f; //回位稍微快一点
        mainAudioSource.PlayOneShot(soundClips.reloadSoundOutfAmmo); //移回时立即播放音效2

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            //采用 Sin 曲线，模拟快速插入后的减速停顿感
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            magazineObj.localPosition = Vector3.Lerp(originalMagPos + magRelPos, originalMagPos, t);
            magazineObj.localRotation = Quaternion.Lerp(Quaternion.Euler(magRelRot), originalMagRot, t);
            yield return null;
        }

        //等待扳机动作
        yield return new WaitForSeconds(2f);

        //扳机/拉栓动作
        Vector3 targetBoltPos = originalBoltPos + new Vector3(0, 0, -0.5f);

        //向后拉 (使用反向平方曲线，模拟拉力的阻碍感)
        elapsed = 0;
        float pullBackTime = 0.15f;
        while (elapsed < pullBackTime)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - (elapsed / pullBackTime), 2); // EaseOutQuad
            boltObj.localPosition = Vector3.Lerp(originalBoltPos, targetBoltPos, t);
            yield return null;
        }

        //弹回 (使用超快速度，模拟弹簧释放)
        elapsed = 0;
        float snapBackTime = 0.07f;
        while (elapsed < snapBackTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / snapBackTime;
            //瞬间归位
            boltObj.localPosition = Vector3.Lerp(targetBoltPos, originalBoltPos, t);
            yield return null;
        }

        //确保最终位置完全精准
        magazineObj.localPosition = originalMagPos;
        boltObj.localPosition = originalBoltPos;

        //最终更新数值 ---
        Reload();
        isReloading = false;
    }
    private IEnumerator BoltFireRoutine()
    {
        //瞬间移动到后方（击发瞬间）
        //如果不确定是哪个轴，请在 Inspector 里手动拖动拉栓看坐标变化
        boltObj.localPosition = boltOriginalLocalPos + new Vector3(0, 0, boltShootOffset);

        //停留极短时间（增加肉眼捕捉率）
        yield return new WaitForSeconds(0.01f);

        //丝滑回位
        float elapsed = 0;
        while (elapsed < returnTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnTime;
            //使用 EaseOut 曲线，回位更清脆
            boltObj.localPosition = Vector3.Lerp(boltObj.localPosition, boltOriginalLocalPos, t);
            yield return null;
        }
        boltObj.localPosition = boltOriginalLocalPos;
    }

    public enum ShootMode
    {
        AutoRifle,
        SemiGun
    }

}
