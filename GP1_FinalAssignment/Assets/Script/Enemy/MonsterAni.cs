using UnityEngine;

public class SimpleWalkAnimation : MonoBehaviour
{
    [Header("Leg References")]
    public Transform leftLeg;
    public Transform rightLeg;

    [Header("Walking Settings")]
    public float speed = 5.0f;      // Speed of the oscillation
    public float maxAngle = 30.0f;  // Maximum angle of the swing

    void Update()
    {
        // Use Mathf.Sin to generate a value between -1 and 1 over time
        // Time.time * speed determines the pace of the steps
        float movement = Mathf.Sin(Time.time * speed);

        // Calculate the rotation angle for the current frame
        float angle = movement * maxAngle;

        // Apply rotation
        // When the left leg swings forward, the right leg should swing backward, 
        // which is why the right leg uses the inverse value (-angle)
        leftLeg.localRotation = Quaternion.Euler(0, 0, angle);
        rightLeg.localRotation = Quaternion.Euler(0, 0, -angle);
    }
}