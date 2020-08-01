using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMgr : MonoBehaviour
{
	// 单例
	public static PanelMgr instance;

	// 画板/面板
	private GameObject canvas;
	public Dictionary<string, PanelBase> dict;

	// 层级
	private Dictionary<PanelLayer, Transform> layerDict;

	// 唤醒
	public void Awake()
	{
		instance = this;
		InitLayer();
		dict = new Dictionary<string, PanelBase>();
	}

	// 初始化层
	private void InitLayer()
	{
		// 画布
		canvas = GameObject.Find("Canvas");
		if (canvas == null)
		{
			Debug.LogError("panelMgr.InitLayer failed, canvas is null.");
		}

		// 各个层级
		layerDict = new Dictionary<PanelLayer, Transform>();
		foreach (PanelLayer pl in Enum.GetValues(typeof(PanelLayer)))
		{
			string name = pl.ToString();
			Transform trans = canvas.transform;
			layerDict.Add(pl, trans);
		}
	}

	// 打开面板
	public void OpenPanel<T>(string skinPath, params object[] args) where T : PanelBase
	{
		// 已经打开
		string name = typeof(T).ToString();
		if (dict.ContainsKey(name))
		{
			return;
		}

		// 面板脚本
		PanelBase panel = canvas.AddComponent<T>();
		panel.Init(args);
		dict.Add(name, panel);

		// 加载皮肤
		skinPath = (skinPath != "" ? skinPath : panel.skinPath);
		GameObject skin = Resources.Load<GameObject>(skinPath);
		if (skin == null)
		{
			Debug.LogError("panelMgr.OpenPanel failed, skin is null");
		}
		panel.skin = GameObject.Instantiate(skin);

		// 坐标
		Transform skinTrans = panel.skin.transform;
		PanelLayer layer = panel.layer;
		Transform parent = layerDict[layer];
		skinTrans.SetParent(parent, false);

		// 生命周期
		panel.OnShowing();

		panel.OnShowed();
	}

	// 关闭面板
	public void ClosePanel(string name)
	{
		PanelBase panel = (PanelBase)dict[name]; 

		if(panel == null)
		{
			Debug.Log("PanelMgr.ClosePanel fail, panel is null.");
			return;
		}

		panel.OnClosing();
		dict.Remove(name);

		panel.OnClosed();
		GameObject.Destroy(panel.skin);
		Component.Destroy(panel);
	}


}

// 分层类型
public enum PanelLayer
{
	Panel,
	Tips,
};
