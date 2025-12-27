using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Bullet : MonoBehaviour
{

    public Rigidbody Rigidbody;
    [Range(0f, 500f)]
    public float Speed = 10f; //Speed of the bullet
    public AudioClip CasingAudioClip;//子弹落地音效
    private AudioSource audioSource;//音频源组件

    private bool hasPlayedSound = false; //防止多次碰撞导致音效重叠
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();//添加音频源组件
        Rigidbody.linearVelocity = transform.forward * Speed;//直线速度 自动往前方飞
        Destroy(gameObject, 3f); //3秒后销毁子弹，防止内存泄漏
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasPlayedSound)
        {
            hasPlayedSound = true; //标记已播放
            StartCoroutine(PlayCasingSoundDelayed(0.5f));

        }
    }

    // 协程函数
    IEnumerator PlayCasingSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (CasingAudioClip != null && audioSource != null)
        {
            //随机化音量
            float randomVol = Random.Range(0.5f, 0.8f);

            //PlayOneShot 的第二个参数就是音量缩放 (0.0 到 1.0)
            audioSource.PlayOneShot(CasingAudioClip, randomVol);
        }
    }


}
