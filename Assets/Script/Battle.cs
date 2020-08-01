using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
	// 单例
	public static Battle instance;

	// 坦克预制件
	public GameObject[] tankPrefabs;

	// 战场中的所有坦克
	public BattleTank[] battleTanks;

	// 初始化
	private void Start()
	{
		// 单例
		instance = this;
	}

	// 获取阵营
	public int GetCamp(GameObject tankObj)
	{
		foreach(BattleTank bt in battleTanks)
		{
			if (bt == null || bt.tank == null) return 0;

			// 确认同一实例
			if(bt.tank.gameObject == tankObj)
			{
				return bt.camp;
			}
		}
		return 0;
	}

	// 是否同一阵营
	public bool IsSameCamp(GameObject tank1, GameObject tank2)
	{
		return tank1 != null && tank2 != null && GetCamp(tank1) == GetCamp(tank2);
	}

	// 胜负判定
	public bool IsWin(int camp)
	{
		foreach(BattleTank bt in battleTanks)
		{
			if(bt.camp != camp && bt.tank.GetComponent<TankBase>().hp > 0)
			{
				return false;
			}
		}
		Debug.Log("Camp" + camp + " win the game.");
		PanelMgr.instance.OpenPanel<WinPanel>("", camp);
		return true;
	}

	// 胜负判断
	public bool IsWin(GameObject attTank)
	{
		int camp = GetCamp(attTank);
		return IsWin(camp);
	}

	// 清理场景
	public void ClearBattle()
	{
		GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");
		foreach(GameObject t in tanks)
		{
			GameObject.Destroy(t);
		}
	}

	// 开始战斗
	public void StartTwoCampBattle(int n1, int n2)
	{
		// 获取出生点容器 
		Transform sp = GameObject.Find("/SwopPoints").transform;
		Transform spCamp1 = sp.GetChild(0);
		Transform spCamp2 = sp.GetChild(1);

		// 判定条件
		if (spCamp1.childCount < n1 || spCamp2.childCount < n2)
		{
			Debug.LogError("Swop points not enough.");
			return;
		}

		if(tankPrefabs.Length < 2)
		{
			Debug.LogError("Tank prefabs not enough.");
			return;
		}

		// 清理场景
		ClearBattle();

		// 产生坦克
		battleTanks = new BattleTank[n1 + n2];
		for(int i = 0; i < n1; i++)
		{
			GenerateTank(1, i, spCamp1, i);
		}
		for(int i = 0; i < n2; i++)
		{
			GenerateTank(2, i, spCamp2, n1+i);
		}

		// 把第一辆坦克设为玩家操控
		TankBase tankCmp = battleTanks[0].tank;
		tankCmp.ctlType = TankBase.Ctltype.player;

		// 设置相机
		CameraFollow cf = Camera.main.gameObject.GetComponent<CameraFollow>();
		Transform target = tankCmp.transform;
		cf.SetTarget(target);
	}

	// 生成坦克
	public void GenerateTank(int camp, int num, Transform spCamp, int index)
	{
		// 获取出生点相关信息
		Transform trans = spCamp.GetChild(num);
		Vector3 pos = trans.position;
		Quaternion rot = trans.rotation;
		GameObject prefab = tankPrefabs[camp - 1];

		// 产生坦克
		GameObject tankObj = GameObject.Instantiate(prefab, pos, rot);

		// 设置属性
		TankBase tankCmp = tankObj.GetComponent<TankBase>();
		tankCmp.ctlType = TankBase.Ctltype.computer;

		// 战场设置
		battleTanks[index] = new BattleTank();
		battleTanks[index].tank = tankCmp;
		battleTanks[index].camp = camp;
	}
}
