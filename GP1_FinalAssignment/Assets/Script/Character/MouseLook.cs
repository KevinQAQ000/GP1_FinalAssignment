using UnityEngine;

/// <summary>
/// Camera rotation
/// 玩家左右旋转的时候实现左右移动
/// 相机上下旋转控制视线上下
/// </summary>
public class MouseLook : MonoBehaviour
{
    private float mouseSensitivity = 400f;//鼠标灵敏度
    private Transform playerBody;//玩家位置
    private float yRotation = 0f;//摄像机垂直上下旋转角度

    public CharacterController characterController;
    public float height = 2f;
    private float interpolationSpeed = 12f;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;//开始游戏前锁定鼠标
        //playercontroller在父物体上，因此获取父物体的控制器组件
        playerBody = transform.GetComponentInParent<PlayerController>().transform;
        characterController = GetComponentInParent<CharacterController>();
    }

    
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;//获取鼠标X轴输入，左右
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;//获取鼠标Y轴输入，上下

        yRotation -= mouseY;//摄像机垂直旋转角度 将上下旋转的轴累计
        yRotation = Mathf.Clamp(yRotation, -60f, 60f);//限制摄像机垂直旋转角度，防止过度旋转
        transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);//设置摄像机的局部旋转，只影响上下旋转
        playerBody.Rotate(Vector3.up * mouseX);//让玩家身体绕Y轴旋转，实现左右旋转

        //让摄像机跟随角色高度变化
        float heightTarget = characterController.height * 1f;//目标高度
        height = Mathf.Lerp(height, heightTarget, Time.deltaTime * interpolationSpeed);//平滑过渡高度
        transform.localPosition = Vector3.up * height;//设置摄像机位置
    }
}
