using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PanelBase : MonoBehaviour
{
	// 皮肤/皮肤路径
	public GameObject skin;
	public string skinPath;

	// 层级/面板参数
	public PanelLayer layer;
	public object[] args;

	#region 生命周期

	// 初始化
	public virtual void Init(params object[] args)
	{
		this.args = args;
	}

	// 开始面板前
	public virtual void OnShowing() { }
	// 显示面板后
	public virtual void OnShowed() { }
	// 帧更新
	public virtual void Update() { }
	// 关闭前
	public virtual void OnClosing() { }
	// 关闭后
	public virtual void OnClosed() { }

	#endregion

	#region 操作

	protected virtual void Close()
	{
		string name = this.GetType().ToString();
		PanelMgr.instance.ClosePanel(name);
	}

	#endregion
}
