using UnityEngine;

public class Animation : MonoBehaviour
{
    PlayerController playerController;
    public GameObject PivotPoint;
    public float smoothSpeed = 10f; // Smoothing speed; the higher the value, the faster the scaling.

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

        // Gradually move from the current localScale to targetScale
        PivotPoint.transform.localScale = Vector3.Lerp
        (
            PivotPoint.transform.localScale,
            targetScale, Time.deltaTime * smoothSpeed
        );
    }
}
