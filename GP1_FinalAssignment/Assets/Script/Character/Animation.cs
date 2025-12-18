using UnityEngine;

public class Animation : MonoBehaviour
{
    PlayerController playerController;
    public GameObject PivotPoint;
    public float smoothSpeed = 10f; // 平滑速度，数值越大缩放越快

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (PivotPoint == null)
        {
            PivotPoint = transform.Find("PivotPoint").gameObject;
        }
    }

    private void Update()
    {
        Vector3 targetScale;
        if (playerController.isCouching)
        {
            targetScale = new Vector3(1f, 0.5f, 1f);
        }
        else
        {
            targetScale = new Vector3(1f, 1f, 1f);
        }

        // 从当前的 localScale 逐渐移动到 targetScale
        PivotPoint.transform.localScale = Vector3.Lerp
        (
            PivotPoint.transform.localScale,
            targetScale, Time.deltaTime * smoothSpeed
        );
    }
}
