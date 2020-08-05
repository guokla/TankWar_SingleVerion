using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
	// 控制/锁定的坦克
	public TankBase tank;
	public Transform target;

	// 状态枚举
	public enum Status
	{
		Patrol,
		Attack
	}
	public Status status = Status.Patrol;

	// 视野范围/上一次搜索时间/搜索间隔
	private float sightDistance = 300f;	
	public float lastSearchTime = 0f;
	private float searchInterval = 3f;

	// 索敌列表
	public List<GameObject> targetList = new List<GameObject>();

	// 路径/上次路径更新时间/路径更新间隔
	public Path path = new Path();
	private float lastUpdateWaypointTime = 0f;
	private float updateWaypointInterval = 10f;

	// 初始化
	private void Start()
	{
		InitPath();
	}

	// 帧更新
	private void Update()
	{
		// 电脑控制
		if (tank.ctlType != TankBase.Ctltype.computer) return;

		// 状态同步
		if(status == Status.Patrol)
		{
			PatrolUpdate();
		}
		else if (status == Status.Attack)
		{
			AttackUpdate();
		}

		// 更新路径
		if (path.Reach(transform))
		{
			path.NextWayPoint();
		}

		// 搜索同步
		TargetUpdate();
	}
	
	// 更改状态
	public void ChangeStatus(Status condition)
	{
		if(condition == Status.Patrol)
		{
			PatrolStart();
		}
		else if(condition == Status.Attack)
		{
			AttackStart();
		}
	}
	
	// 巡逻开始
	private void PatrolStart()
	{
		status = Status.Patrol;
	}

	// 攻击开始
	private void AttackStart()
	{
		status = Status.Attack;
		Vector3 targetPos = target.position;
		path.InitByNavMeshPath(transform.position, targetPos);
	}

	// 巡逻更新
	private void PatrolUpdate()
	{
		// 发现敌人
		if (target != null)
		{
			ChangeStatus(Status.Attack);
		}

		// 计算时间间隔
		float interval = Time.time - lastUpdateWaypointTime;
		if (interval < updateWaypointInterval) return;
		lastUpdateWaypointTime = Time.time;

		// 处理巡逻点
		if(path.wayPoint == null || path.isFinish)
		{
			GameObject obj = GameObject.Find("WayPointContainer");
			int count = obj.transform.childCount;
			if (count == 0) return;
			int index = Random.Range(0, count);
			Vector3 targetPos = obj.transform.GetChild(index).position;

			Debug.DrawLine(transform.position + 2 * Vector3.up, targetPos + 2 * Vector3.up, Color.green, 2f);
			path.InitByNavMeshPath(transform.position, targetPos);
		}

	}

	// 进攻更新
	private void AttackUpdate()
	{
		// 目标丢失
		if(target == null)
		{
			ChangeStatus(Status.Patrol);
		}

		// 时间间隔
		float interval = Time.time - lastUpdateWaypointTime;
		if (interval < updateWaypointInterval) return;
		lastUpdateWaypointTime = Time.time;

		// 更新路径
		path.InitByNavMeshPath(tank.transform.position, target.position);
	}

	// 搜索目标
	private void TargetUpdate()
	{
		// 计算间隔
		float interval = Time.time - lastSearchTime;
		if (interval < searchInterval) return;
		lastSearchTime = Time.time;

		// 已有目标时, 判断目标是否丢失
		if (target)
		{
			HasTarget();
		}
		else
		{
			NoTarget();
		}
	}

	// 判断目标是否丢失
	private void HasTarget()
	{
		TankBase targetTank = target.GetComponent<TankBase>();
		Vector3 pos = transform.position;
		Vector3 targetPos = target.position;

		if(targetTank == null|| targetTank.ctlType == TankBase.Ctltype.none)
		{
			Debug.Log("目标死亡, 丢失目标");
			target = null;
			path.NextWayPoint();
		}
		else if (Vector3.Distance(pos, targetPos) > sightDistance)
		{
			Debug.Log("距离过远, 丢失目标");
			target = null;
			path.NextWayPoint();
		}
	}

	// 无目标, 搜索视野中的坦克
	private void NoTarget()
	{
		// 遍历所有坦克, 
		targetList.Clear();
		targetList.AddRange(GameObject.FindGameObjectsWithTag("Tank"));

		// 无合适目标
		if (targetList.Count <= 1) return;

		// 根据生命值排序
		targetList.Sort((x, y) => {
			return (x.GetComponent<TankBase>().hp.CompareTo(y.GetComponent<TankBase>().hp));
		});

		foreach(GameObject t in targetList)
		{
			// 获取组件 
			TankBase tankCmp = t.GetComponent<TankBase>();
			
			// 无操控状态
			if (!tankCmp || tankCmp.ctlType == TankBase.Ctltype.none) continue;

			// 自身
			if (t == gameObject) continue;

			// 队友
			if (Battle.instance.IsSameCamp(gameObject, t)) continue;
	
			// 判断距离
			Vector3 pos = transform.position;
			Vector3 targetPos = t.transform.position;
			if (Vector3.Distance(pos, targetPos) > sightDistance) continue;

			target = t.transform;
			break;
		}
	}

	// 被攻击
	public void OnAttacked(Transform attackTank)
	{
		if (attackTank == null || gameObject == null) return;

		// 队友
		if (Battle.instance.IsSameCamp(gameObject, attackTank.gameObject)) return;

		// 重置索敌效果
		lastSearchTime = Time.time;

		target = attackTank;
	}

	// 获取炮塔的目标角度
	public Quaternion GetTurretTarget(Transform turret)
	{
		if (turret && target)
		{
			Vector3 pos = turret.position;
			Vector3 targetPos = target.position;
			Vector3 vec = targetPos - pos;
			return Quaternion.LookRotation(vec);
		}
		else
		{
			return Quaternion.identity;
		}
	}

	// 获取转向角
	public float GetSteering()
	{
		if (!tank || path.isFinish) return 0;

		// 获得路径点相对于坦克的坐标
		Vector3 itp = transform.InverseTransformPoint(path.wayPoint);
		float ratio = 1f;

		if (itp.x > 1f)
		{
			// 左转
			return tank.maxSteeringAngle * ratio;
		}
		else if (itp.x < -1f) 
		{
			// 右转
			return -tank.maxSteeringAngle * ratio;
		}
		else return 0;
	}

	// 获取马力
	public float GetMotor()
	{
		if (tank == null || path.isFinish) return 0;

		// 获取路径点相对坦克的坐标
		Vector3 itp = transform.InverseTransformPoint(path.wayPoint);
		float ratio = 1f;

		// 设置后退区域
		Vector3 vec = new Vector3(itp.x, 6, itp.z);

		// 判定后退条件
		if(vec.z < 0 && Mathf.Abs(vec.x) < -vec.z && Mathf.Abs(vec.x) < vec.y)
		{
			return -tank.maxMotorTorque * ratio;
		}
		else
		{
			return tank.maxMotorTorque * ratio;
		}
	}

	// 获取刹车
	public float GetBrakeTorque()
	{
		if (path.isFinish)
		{
			return tank.maxBrakeTorque;
		}
		else
		{
			return 0;
		}
	}

	// 判定是否可以开炮
	public bool IsShoot()
	{
		if (!target) return false;

		// 计算角度差
		Vector3 vec = tank.turret.eulerAngles - GetTurretTarget(transform).eulerAngles;
		float angle = vec.y;
		if (angle < 0) angle += 360f;

		// 30度内发射炮弹
		if(angle < 30f || angle > 330f)
			return true;
		return false;
	}

	// 初始化路径
	private void InitPath()
	{

	}

	// 绘制路径
	private void OnDrawGizmos()
	{
		
	}
}
