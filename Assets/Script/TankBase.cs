using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TankBase : MonoBehaviour
{
	// 操控类型
	public enum Ctltype
	{
		none,
		player,
		computer
	}
	public Ctltype ctlType = Ctltype.player;

	// 生命值/最大生命值
	private float maxHp = 100;
	public float hp;

    // 轮轴
    public List<AxleInfo> axleInfos;

	// 马力/最大马力
	private float motor = 0;
	public float maxMotorTorque;

	// 制动/最大制动
	private float brakeTorque = 0;
    public float maxBrakeTorque;

	// 转向角/最大转向角
	private float steering = 0;
    public float maxSteeringAngle;

	// 炮塔旋转参数
	public Transform turret;
	private Transform body;
	private float turretRotSpeed = 180f;
	private float turretRotTarget = 0f;

	// 炮管方向参数
	private Transform gun;
	private float turretRollTarget = -5f;

	// 轮子和履带
	private Transform wheels;
	private Transform tracks;

	// 马达音源和音效
	private AudioSource motorAudioSource;
	public AudioClip motorClip;

	// 摧毁特效
	public GameObject destroyEffect;

	// 炮弹相关参数
	public GameObject bullet;
	private Transform muzzle;
	public float lastShootTime = 0;
	public float shootInterval = 0.5f;

	// 发射炮弹音源/音效
	private AudioSource shootAudioSource;
	public AudioClip shootClip;

	// 中心准心和坦克准心
	public Texture2D centerSight;
	public Texture2D tankSight;

	// 血条素材
	public Texture2D hpBarBg;
	public Texture2D hpBar;

	// 击杀提示素材
	public Texture2D killUI;
	private float killUIStartTime = float.MinValue;

	// 人工智能
	private AI ai;

	// 初始化
    private void Start()
    {
        turret = transform.Find("Turret");		// 炮塔
        gun = transform.Find("Turret/Gun");		// 炮管
        body = transform.Find("Other");			// 车身
		wheels = transform.Find("Wheels");		// 履带
		tracks = transform.Find("Tracks");      // 轮子

		// 炮弹发射口
		muzzle = transform.Find("Turret/Gun/Muzzle");

		// 马达音效
		motorAudioSource = gameObject.AddComponent<AudioSource>();
		motorAudioSource.spatialBlend = 1;

		// 炮弹音效
		shootAudioSource = gameObject.AddComponent<AudioSource>();
		shootAudioSource.spatialBlend = 1;

		// 重心
		transform.GetComponent<Rigidbody>().centerOfMass = new Vector3(0.3f, 0.8f, 1.5f);

		// 状态初始化
		hp = maxHp;

		// 人工智能
		if (ctlType == Ctltype.computer)
		{
			ai = gameObject.AddComponent<AI>();
			ai.tank = this;
		}
	}

	// 玩家控制
    public void PlayerCtrl()
    {
		if (ctlType != Ctltype.player) return;

		// 马力和转向角
		motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");

		// 制动
		brakeTorque = 0;
		foreach (AxleInfo a in axleInfos)
		{
			if (a.leftWheel.rpm > 5 && motor < 0)
			{
				// 前进时, 按下 Down 键
				brakeTorque = maxBrakeTorque;
			}
			else if (a.leftWheel.rpm < -5 && motor > 0)
			{
				// 后退时,按下 Up 键
				brakeTorque = maxBrakeTorque;
			}
			break;
		}
		
		// 发射炮弹
		if (Input.GetMouseButtonUp(0))
		{
			Shoot();
		}

		// 炮管炮塔角度
		TargetSignPos();
		TurretRotate();
		TurretRoll();
	}

	// 电脑控制
	public void ComputerCtrl()
	{
		if (ctlType != Ctltype.computer) return;

		// 获取炮塔角度
		Quaternion vec = ai.GetTurretTarget(turret);
		if(vec == Quaternion.identity)
		{
			// 恢复原位
			turret.rotation = Quaternion.RotateTowards(turret.rotation, body.rotation, 60f * Time.deltaTime);
		}
		else
		{
			// 指向敌军
			turret.rotation = Quaternion.RotateTowards(turret.rotation, vec, 60f * Time.deltaTime);
		}

		// 炮塔方向修正
		Vector3 euler = new Vector3(0f, turret.localEulerAngles.y, 0f);
		turret.localEulerAngles = euler;

		// 炮管调整
		float angle = gun.localEulerAngles.x;
		if (vec == Quaternion.identity)
		{
			angle = Mathf.Lerp(gun.localEulerAngles.x, 270f, 30f * Time.deltaTime);
		}
		else
		{
			angle = Mathf.Lerp(gun.localEulerAngles.x, vec.eulerAngles.x + 270f, 30f * Time.deltaTime);
		}
		if (angle < 260f) angle = 260f;
		if (angle > 280f) angle = 280f;
		gun.localEulerAngles = new Vector3(angle, 0, 0);

		// 发射炮弹
		if (ai.IsShoot())
		{
			Shoot();
		}

		// 移动
		steering = ai.GetSteering();
		motor = ai.GetMotor();
		brakeTorque = ai.GetBrakeTorque();
	}

	// 无人控制
	public void NoneCtrl()
	{
		if (ctlType != Ctltype.none) return;

		motor = 0;
		steering = 0;
		brakeTorque = maxBrakeTorque / 2;
	}

	// 帧更新
	private void Update()
    {
		// 操控
		PlayerCtrl();
		ComputerCtrl();
		NoneCtrl();

        // 遍历车轴
        foreach (AxleInfo a in axleInfos)
        {
			// 转向
			if (a.steering)
			{
				a.leftWheel.steerAngle = steering;
				a.rightWheel.steerAngle = steering;
			}

			// 马力
			if (a.motor)
            {
                a.leftWheel.motorTorque = motor;
                a.rightWheel.motorTorque = motor;
            }

            // 制动
            if (true)
            {
                a.leftWheel.brakeTorque = brakeTorque;
                a.rightWheel.brakeTorque = brakeTorque;
            }
        }   

		// 旋转履带和轮子
		if(axleInfos[0] != null)
		{
			WheelRotate(axleInfos[0].leftWheel);
			TrackMove();
		}

		// 声效
		MotorSound();

		// 高度判定
		if(transform.position.y - GameObject.Find("Terrain").GetComponent<Terrain>().SampleHeight(transform.position) < -20f)
		{
			SufferAttack(maxHp, null);
		}
    }

	// 炮塔旋转
    public void TurretRotate()
    {
        if(!Camera.main || !turret) return;

		// 四元数插值
		turret.rotation = Quaternion.RotateTowards(turret.rotation, Camera.main.transform.rotation, turretRotSpeed * Time.deltaTime);

		// 炮塔方向修正
		Vector3 euler = new Vector3(0f, turret.localEulerAngles.y, 0f);
		turret.localEulerAngles = euler;
	}

	// 炮管升降
    public void TurretRoll()
    {
        if(!Camera.main || !gun) return;

		// 考虑方向为(260-280)=(265-5,265+10)
		Vector3 euler = new Vector3(gun.localEulerAngles.x, 0f, 0f);

		euler.x = 265f + turretRollTarget;
		gun.localEulerAngles = euler;
	}

	// 履带转动
	public void WheelRotate(WheelCollider collider)
	{
		if (!wheels) return;

		// 获取旋转信息
		Vector3 position;
		Quaternion rotation;
		collider.GetWorldPose(out position, out rotation);

		// 旋转每个轮子
		foreach(Transform wheel in wheels)
		{
			wheel.rotation = rotation;
		}
	}

	// 履带转动特效
	public void TrackMove()
	{
		if (!tracks)
		{
			return;
		}

		float offset = 0;
		if(wheels.GetChild(0) != null)
		{
			// 根据轮子的角度确定偏移量
			offset = wheels.GetChild(0).localEulerAngles.x / 90f;
		}

		foreach(Transform track in tracks)
		{
			// 获取材质
			MeshRenderer mr = track.GetComponent<MeshRenderer>();
			if (mr)
			{
				Material mtl = mr.material;
				mtl.mainTextureOffset = new Vector2(0, offset);
			}
		}
	}

	// 马达声音
	public void MotorSound()
	{
		if(motor != 0 && !motorAudioSource.isPlaying)
		{
			// 声音发动
			motorAudioSource.loop = true;
			motorAudioSource.clip = motorClip;
			motorAudioSource.Play();
		}
		else if(motor == 0)
		{
			// 停止
			motorAudioSource.Pause();
		}
	}

	// 发射炮弹
	public void Shoot()
	{
		// 发射间隔控制
		if (Time.time - lastShootTime < shootInterval) return;

		// 保证炮弹类型已设置
		if (!bullet) return;

		// 发射
		Vector3 pos = gun.position + gun.up * -3;
		GameObject g = GameObject.Instantiate(bullet, pos, gun.rotation);
		g.GetComponent<Bullet>().Launcher = transform;

		// 记录上次发射时间
		lastShootTime = Time.time;

		// 播放音效
		shootAudioSource.PlayOneShot(shootClip);
	}

	// 受到伤害
	public void SufferAttack(float damage, Transform attacker)
	{
		// 物体已经被摧毁
		if (hp <= 0)
		{
			return;
		}

		// 击中处理
		if (hp > 0)
		{
			hp -= damage;

			// 反击
			if (attacker != null && ai != null)
			{
				ai.OnAttacked(attacker);
			}
		}

		// 被摧毁
		if(hp <= 0)
		{
			// 战场结算
			if(attacker == null)
			{
				Battle.instance.IsWin(0);
				Battle.instance.IsWin(1);
			}
			else
			{
				Battle.instance.IsWin(attacker.gameObject);
			}
			

			Vector3[] pos = new Vector3[3];
			pos[0] = new Vector3(0, 1f, 3f);
			pos[1] = new Vector3(-0.8f, 1f, -0.4f);
			pos[2] = new Vector3(1.5f, 1f, 0f);

			for (int i = 0; i < 3; i++)
			{
				GameObject destroyObj = GameObject.Instantiate(destroyEffect);
				destroyObj.transform.SetParent(transform, false);
				destroyObj.transform.localPosition = pos[i];
			}
			
			// 取消操控权
			ctlType = Ctltype.none;
			gameObject.GetComponent<Rigidbody>().mass = 50;

			// 击杀提示
			if (attacker)
			{
				TankBase tankCmp = attacker.GetComponent<TankBase>();
				if(tankCmp && tankCmp.ctlType == Ctltype.player)
				{
					tankCmp.StartDrawKill();
				}
			}
		}
	}

	// 计算目标角度
	public void TargetSignPos()
	{
		// 炮管炮塔角度
		turretRollTarget = Camera.main.transform.eulerAngles.x;
		turretRotTarget = Camera.main.transform.eulerAngles.y;
	}

	// 计算爆炸位置
	public Vector3 CalExplodePoint()
	{
		// 计算碰撞坐标和碰撞信息
		Vector3 hitPoint = Vector3.zero;
		RaycastHit hit;

		// 沿着炮管方向的射线
		Vector3 pos = gun.position + gun.up * -5;
		Ray ray = new Ray(gun.GetChild(0).position, -gun.up);

		// 射线检测
		bool isCollider = Physics.Raycast(ray, out hit, 400f);
		if (isCollider && hit.point.y > 0.1)
		{
			hitPoint = hit.point;
		}
		else
		{
			hitPoint = Vector3.zero;
		}

		return hitPoint;
	}

	// 绘制准心
	public void DrawSight()
	{
		// 计算实际射击位置
		Vector3 explodePoint = CalExplodePoint();

		if(explodePoint != Vector3.zero)
		{
			// 获取"坦克准心"的屏幕坐标
			Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);

			// 绘制坦克准心
			Rect tankRect = new Rect(screenPoint.x - tankSight.width / 2,
									 Screen.height - screenPoint.y - tankSight.height / 2,
									 tankSight.width,
									 tankSight.height);
			GUI.DrawTexture(tankRect, tankSight);
		}

		// 绘制中心准心
		//Rect centerRect = new Rect(Screen.width / 2 - centerSight.width / 2,
		//						   Screen.height / 2 - centerSight.height / 2,
		//						   centerSight.width,
		//						   centerSight.height);
		//GUI.DrawTexture(centerRect, centerSight);
	}

	// 绘图
	private void OnGUI()
	{
		if(ctlType != Ctltype.player)
		{
			return;
		}

		DrawSight();
		DrawHp();
		DrawKillUI();
	}

	// 绘制生命条
	public void DrawHp()
	{
		// 底框
		Rect bgRect = new Rect(30, Screen.height - hpBarBg.height - 15,
							   hpBarBg.width, hpBarBg.height);
		GUI.DrawTexture(bgRect, hpBarBg);

		// 指示条
		float width = hp * 102 / maxHp;
		Rect hpRect = new Rect(bgRect.x + 29, bgRect.y + 9, width, hpBar.height);
		GUI.DrawTexture(hpRect, hpBar);

		// 文字
		string text = Mathf.Ceil(hp).ToString() + "/" + Mathf.Ceil(maxHp).ToString();
		Rect textRect = new Rect(bgRect.x + 80, bgRect.y - 10, 50, 50);
		GUI.Label(textRect, text);
	}

	// 显示击杀图标
	public void StartDrawKill()
	{
		killUIStartTime = Time.time;
	}

	// 绘制击杀图标
	public void DrawKillUI()
	{
		if(Time.time - killUIStartTime < 1f)
		{
			Rect rect = new Rect(Screen.width / 2 - killUI.width / 2, 50,
								 killUI.width, killUI.height);
			GUI.DrawTexture(rect, killUI);
		}
	}
}

