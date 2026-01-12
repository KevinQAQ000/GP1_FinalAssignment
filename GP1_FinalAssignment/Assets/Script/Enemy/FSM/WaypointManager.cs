using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Assigns different patrol points to each enemy.
/// </summary>
public class WaypointManager : MonoBehaviour
{
    private static WaypointManager _instance;
    public static WaypointManager Instance
    {
        get // Singleton pattern
        {
            return _instance;
        }
    }

    // Use two Lists to track used waypoint indices and all available waypoint indices.
    // This facilitates assigning unique patrol point IDs to enemies.
    public List<int> usingIndex = new List<int>(); // Tracks indices of waypoints currently in use
    public List<int> rawIndex = new List<int>();   // Tracks all available waypoint indices

    private void Awake()
    {
        _instance = this; // Initialize Singleton

        // Assign route IDs
        int tempCount = rawIndex.Count;
        for (int i = 0; i < tempCount; i++)
        {
            // Select a random index from the available list
            int tempIndex = Random.Range(0, rawIndex.Count);

            // Add the corresponding waypoint ID to the used list
            usingIndex.Add(rawIndex[tempIndex]);

            // Remove the index from the available list to prevent duplicate assignments
            rawIndex.RemoveAt(tempIndex);
        }
    }
}