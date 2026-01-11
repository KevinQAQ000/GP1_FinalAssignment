using UnityEngine;

public class HitPoint : MonoBehaviour
{
    public int MAX_Damage;
    public int MIN_Damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enemy hit Player");
            other.GetComponent<PlayerController>().PlayerHealth(Random.Range(MIN_Damage, MAX_Damage));
        }
    }

}
