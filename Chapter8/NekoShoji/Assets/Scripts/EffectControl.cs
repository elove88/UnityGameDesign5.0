using UnityEngine;
using System.Collections;

public class EffectControl : MonoBehaviour {

	public GameObject	eff_break = null;	// 紙をやぶったときのエフェクト.
	public GameObject	eff_miss  = null;	// 鉄板に当たったときのエフェクト.

	public GameObject	game_camera = null;

	// ---------------------------------------------------------------- //

	void 	Start()
	{
		this.game_camera = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	void	Update()
	{
	
	}

	// 紙をやぶったときのエフェクト.
	public void	createBreakEffect(SyoujiPaperControl paper, NekoControl neko)
	{
		GameObject 	go = Instantiate(this.eff_break) as GameObject;

		go.AddComponent<Effect>();

		Vector3	position = paper.transform.position;

		position.x = neko.transform.position.x;
		position.y = neko.transform.position.y;

		position.y += 0.3f;

		go.transform.position = position;

		// 画面に長く映るよう、カメラの子供にする（カメラと一緒に移動するように）.
		go.transform.parent = this.game_camera.transform;
	}

	public void	createMissEffect(NekoControl neko)
	{
		GameObject 	go = Instantiate(this.eff_miss) as GameObject;

		go.AddComponent<Effect>();

		Vector3	position = neko.transform.position;

		position.y += 0.3f;

		// 鉄板にうまってしまわないように.
		position.z -= 0.2f;

		go.transform.position = position;
	}
}
