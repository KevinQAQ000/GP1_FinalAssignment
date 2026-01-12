using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Bullet : MonoBehaviour
{
    public Rigidbody Rigidbody;
    [Range(0f, 500f)]
    public float Speed = 10f; // Speed of the bullet
    public AudioClip CasingAudioClip; // Sound effect for the bullet casing hitting the ground
    private AudioSource audioSource; // Reference to the AudioSource component

    private bool hasPlayedSound = false; // Prevents overlapping sound effects from multiple collisions

    // Start is called before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Add an AudioSource component dynamically
        audioSource = gameObject.AddComponent<AudioSource>();

        // Set linear velocity to move forward automatically
        Rigidbody.linearVelocity = transform.forward * Speed;

        // Destroy the bullet after 3 seconds to prevent memory leaks
        Destroy(gameObject, 3f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasPlayedSound)
        {
            hasPlayedSound = true; // Mark as played to prevent repeats
            StartCoroutine(PlayCasingSoundDelayed(0.5f));
        }
    }

    // Coroutine to play sound after a short delay
    IEnumerator PlayCasingSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (CasingAudioClip != null && audioSource != null)
        {
            // Randomize volume for more natural sound
            float randomVol = Random.Range(0.5f, 0.8f);

            // PlayOneShot's second parameter is the volume scale (0.0 to 1.0)
            audioSource.PlayOneShot(CasingAudioClip, randomVol);
        }
    }
}