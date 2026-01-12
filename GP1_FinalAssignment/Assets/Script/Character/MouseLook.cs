using UnityEngine;

/// <summary>
/// Camera rotation
/// 玩家左右旋转的时候实现左右移动
/// 相机上下旋转控制视线上下
/// </summary>
public class MouseLook : MonoBehaviour
{
    private float mouseSensitivity = 400f;//Mouse sensitivity
    private Transform playerBody;
    private float yRotation = 0f;//Camera vertical rotation angle

    public CharacterController characterController;
    public float height = 2f;
    private float interpolationSpeed = 12f;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;//Lock the mouse before starting the game
        //The player controller is on the parent object, therefore it obtains the parent object's controller component.
        playerBody = transform.GetComponentInParent<PlayerController>().transform;
        characterController = GetComponentInParent<CharacterController>();
    }

    
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;//Get mouse X-axis input, left and right
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;//Get mouse Y-axis input, up and down

        yRotation -= mouseY;//The camera's vertical rotation angle is calculated by accumulating the vertical rotation axes.
        yRotation = Mathf.Clamp(yRotation, -60f, 60f);
        transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);//Setting partial camera rotation only affects vertical rotation.
        playerBody.Rotate(Vector3.up * mouseX);//Allow the player's body to rotate around the Y-axis, achieving left and right rotation.

        //Make the camera follow the character's height change
        float heightTarget = characterController.height * 1f;
        height = Mathf.Lerp(height, heightTarget, Time.deltaTime * interpolationSpeed);
        transform.localPosition = Vector3.up * height;
    }
}
