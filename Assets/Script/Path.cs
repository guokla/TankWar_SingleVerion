using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Path
{
	// 所有路径点
	public Vector3[] wayPoints;

	// 当前路径点索引
	public int index = -1;
	public readonly Vector3[] dir = new Vector3[9] {
		new Vector3(  0,  0,  0),	// origin
		new Vector3(  1,  0,  0),	// right
		new Vector3(  0,  0,  1),	// forward
		new Vector3( -1,  0,  0),	// left
		new Vector3(  0,  0, -1),	// backward
		new Vector3(  1,  0,  1),
		new Vector3(  1,  0, -1),
		new Vector3( -1,  0,  1),
		new Vector3( -1,  0, -1),
	};

	// 当前路径点
	public Vector3 wayPoint;

	// 是否循环
	private bool isLoop = false;

	// 到达误差
	public float deviation = 5f;

	// 是否完成
	public bool isFinish = true;

	// 是否到达目的地
	public bool Reach(Transform t)
	{
		Vector3 pos = t.position;
		float distance = Vector3.Distance(pos, wayPoint);
		return distance < deviation;
	}

	// 下一个路径点
	public void NextWayPoint()
	{
		// 未定义路径
		if(index < 0)
		{
			return;
		}

		if(index < wayPoints.Length - 1)
		{
			index++;
		}
		else
		{
			if (isLoop)
			{
				index = 0;
			}
			else
			{
				isFinish = true;
			}
		}
		wayPoint = wayPoints[index];
	}

	// 根据场景标志物生成路径点
	public void InitByObj(GameObject obj, bool flag = false)
	{
		int length = obj.transform.childCount;

		// 没有子物体
		if(length == 0)
		{
			wayPoints = null;
			index = -1;
			Debug.LogWarning("Path.InitByObj Length == 0");
		}

		// 遍历子物体生成路径
		wayPoints = new Vector3[length];
		for(int i = 0; i < length; i++)
		{
			Transform t = obj.transform.GetChild(i);
			wayPoints[i] = t.position;
		}

		// 设置一些参数
		index = 0;
		wayPoint = wayPoints[0];
		isLoop = flag;
		isFinish = false;
	}

	// Nav导航生成路径点
	public void InitByNavMeshPath(Vector3 pos, Vector3 targetPos)
	{
		// 重置
		wayPoints = null;
		index = -1;

		// 判定路径存在
		NavMeshPath navPath = new NavMeshPath();
		bool hasFoundPath = false;
		for (int i = 0; i < 9; i++)
		{
			Vector3 fixPos = targetPos + dir[i] * 5f;

			// 计算路径
			hasFoundPath = NavMesh.CalculatePath(pos, targetPos, NavMesh.AllAreas, navPath);
			Debug.DrawLine(pos + 2 * Vector3.up, fixPos + 2 * Vector3.up, Color.red, 2f);
			if (hasFoundPath) break;
		}

		if (!hasFoundPath)
		{
			Debug.Log("Path Initial failed. Please check the reachability");
			return;
		}

		// 生成路径
		int length = navPath.corners.Length;
		wayPoints = new Vector3[length];
		for(int i = 0; i < length; i++)
		{
			wayPoints[i] = navPath.corners[i];
		}

		index = 0;
		wayPoint = wayPoints[0];
		isFinish = false;
	}

	// 调试路径
	public void DrawWayPoints()
	{
		if (wayPoints == null) return;

		for(int i = 0; i < wayPoints.Length; i++)
		{
			if(i == index)
			{
				Gizmos.DrawSphere(wayPoints[i], 2);
			}
			else
			{
				Gizmos.DrawCube(wayPoints[i], Vector3.one);
			}
		}
	}
}
