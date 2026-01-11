using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NativeMultiPointFlight : MonoBehaviour
{
    [Header("角色与座位")]
    public Transform seat;           // 角色在车内的固定位置
    public float mountSpeed = 3f;      // 角色坐下的速度

    [Header("飞行路径")]
    public List<Transform> waypoints; // 所有的路点
    public float flySpeed = 5f;       // 飞行速度
    public float turnSpeed = 3f;      // 转向平滑度
    public float arriveDistance = 0.5f; // 到达阈值

    private bool isFlying = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFlying)
        {
            StartCoroutine(StartFlightProcess(other.gameObject));
        }
    }

    IEnumerator StartFlightProcess(GameObject player)
    {
        isFlying = true;

        // 1. 玩家进入逻辑
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        player.transform.SetParent(this.transform);

        // 平滑上车
        while (Vector3.Distance(player.transform.localPosition, seat.localPosition) > 0.01f)
        {
            player.transform.localPosition = Vector3.Lerp(player.transform.localPosition, seat.localPosition, Time.deltaTime * mountSpeed);
            player.transform.localRotation = Quaternion.Lerp(player.transform.localRotation, Quaternion.identity, Time.deltaTime * mountSpeed);
            yield return null;
        }

        // 2. 逐点飞行逻辑
        foreach (Transform targetPoint in waypoints)
        {
            while (Vector3.Distance(transform.position, targetPoint.position) > arriveDistance)
            {
                // 位移
                transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, flySpeed * Time.deltaTime);

                // 转向优化：强制向上向量为 Vector3.up，防止飞行器自身倾斜
                Vector3 direction = (targetPoint.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                }
                yield return null;
            }
        }

        // 3. 到达终点下车逻辑
        // 脱离父子关系
        player.transform.SetParent(null);

        // --- 核心修正：修正角色旋转角度 ---
        // 获取当前欧拉角，只保留 Y 轴，强制将 X 和 Z 轴归零（防止歪斜）
        Vector3 currentEuler = player.transform.eulerAngles;
        player.transform.eulerAngles = new Vector3(0f, currentEuler.y, 0f);
        // ------------------------------

        // 强制将玩家瞬移到车外空地
        Vector3 exitPosition = transform.position + (transform.right * 3f) + (Vector3.up * 1f);
        player.transform.position = exitPosition;

        // 恢复控制
        if (cc) cc.enabled = true;

        Debug.Log("到达终点，角色已扶正并下车");

        // 防止瞬间再次触发
        yield return new WaitForSeconds(5f);
        isFlying = false;
    }
}