using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 核心引用

/// <summary>
/// character player controller
/// </summary>
public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;//获取角色控制器组件
    public Vector3 moveDirection;//人物移动方向
    private AudioSource audioSource;//音频源组件
    private Vector3 impactVelocity; // 受击产生的瞬时速度

    private float verticalVelocity; // 专门处理 y 轴的速度
    private Vector3 airControlDirection; // 记录起跳瞬间的方向

    [Header("玩家数值")]
    public float Speed;//默认速度
    public float walkSpeed;//行走速度
    public float runSpeed;//奔跑速度
    public float crouchSpeed;//蹲伏速度
    public float playerHealth;//玩家生命值
    public float jumpForce;//跳跃力度
    public float fallForce;//下落速度
    public float crouchHeight;//蹲伏高度
    public float standHeight;//站立高度

    [Header("键位设置")]
    public KeyCode runInputName = KeyCode.LeftShift;//奔跑按键
    public KeyCode jumpInputName = KeyCode.Space;//跳跃按键
    public KeyCode crouchInputName = KeyCode.LeftControl;//蹲伏按键

    [Header("玩家属性判断")]
    public MovementState state;//人物移动状态
    public CollisionFlags collisionFlags;//碰撞标志
    public bool isWalk;//是否行走
    public bool isRun;//是否奔跑
    public bool isJump;//是否跳跃
    public bool isGround;//是否在地面
    public bool isCanCrouch;//是否可以蹲伏
    public bool isCouching;//是否正在蹲伏
    public bool isBlocked;//是否被阻挡
    public bool playerisDead;//玩家是否死亡  
    private bool isDamage;//玩家是否受伤
    public GameObject YouDied;//玩家死亡UI显示

    public LayerMask crouchLayerMask;//地面图层
    public Text playerHealthUI;//玩家生命值UI显示
    public Image hurtImage;//受伤图片显示
    private Color flashColor = new Color(1f, 0f, 0f, 0.5f);//受伤闪烁颜色
    private Color clearColor = Color.clear;//透明颜色

    [Header("音效")]
    public AudioClip walkSound;//行走音效
    public AudioClip runSound;//奔跑音效

    [Header("获胜逻辑")]
    public GameObject YouWinUI; // 获胜 UI 面板
    public string winTagName = "WinObject"; // 获胜触发物体的 Tag
    private float winTimer = 0f; // 计时器
    private bool playerWon = false; // 是否已经获胜

    [Header("悬崖死亡设置")]
    public float deathYThreshold = -10f; // 掉到多深算“掉下悬崖”
    private float fallTimer = 0f;        // 掉落计时器

    void Start()
    {
        characterController = GetComponent<CharacterController>();//获取角色控制器组件
        audioSource = GetComponent<AudioSource>();//获取音频源组件
        walkSpeed = 4f;
        runSpeed = 6f;
        jumpForce = 0f;
        fallForce = 15f;
        crouchSpeed = 3f;
        crouchHeight = 1.2f;
        playerHealth = 100f;
        standHeight = characterController .height;
        playerHealthUI.text = "Hp:" + playerHealth;
        // 确保游戏开始时死亡 UI 是隐藏的
        if (YouDied != null)
        {
            YouDied.gameObject.SetActive(false);
        }
        if (YouWinUI != null) YouWinUI.SetActive(false);
    }

    private bool isTouchingWinObject = false; // 新增变量：是否正触碰获胜物体
    void Update()//60
    {
        //玩家受到伤害后，屏幕产生红色闪烁效果
        if (isDamage)
        {
            hurtImage.color = flashColor;
        }
        else
        {
            hurtImage.color = Color.Lerp(hurtImage.color, clearColor, 5f * Time.deltaTime);//逐渐淡出红色
        }
        isDamage = false;
        if (playerisDead)
        {
            //游戏结束，停止一切操作
            return;
        }

        CheckFallDeath();

        playerHealthUI.text = "Hp:" + playerHealth;
        isCanCrouch = CanCrouch();//判断是否可以蹲伏
        CheckGroundStatus(); // 优化后的地面检测
        Vector3 finalMovement = CalculateMovement();
        characterController.Move(finalMovement * Time.deltaTime);
        if (Input.GetKey(crouchInputName))
        {
            Crouch(true);
        }
        else
        {
            if (isCanCrouch)
            {
                Crouch(false);
            }
            else
            {
                // 被卡住了，維持蹲下狀態
                isCouching = true;
            }
        }
        Jump();
        PlayerFootSoundSet();
        Moving();
        if (playerWon) return;

        // 获胜计时逻辑
        if (isTouchingWinObject)
        {
            winTimer += Time.deltaTime;
            Debug.Log("正在占领...进度: " + (winTimer / 5f * 100f).ToString("F0") + "%");

            if (winTimer >= 5f)
            {
                WinGame();
            }
        }
        else
        {
            // 离开区域后计时器缓慢回落
            winTimer = Mathf.Max(0, winTimer - Time.deltaTime);
        }
    }
    private void FixedUpdate()//50
    {
    }

    /// <summary>
    /// 检测掉下悬崖的逻辑
    /// </summary>
    private void CheckFallDeath()
    {
        // 如果 Y 轴坐标低于阈值
        if (transform.position.y < deathYThreshold)
        {
            fallTimer += Time.deltaTime;
            // 每秒打印一次调试信息
            if (fallTimer > 0) Debug.Log("正在坠落... " + fallTimer.ToString("F1") + "s");

            if (fallTimer >= 3f)
            {
                Die();
                fallTimer = 0f; // 触发后重置
            }
        }
        else
        {
            // 在安全区域时重置计时器
            fallTimer = 0f;
        }
    }

    public void Moving()//人物移动方法
    {
        //float h = Input.GetAxis("Horizontal");//获取水平轴输入
        float h = Input.GetAxisRaw("Horizontal");//急停顿
        //float v = Input.GetAxis("Vertical");//获取垂直轴输入
        float v = Input.GetAxisRaw("Vertical");

        isRun = Input.GetKey(runInputName);//获取奔跑按键输入
        isWalk = (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0) ? true : false;//判断是否行走
        if (isGround)
        {
            // 优先级：蹲下 > 奔跑 > 走路
            if (isCouching)
            {
                state = MovementState.Crouching;
                Speed = crouchSpeed;
            }
            else if (isRun) // 只有在没蹲下且按住Shift时奔跑
            {
                state = MovementState.Running;
                Speed = runSpeed;
            }
            else
            {
                state = MovementState.Walking;
                Speed = walkSpeed;
            }
            //if (isRun && isGround)
            //{
            //    state = MovementState.Running;
            //    Speed = runSpeed;//奔跑速度
            //}
            //else if (isGround)
            //{
            //    state = MovementState.Walking;
            //    Speed = walkSpeed;//行走速度
            //    if (isCouching)
            //    {
            //        Speed = crouchSpeed;//蹲伏速度
            //    }
        }

        //transform.right  * h + transform.forward * v;//根据输入方向移动
        //normalized 归一化 向量长度为1
        //设置人物移动方向，将速度规范化，防止斜向移动过快
        moveDirection = (transform.right * h + transform.forward * v).normalized;//根据输入方向移动
        characterController.Move(moveDirection * Speed * Time.deltaTime);//移动角色控制器

    }

    public void Jump()//人物跳跃方法
    {
        if (!isCanCrouch) return;
        isJump = Input.GetKeyDown(jumpInputName);//获取跳跃按键输入
        if (isJump && isGround)//判断是否跳跃
        {
            isGround = false;
            verticalVelocity = 5f;//跳跃力度
        }
        else if (!isGround && isGround)
        {
            isGround = false;
        }

        if (!isGround)
        {
            jumpForce -= fallForce * Time.deltaTime;//下落速度
            Vector3 jump = new Vector3(0, jumpForce * Time.deltaTime, 0);//将跳跃力度转换成V3坐标
            collisionFlags = characterController.Move(jump * Time.deltaTime);//调用角色控制器移动方法，向上移动模拟跳跃
            Debug.Log("collisionFlags:" + collisionFlags);
            Debug.Log("characterController.isGround:" + characterController.isGrounded);
            if (collisionFlags == CollisionFlags.Below)//判断是否落地
            {
                isGround = true;
                jumpForce = -2f;
            }
    }

        //if (isBlocked)
        //{
        //    return;
        //}
        //else
        //{
        //    isJump = Input.GetKeyDown(jumpInputName);//获取跳跃按键输入
        //    if (isJump && isGround)//判断是否跳跃
        //    {
        //        isGround = false;
        //        jumpForce = 5f;//跳跃力度
        //    }

        //    //人物跳跃逻辑
        //    if (!isGround)
        //    {
        //        jumpForce -= fallForce * Time.deltaTime;//下落速度
        //        Vector3 jump = new Vector3(0, jumpForce * Time.deltaTime, 0);//将跳跃力度转换成V3坐标
        //        collisionFlags = characterController.Move(jump * Time.deltaTime);//调用角色控制器移动方法，向上移动模拟跳跃

        //        //判断玩家在地面
        //        //collisionFlags = characterController.collisionFlags;//获取碰撞标志
        //        if (collisionFlags == CollisionFlags.Below)//判断是否落地
        //        {
        //            isGround = true;
        //            jumpForce = 0f;
        //        }
        //        if (isGround && collisionFlags == CollisionFlags.None)//防止跳跃过程中落地后继续下落
        //        {
        //            isGround = false;
        //        }
        //        isJump = true;
        //    }
        //}

    }

    /// <summary>
    /// 判断是否可以蹲伏
    /// isCanCrouch = true 可以蹲伏
    /// isCanCrouch = false 不可以蹲伏 头顶有障碍物
    /// </summary>
    public bool CanCrouch()//是否可以蹲伏方法 ?
    {
        // 檢測點：從腳底向上偏移（站立高度 - 球體半徑）
        // 這樣球體正好位於站立時「頭部」的位置
        Vector3 sphereLocation = transform.position + Vector3.up * (standHeight - characterController.radius);

        float checkRadius = characterController.radius * 1.2f;
        //绘制出checkSphere检测范围
        isBlocked = Physics.CheckSphere(sphereLocation, checkRadius, crouchLayerMask);

        return !isBlocked; 

        //Vector3 shpereLocation = transform .position + Vector3.up * standHeight;//测试位置 在角色头顶位置
        //isCanCrouch = (Physics.OverlapSphere(shpereLocation, characterController.radius, crouchLayerMask).Length) == 0;//检测头顶是否有障碍物

        ////在场景视图中绘制一个球体，表示检测范围
        //Collider[] colliders = Physics.OverlapSphere(shpereLocation, characterController.radius, crouchLayerMask);
        //当这个球体与任何物体重叠时，说明头顶有障碍物
        //for (int i = 0; i < colliders.Length; i++)
        //{
        //    Debug.Log(colliders[i].name);
        //}
        //Debug.Log(shpereLocation);
    }
    public void Crouch( bool newCrouching)//人物蹲伏方法
    {
        //if(!isCanCrouch) return;//如果不能站立则返回
        isCouching = newCrouching;//设置是否蹲伏状态
        characterController.height = isCouching ? crouchHeight : standHeight;//根据是否可以站立设置角色控制器高度
        characterController.center = Vector3.up * (characterController.height / 2f);//根据角色控制器高度设置中心点位置
        if (isCouching) 
            state = MovementState.Crouching;

        //isCouching = Input.GetKey(crouchInputName);//获取蹲伏按键输入
        //if (isCouching)
        //{
        //    state = MovementState.Crouching;
        //    Speed = crouchSpeed;//蹲伏速度
        //    characterController.height = crouchHeight;//设置角色控制器高度为蹲伏高度
        //}
        //else
        //{
        //    if (isCanCrouch)//判断是否可以站立
        //    {
        //        characterController.height = standHeight;//设置角色控制器高度为站立高度
        //    }
        //}
    }

    private void OnDrawGizmos()//检测范围
    {
        // 设置颜色
        Gizmos.color = Color.blue;

        // 计算位置
        Vector3 shpereLocation = transform.position + Vector3.up * standHeight;

        // 画出这个球
        Gizmos.DrawWireSphere(shpereLocation, characterController.radius);

        // 1. 设置颜色：如果头顶有障碍物变红，否则为绿
        Gizmos.color = CanCrouch() ? Color.green : Color.red;

        // 2. 计算检测球的位置 (必须与逻辑代码一致)
        // 建议位置：脚底坐标 + 站立高度 - 球体半径
        Vector3 sphereLocation = transform.position + Vector3.up * (standHeight - characterController.radius);

        // 3. 画出检测范围
        Gizmos.DrawWireSphere(sphereLocation, characterController.radius * 1.2f);

    }

    /// <summary>
    /// 移动音效设置
    /// </summary>
    public void PlayerFootSoundSet()
    {
        //sqrMagnitude 是将向量的长度平方后返回 V3转换成平方值
        if (isGround && moveDirection.sqrMagnitude > 0)
        {
            audioSource.clip = isRun ? runSound : walkSound;//根据是否奔跑设置音效
            if (!audioSource.isPlaying)//如果音效没有播放
            {
                audioSource.Play();//播放音效
            }
            
        }
        else//如果不在地面或者没有移动
        {
            if (audioSource.isPlaying)//如果音效正在播放
            {
                audioSource.Pause();
            }
        }

    }

    private void CheckGroundStatus()
    {
        // 使用 CharacterController 自带的 isGrounded 结合你的 collisionFlags
        isGround = characterController.isGrounded || (collisionFlags & CollisionFlags.Below) != 0;
    }

    // 核心计算逻辑
    private Vector3 CalculateMovement()
    {
        // 处理水平输入
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = (transform.right * h + transform.forward * v).normalized;

        //状态与速度判定
        isRun = Input.GetKey(runInputName);
        if (isCouching) Speed = crouchSpeed;
        else if (isRun) Speed = runSpeed;
        else Speed = walkSpeed;

        //计算最终移动向量
        Vector3 horizontalMove;

        if (isGround)
        {
            // 在地面时，正常更新移动方向
            horizontalMove = inputDir * Speed;

            // 实时记录当前方向，准备为可能的跳跃做备份
            airControlDirection = horizontalMove;

            // 落地重置垂直速度（-2f 是为了让角色更稳地贴着地面/斜坡）
            if (verticalVelocity < 0) verticalVelocity = -3f;

            // 跳跃逻辑
            if (Input.GetKeyDown(jumpInputName) && !isBlocked)
            {
                verticalVelocity = 6f; // 你的跳跃力度
                isGround = false;
            }
        }
        else
        {
            // 【关键点】在空中时，不使用 inputDir，而是使用起跳瞬间记录的方向
            // 这样玩家在空中按 WASD 就不会改变飞行轨迹了
            horizontalMove = airControlDirection;

            // 持续计算重力
            verticalVelocity -= fallForce * Time.deltaTime;
        }

        if (impactVelocity.magnitude > 0.2f)
        {
            // 将冲击力加入最终移动向量
            // 使用 Lerp 让冲击力迅速衰减
            impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, 5f * Time.deltaTime);
        }
        else
        {
            impactVelocity = Vector3.zero;
        }

        // 合并最终向量 
        return new Vector3(horizontalMove.x, verticalVelocity, horizontalMove.z) + impactVelocity;
    }


    /// <summary>
    /// 玩家生命值
    /// </summary>
    /// <param name="damage"><接受到的伤害/param>
    public void PlayerHealth(float damage)
    {
        if (playerisDead) return; // 防止重复执行死亡逻辑

        playerHealth -= damage;
        isDamage = true;

        // 产生受击反冲效果
        impactVelocity = -transform.forward * 1.5f * 10f;

        playerHealthUI.text = "Hp: " + playerHealth;
        if (playerHealth <= 0 )
        {
            //playerisDead = true;
            //Debug .Log ("Player is Dead!");
            //playerHealthUI.text = "Hp: 0";
            //Time.timeScale = 0f;//暂停游戏
            Die();
        }
    }
    IEnumerator ExecuteAfterTime(float time)//协程等待指定秒数后执行方法
    {
        // 等待指定秒数
        yield return new WaitForSeconds(time);

        // 这里写你要执行的代码
        Debug.Log("2秒时间到！执行逻辑。");
    }
    private void Die()
    {
        playerisDead = true;
        playerHealth = 0;
        playerHealthUI.text = "Hp: 0";

        // --- 新增死亡 UI 显示 ---
        if (YouDied != null)
        {
            YouDied.SetActive(true);
            //YouDied.text = "YOU DIED"; // 或者在 Inspector 里预先写好
            // 释放鼠标，让玩家可以点击按钮
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Debug.Log("Player is Dead!");

        // 注意：Time.timeScale = 0f 会冻结整个游戏，包括 UI 动画
        // 如果想要死亡后有淡入效果，可以不在这里立即暂停
        Time.timeScale = 0f;
    }
    public void ResetStatus(Vector3 targetPos)
    {
        // 1. 恢复时间缩放（必须在移动前，否则某些物理计算会卡住）
        Time.timeScale = 1f;

        // 2. 彻底禁用组件（这是解决“原地复活”的关键）
        characterController.enabled = false;

        // 3. 强制更改位置
        transform.position = targetPos;

        // 4. 重置状态数值
        playerHealth = 100f;
        playerisDead = false;
        verticalVelocity = 0f;
        impactVelocity = Vector3.zero;

        // 5. 更新 UI 显示
        playerHealthUI.text = "Hp: " + playerHealth;

        // 6. 隐藏整个死亡 UI 面板
        if (YouDied != null)
        {
            YouDied.SetActive(false);
        }

        // 7. 重新锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 8. 最后重新开启组件
        characterController.enabled = true;

        Debug.Log("复活成功，目标点: " + targetPos);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(winTagName))
        {
            isTouchingWinObject = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(winTagName))
        {
            isTouchingWinObject = false;
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1f; // 必须恢复时间
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // 加载当前场景
    }
    private void WinGame()
    {
        playerWon = true;
        Debug.Log("You Win!");

        if (YouWinUI != null)
        {
            YouWinUI.SetActive(true);
            Time.timeScale = 0f; // 暂停游戏
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public enum MovementState//人物移动状态
    {
        Walking,
        Running,
        Crouching,
        idle
    }

}
