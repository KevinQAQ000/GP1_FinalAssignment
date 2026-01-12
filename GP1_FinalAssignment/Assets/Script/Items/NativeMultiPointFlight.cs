using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NativeMultiPointFlight : MonoBehaviour
{
    [Header("角色与座位")]
    public Transform seat;
    public float mountSpeed = 3f;

    [Header("飞行路径")]
    public List<Transform> waypoints;
    public float flySpeed = 5f;
    public float turnSpeed = 3f;
    public float arriveDistance = 0.5f;

    [Header("自动返航设置")]
    public float waitTimeAtDestination = 5f; // 到达终点后停多久

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

        // --- 1. 玩家上车 ---
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        player.transform.SetParent(this.transform);

        while (Vector3.Distance(player.transform.localPosition, seat.localPosition) > 0.01f)
        {
            player.transform.localPosition = Vector3.Lerp(player.transform.localPosition, seat.localPosition, Time.deltaTime * mountSpeed);
            player.transform.localRotation = Quaternion.Lerp(player.transform.localRotation, Quaternion.identity, Time.deltaTime * mountSpeed);
            yield return null;
        }

        // --- 2. 载人去程 ---
        yield return StartCoroutine(FlyRoute(false));

        // --- 3. 到达终点：立即放玩家下来 ---
        player.transform.SetParent(null);
        Vector3 currentEuler = player.transform.eulerAngles;
        player.transform.eulerAngles = new Vector3(0f, currentEuler.y, 0f);

        // 确保玩家被推开一点，防止再次碰到触发器
        Vector3 exitPosition = transform.position + (transform.right * 3f) + (Vector3.up * 1f);
        player.transform.position = exitPosition;

        if (cc) cc.enabled = true;
        Debug.Log("已到达终点，玩家已下车。载具开始等待返航...");

        // --- 4. 载具原地等待 ---
        yield return new WaitForSeconds(waitTimeAtDestination);

        // --- 5. 载具自动原路返回 (无人) ---
        yield return StartCoroutine(FlyRoute(true));

        Debug.Log("载具已回到起点，准备下一次任务");
        isFlying = false;
    }

    // 核心飞行逻辑
    IEnumerator FlyRoute(bool reverse)
    {
        // 反向时：从倒数第二个点开始回溯到第0个点
        // 正向时：从第0个点到最后一个点
        int start = reverse ? waypoints.Count - 1 : 0;
        int end = reverse ? -1 : waypoints.Count;
        int step = reverse ? -1 : 1;

        for (int i = start; i != end; i += step)
        {
            Transform targetPoint = waypoints[i];
            while (Vector3.Distance(transform.position, targetPoint.position) > arriveDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, flySpeed * Time.deltaTime);

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