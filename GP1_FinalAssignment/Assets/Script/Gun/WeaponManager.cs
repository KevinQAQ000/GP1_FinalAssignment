using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    public GameObject[] weapons;
    private bool[] hasWeapon;
    private int currentWeaponIndex = -1;

    [Header("Animation Settings")]
    public float switchSpeed = 8f; // Speed of the switching animation
    public float dropDistance = 0.5f; // Distance the weapon moves down during switch
    private bool isSwitching = false; // Flag to check if a switch is currently in progress

    private Vector3 originalLocalPos; // Stores the initial local position of the weapons
    public PlayerController PlayerController;

    void Start()
    {
        hasWeapon = new bool[weapons.Length];

        // Assuming all weapons share the same relative starting position
        if (weapons.Length > 0) originalLocalPos = weapons[0].transform.localPosition;

        PlayerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (PlayerController.playerisDead)
        {
            return;
        }

        // Ignore input if currently switching weapons
        if (isSwitching) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Only trigger switch if the player owns at least two weapons
            int ownedCount = 0;
            foreach (bool b in hasWeapon) if (b) ownedCount++;

            if (ownedCount >= 2)
            {
                StartCoroutine(SwitchWeaponRoutine());
            }
        }
    }

    // Coroutine for the standard weapon switching sequence
    IEnumerator SwitchWeaponRoutine()
    {
        isSwitching = true;

        // Move the current weapon down
        yield return StartCoroutine(MoveWeapon(weapons[currentWeaponIndex], originalLocalPos - new Vector3(0, dropDistance, 0)));
        weapons[currentWeaponIndex].SetActive(false);

        // Toggle index (cycles between 0 and 1 for a 2-weapon setup)
        currentWeaponIndex = (currentWeaponIndex == 0) ? 1 : 0;

        // Prepare the new weapon (set position to bottom before enabling)
        weapons[currentWeaponIndex].transform.localPosition = originalLocalPos - new Vector3(0, dropDistance, 0);
        weapons[currentWeaponIndex].SetActive(true);

        // Move the new weapon up to the original position
        yield return StartCoroutine(MoveWeapon(weapons[currentWeaponIndex], originalLocalPos));

        isSwitching = false;
    }

    // Helper coroutine to smoothly interpolate weapon movement
    IEnumerator MoveWeapon(GameObject weapon, Vector3 targetPos)
    {
        while (Vector3.Distance(weapon.transform.localPosition, targetPos) > 0.01f)
        {
            weapon.transform.localPosition = Vector3.Lerp(weapon.transform.localPosition, targetPos, Time.deltaTime * switchSpeed);
            yield return null;
        }
        weapon.transform.localPosition = targetPos;
    }

    // Method to unlock/pick up a weapon
    public void UnlockWeapon(int index)
    {
        // Return if the weapon is already owned to prevent redundant triggers
        if (hasWeapon[index]) return;

        hasWeapon[index] = true;

        // Logic for picking up the very first weapon
        if (currentWeaponIndex == -1)
        {
            currentWeaponIndex = index;
            weapons[currentWeaponIndex].SetActive(true);
            weapons[currentWeaponIndex].transform.localPosition = originalLocalPos;
        }
        // If already holding a weapon, automatically switch to the newly picked one
        else
        {
            // Stop any ongoing switch routines to prevent conflicts
            StopAllCoroutines();

            // Start the specific routine for switching to a newly acquired weapon
            StartCoroutine(SwitchToNewWeapon(index));
        }
    }

    // Coroutine designed specifically for the "Pick Up" switch logic
    IEnumerator SwitchToNewWeapon(int newIndex)
    {
        isSwitching = true;

        // Move the old weapon down
        yield return StartCoroutine(MoveWeapon(weapons[currentWeaponIndex], originalLocalPos - new Vector3(0, dropDistance, 0)));
        weapons[currentWeaponIndex].SetActive(false);

        // Update the index to the new weapon
        currentWeaponIndex = newIndex;

        // Prepare and show the new weapon
        weapons[currentWeaponIndex].transform.localPosition = originalLocalPos - new Vector3(0, dropDistance, 0);
        weapons[currentWeaponIndex].SetActive(true);

        // Move the new weapon up
        yield return StartCoroutine(MoveWeapon(weapons[currentWeaponIndex], originalLocalPos));

        isSwitching = false;
    }
}