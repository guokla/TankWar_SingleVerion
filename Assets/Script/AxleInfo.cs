using UnityEngine;

[System.Serializable]
public class AxleInfo
{
    // 左右轮碰撞器
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;

    // 发动机是否将动力送给轴上的轮子
    public bool motor;

    // 轮子是否转向
    public bool steering;
}
