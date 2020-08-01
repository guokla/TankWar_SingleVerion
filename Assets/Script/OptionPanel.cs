using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionPanel : PanelBase
{
	private Button startBtn;
	private Button closeBtn;
	private Dropdown dropdown1;
	private Dropdown dropdown2;

	#region 生命周期
	public override void Init(params object[] args)
	{
		base.Init(args);
		skinPath = "OptionPanel";
		layer = PanelLayer.Panel;
	}

	public override void OnShowing()
	{
		base.OnShowing();
		Transform skinTrans = skin.transform;

		startBtn = skinTrans.Find("StartBtn").GetComponent<Button>();
		startBtn.onClick.AddListener(OnStartClick);

		closeBtn = skinTrans.Find("CloseBtn").GetComponent<Button>();
		closeBtn.onClick.AddListener(OnCloseClick);

		dropdown1 = skinTrans.Find("Dropdown1").GetComponent<Dropdown>();
		dropdown2 = skinTrans.Find("Dropdown2").GetComponent<Dropdown>();
		
	}
	#endregion

	public void OnStartClick()
	{
		int n1 = dropdown1.value + 1;
		int n2 = dropdown2.value + 1;
		Battle.instance.StartTwoCampBattle(n1, n2);
		Close();
	}

	public void OnCloseClick()
	{
		Close();
	}
}
