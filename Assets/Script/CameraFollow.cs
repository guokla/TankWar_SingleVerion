using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    // 距离
    public float distance = 8;

    // 横向角度
    public float rot = 0;

    // 纵向角度
    private float roll = 10f * Mathf.PI * 2 / 360;

    // 目标物体
    public Transform target;

    // 横向旋转速度
    public float rotSpeed = 0.2f;

    // 纵向角度范围
    private float maxRoll = 10f * Mathf.PI * 2 / 360;
    private float minRoll = -5f * Mathf.PI * 2 / 360;

    // 纵向旋转速度
    private float rollSpeed = 0.2f;

    // 距离范围
    public float maxDistance = 22f;
    public float minDistance = 5f;

    // 距离变化速度
    public float zoomSpeed = 0.2f;

	// 被选中判断
	public bool isRotating = false;

    void LateUpdate() 
    {
        if (!target || !Camera.main) return;
 
		// 计算镜头坐标和角度
        Vector3 targetPos = target.transform.position;
        Vector3 cameraPos;
        float d = distance *Mathf.Cos (roll);
        float height = distance * Mathf.Sin(roll);
        cameraPos.x = targetPos.x +d * Mathf.Cos(rot);
        cameraPos.z = targetPos.z + d * Mathf.Sin(rot);
        cameraPos.y = targetPos.y + height;

        Camera.main.transform.position = cameraPos;
        Camera.main.transform.LookAt(target);

		// 判断旋转状态
		OnRotate();
		if (isRotating)
		{
			Rotate();
			Roll();
		}
        Zoom();
    }

    //设置目标
    public void SetTarget(Transform t)
    {
        if (t.transform.Find("Turret/CameraPos"))
		{
			target = t.transform.Find("Turret/CameraPos");
		}
		else
		{
			target = t;
		}
    }

    //横向旋转
    void Rotate()
    {
        float w = Input.GetAxis("Mouse X") * rotSpeed;
        rot -= w;
    }

    //纵向旋转
    void Roll()
    {
        float w = Input.GetAxis("Mouse Y") * rollSpeed * 0.5f;

        roll -= w;
        if (roll > maxRoll) roll = maxRoll;
        if (roll < minRoll) roll = minRoll;
    }

    //调整距离
    void Zoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (distance > minDistance)
                distance -= zoomSpeed;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (distance < maxDistance)
                distance += zoomSpeed;
        }
    }

	// 判断是否处于旋转状态
	public void OnRotate()
	{
		if (Input.GetMouseButtonDown(1))
		{
			isRotating = true;
		}
		else if (Input.GetMouseButtonUp(1))
		{
			isRotating = false;
		}
	}
}