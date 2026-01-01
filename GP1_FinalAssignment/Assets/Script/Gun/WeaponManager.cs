using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    public GameObject[] weapons;
    private bool[] hasWeapon;
    private int currentWeaponIndex = -1;

    [Header("Animation Settings")]
    public float switchSpeed = 8f;//切换速度
    public float dropDistance = 0.5f;//向下移动的距离
    private bool isSwitching = false;//是否正在切换中

    private Vector3 originalLocalPos;//记录武器的初始局部坐标

    void Start()
    {
        hasWeapon = new bool[weapons.Length];
        //假设两把枪的初始相对位置是一样的
        if (weapons.Length > 0) originalLocalPos = weapons[0].transform.localPosition;
    }

    void Update()
    {
        if (isSwitching) return; //如果正在切枪，不接受新输入

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            //只有拥有两把枪时才启动协程
            int ownedCount = 0;
            foreach (bool b in hasWeapon) if (b) ownedCount++;

            if (ownedCount >= 2)
            {
                StartCoroutine(SwitchWeaponRoutine());
            }
        }
    }

    IEnumerator SwitchWeaponRoutine()
    {
        isSwitching = true;

        //下拉当前武器
        yield return StartCoroutine(MoveWeapon(weapons[currentWeaponIndex], originalLocalPos - new Vector3(0, dropDistance, 0)));
        weapons[currentWeaponIndex].SetActive(false);

        //切换索引
        currentWeaponIndex = (currentWeaponIndex == 0) ? 1 : 0;

        //准备新武器（先设为下方位置，再显示）
        weapons[currentWeaponIndex].transform.localPosition = originalLocalPos - new Vector3(0, dropDistance, 0);
        weapons[currentWeaponIndex].SetActive(true);

        //上抬新武器
        yield return StartCoroutine(MoveWeapon(weapons[currentWeaponIndex], originalLocalPos));

        isSwitching = false;
    }

    //移动武器的辅助协程
    IEnumerator MoveWeapon(GameObject weapon, Vector3 targetPos)
    {
        while (Vector3.Distance(weapon.transform.localPosition, targetPos) > 0.01f)
        {
            weapon.transform.localPosition = Vector3.Lerp(weapon.transform.localPosition, targetPos, Time.deltaTime * switchSpeed);
            yield return null;
        }
        weapon.transform.localPosition = targetPos;
    }

    public void UnlockWeapon(int index)
    {
        //如果已经拥有该武器，直接返回（防止重复触发）
        if (hasWeapon[index]) return;

        hasWeapon[index] = true;

        //这是捡到的第一把武器
        if (currentWeaponIndex == -1)
        {
            currentWeaponIndex = index;
            weapons[currentWeaponIndex].SetActive(true);
            weapons[currentWeaponIndex].transform.localPosition = originalLocalPos;
        }
        //手里已经有武器了，自动切到新捡到的这一把
        else
        {
            //停止当前可能正在进行的切枪协程，防止冲突
            StopAllCoroutines();

            //启动切换到新武器的协程
            StartCoroutine(SwitchToNewWeapon(index));
        }
    }

    //专门为“拾取”设计的切换逻辑
    IEnumerator SwitchToNewWeapon(int newIndex)
    {
        isSwitching = true;

        //下拉当前老武器
        yield return StartCoroutine(MoveWeapon(weapons[currentWeaponIndex], originalLocalPos - new Vector3(0, dropDistance, 0)));
        weapons[currentWeaponIndex].SetActive(false);

        //更新索引为新捡到的武器
        currentWeaponIndex = newIndex;

        //准备新武器
        weapons[currentWeaponIndex].transform.localPosition = originalLocalPos - new Vector3(0, dropDistance, 0);
        weapons[currentWeaponIndex].SetActive(true);

        //上抬新武器
        yield return StartCoroutine(MoveWeapon(weapons[currentWeaponIndex], originalLocalPos));

        isSwitching = false;
    }
}