using UnityEngine;

/// <summary>
/// character player controller
/// </summary>
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;//获取角色控制器组件
    public Vector3 moveDirection;//人物移动方向

    [Header("玩家数值")]
    public float Speed;//默认速度
    public float walkSpeed;//行走速度
    public float runSpeed;//奔跑速度
    public float crouchSpeed;//蹲伏速度
    public float jumpForce;//跳跃力度
    public float fallForce;//下落速度

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
    public bool isCrouch;//是否蹲伏

    void Start()
    {
        characterController = GetComponent<CharacterController>();//获取角色控制器组件
        walkSpeed = 4f;
        runSpeed = 8f;
        jumpForce = 0f;
        fallForce = 10f;
        crouchSpeed = 2f;
    }


    void Update()//60
    {
        Jump();
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
        if (isRun && isGround)
        {
            state = MovementState.Running;
            Speed = runSpeed;//奔跑速度
        }
        else if (isGround)
        {
            state = MovementState.Walking;
            Speed = walkSpeed;//行走速度
        }

        //transform.right  * h + transform.forward * v;//根据输入方向移动
        //normalized 归一化 向量长度为1
        //设置人物移动方向，将速度规范化，防止斜向移动过快
        moveDirection = (transform.right * h + transform.forward * v).normalized;//根据输入方向移动
        characterController.Move(moveDirection * Speed * Time.deltaTime);//移动角色控制器

    }

    public void Jump()//人物跳跃方法
    {
        //待补充
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
