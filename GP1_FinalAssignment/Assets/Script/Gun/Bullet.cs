using UnityEngine;

public class Bullet : MonoBehaviour
{

    public Rigidbody Rigidbody;
    [Range(0f, 100f)]
    public float Speed = 10f; // Speed of the bullet

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
            Rigidbody.linearVelocity = transform.forward * Speed;// 直线速度 自动往前方飞

    }
    

}
