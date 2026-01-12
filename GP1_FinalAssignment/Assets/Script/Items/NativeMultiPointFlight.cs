using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NativeMultiPointFlight : MonoBehaviour
{
    [Header("Character and Seating")]
    public Transform seat;
    public float mountSpeed = 3f;

    [Header("Flight Path")]
    public List<Transform> waypoints;
    public float flySpeed = 5f;
    public float turnSpeed = 3f;
    public float arriveDistance = 0.5f;

    [Header("Auto Return Settings")]
    public float waitTimeAtDestination = 5f; // Duration to wait after arriving at the final point

    private bool isFlying = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFlying)
        {
            StartCoroutine(MainFlightRoutine(other.gameObject));
        }
    }

    IEnumerator MainFlightRoutine(GameObject player)
    {
        isFlying = true;

        // Player boarding logic
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false; // Disable controller to prevent movement conflicts
        player.transform.SetParent(this.transform);

        // Smoothly lerp the player into the seat position
        while (Vector3.Distance(player.transform.localPosition, seat.localPosition) > 0.01f)
        {
            player.transform.localPosition = Vector3.Lerp(player.transform.localPosition, seat.localPosition, Time.deltaTime * mountSpeed);
            player.transform.localRotation = Quaternion.Lerp(player.transform.localRotation, Quaternion.identity, Time.deltaTime * mountSpeed);
            yield return null;
        }

        // Outbound flight with player
        yield return StartCoroutine(FlyRoute(false));

        // Destination reached: Unmount the player immediately
        player.transform.SetParent(null);
        Vector3 currentEuler = player.transform.eulerAngles;
        player.transform.eulerAngles = new Vector3(0f, currentEuler.y, 0f); // Keep player upright

        // Push the player away slightly to prevent re-triggering the flight immediately
        Vector3 exitPosition = transform.position + (transform.right * 3f) + (Vector3.up * 1f);
        player.transform.position = exitPosition;

        if (cc) cc.enabled = true;
        Debug.Log("Destination reached, player unmounted. Vehicle waiting to return...");

        // Vehicle waits at the destination
        yield return new WaitForSeconds(waitTimeAtDestination);

        // Vehicle returns to start automatically (empty)
        yield return StartCoroutine(FlyRoute(true));

        Debug.Log("Vehicle returned to start, ready for next mission.");
        isFlying = false;
    }

    // Core flight logic
    IEnumerator FlyRoute(bool reverse)
    {
        // Reverse: Trace back from the last waypoint to the first (index 0)
        // Forward: Move from the first waypoint to the last
        int start = reverse ? waypoints.Count - 1 : 0;
        int end = reverse ? -1 : waypoints.Count;
        int step = reverse ? -1 : 1;

        for (int i = start; i != end; i += step)
        {
            Transform targetPoint = waypoints[i];
            while (Vector3.Distance(transform.position, targetPoint.position) > arriveDistance)
            {
                // Move towards the target
                transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, flySpeed * Time.deltaTime);

                // Rotate towards the target
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                }
                yield return null;
            }
        }
    }
}