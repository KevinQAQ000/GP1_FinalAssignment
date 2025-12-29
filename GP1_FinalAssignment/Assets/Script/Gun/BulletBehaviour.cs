using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public GameObject hitEffectPrefab; // 在编辑器里拖入火花特效
    // 当子弹碰到带有 Collider 的物体时触发
    private void OnCollisionEnter(Collision collision)
    {
        if (hitEffectPrefab != null)
        {
            ContactPoint contact = collision.contacts[0];
            Instantiate(hitEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
        }
        Destroy(gameObject);
    }
}