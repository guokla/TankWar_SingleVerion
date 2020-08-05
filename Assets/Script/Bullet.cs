using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	// 速度
	public float velocity = 100f;       

	// 爆炸特效/音效
	public GameObject explode;
	public AudioClip explodeClip;

	// 最大生存时间/初始时间
	public float maxLifeTime = 2f;      
	private float instaniateTime = 0;

	// 发射者
	public Transform Launcher;

	// 初始化
	private void Start()
	{
		instaniateTime = Time.time;
	}

	// 帧更新
	private void Update()
	{
		// 前进
		transform.position = transform.position + transform.up * -velocity * Time.deltaTime;

		// 生存时间判定
		if (Time.time - instaniateTime >= maxLifeTime)
		{
			GameObject.Destroy(gameObject);
		}

		// 地形碰撞(--待解决 Bug--)
		float diff = transform.position.y - transform.Find("/Terrain").GetComponent<Terrain>().SampleHeight(transform.position);
		if (diff <= 0)
		{
			// 爆炸效果
			GameObject explodeEffect = GameObject.Instantiate(explode, transform.position - diff * transform.forward, Quaternion.identity);
			GameObject.Destroy(explodeEffect, 2f);
			AudioSource audioSource = explodeEffect.AddComponent<AudioSource>();
			audioSource.spatialBlend = 1;
			audioSource.PlayOneShot(explodeClip);

			// 销毁自身
			GameObject.Destroy(gameObject);
		}
	}

	// 碰撞处理
	public void OnCollisionEnter(Collision collision)
	{
		Debug.Log("Bullet Collider: " + collision.transform.name);

		// 防误判
		if (!collision.gameObject) return;
		TankBase tankCmp = collision.transform.GetComponent<TankBase>();
		if (collision.transform == Launcher || !tankCmp || tankCmp.ctlType == TankBase.Ctltype.none) return;

		// 爆炸效果
		GameObject explodeEffect = GameObject.Instantiate(explode, transform.position, Quaternion.identity);
		GameObject.Destroy(explodeEffect, 2f);
		AudioSource audioSource = explodeEffect.AddComponent<AudioSource>();
		audioSource.spatialBlend = 1;
		audioSource.PlayOneShot(explodeClip);

		// 销毁自身
		GameObject.Destroy(gameObject);

		// 击中坦克
		if(collision.transform.tag == "Tank")
		{
			TankBase tank = collision.transform.GetComponent<TankBase>();
			if (tank)
			{
				tank.SufferAttack(GetDamage(), Launcher);
			}
		}
	}

	// 获取伤害
	public float GetDamage()
	{
		// 伤害与飞行时间相关
		float damage = 10f - (Time.time - instaniateTime) * 4f;

		// 限制伤害范围
		return Mathf.Max(damage, 1f);
	}
}
