using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public int weaponID; // 在面板里，物体A填0，物体B填1

    [Header("旋转设置")]
    public float rotationSpeed = 50f; // 旋转速度，可以在 Inspector 面板调整

    void Update()
    {
        // 沿着世界坐标的 Y 轴（上下轴）进行旋转
        // Space.World 确保它始终绕着垂直于地面的轴转，不受物体自身倾斜的影响
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检测是否是玩家碰到了
        if (other.CompareTag("Player"))
        {
            // 调用玩家身上的管理脚本
            WeaponManager manager = other.GetComponent<WeaponManager>();

            if (manager != null)
            {
                manager.UnlockWeapon(weaponID);
                // 销毁地上的拾取物
                Destroy(gameObject);
            }
        }
    }
}