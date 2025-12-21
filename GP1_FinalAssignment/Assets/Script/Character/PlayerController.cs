using UnityEngine;

/// <summary>
/// character player controller
/// </summary>
public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;//获取角色控制器组件
    public Vector3 moveDirection;//人物移动方向
    private AudioSource audioSource;//音频源组件

    [Header("玩家数值")]
    public float Speed;//默认速度
    public float walkSpeed;//行走速度
    public float runSpeed;//奔跑速度
    public float crouchSpeed;//蹲伏速度
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

    public LayerMask crouchLayerMask;//地面图层

    [Header("音效")]
    public AudioClip walkSound;//行走音效
    public AudioClip runSound;//奔跑音效

    void Start()
    {
        characterController = GetComponent<CharacterController>();//获取角色控制器组件
        audioSource = GetComponent<AudioSource>();//获取音频源组件
        walkSpeed = 4f;
        runSpeed = 8f;
        jumpForce = 0f;
        fallForce = 10f;
        crouchSpeed = 3f;
        crouchHeight = 1.2f;
        standHeight = characterController .height;
    }


    void Update()//60
    {
        isCanCrouch = CanCrouch();//判断是否可以蹲伏
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
    }
    private void FixedUpdate()//50
    {
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
        if (isBlocked)
        {
            return;
        }
        else
        {
            isJump = Input.GetKeyDown(jumpInputName);//获取跳跃按键输入
            if (isJump && isGround)//判断是否跳跃
            {
                isGround = false;
                jumpForce = 5f;//跳跃力度
            }
            if (!isGround)
            {
                jumpForce -= fallForce * Time.deltaTime;//下落速度
                Vector3 jump = new Vector3(0, jumpForce, 0);//将跳跃力度转换成V3坐标
                collisionFlags = characterController.Move(jump * Time.deltaTime);//调用角色控制器移动方法，向上移动模拟跳跃

                if (collisionFlags == CollisionFlags.Below)//判断是否落地
                {
                    isGround = true;
                    jumpForce = 0f;
                }
                if (isGround && collisionFlags == CollisionFlags.None)//防止跳跃过程中落地后继续下落
                {
                    isGround = false;
                }
                isJump = true;
            }
        }
            
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

    public enum MovementState//人物移动状态
    {
        Walking,
        Running,
        Crouching,
        idle
    }   
}
