using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 给每一个敌人分配不同巡逻点
/// </summary>
public class WaypointManager : MonoBehaviour
{
    private static WaypointManager _instance;
    public static WaypointManager Instance
    {
        get//单例模式
        {
            return _instance;
        }
    }

    //用两个List分别记录已经被使用的巡逻点下标和所有巡逻点下标
    //方便给敌人分配不同的巡逻点ID
    public List<int> usingIndex = new List<int>();//记录已经被使用的巡逻点下标
    public List<int> rawIndex = new List<int>();//记录所有巡逻点下标

    private void Awake()
    {
        _instance = this;//单例初始化
        //分配路线ID
        int tempCount = rawIndex.Count;
        for (int i = 0; i < tempCount; i++)
        {
            int tempIndex = Random.Range(0, rawIndex.Count);//随机获取一个下标
            //usingIndex.Add(tempIndex);
            usingIndex.Add(rawIndex[tempIndex]);//将该下标对应的巡逻点ID添加到已使用列表中
            rawIndex.RemoveAt(tempIndex);//将该下标从未使用列表中移除，避免重复分配
        }
    }

}
