using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

	public Launcher		launcher;		// ランチャーのプレハブ.

	public	Vector3		velocity;		// 初速度（Launcher から受け取る用）.
	public	bool		is_touched;		// プレイヤーが触った？.
	private	float		time;			// 実行中タイマー.
	private bool		is_launched;	// 発射された？（false ならフェードイン中）.

	// ---------------------------------------------------------------- //

	void Start ()
	{
		// ランチャーのゲームオブジェクトを探しておく.
		this.launcher = GameObject.FindGameObjectWithTag("Launcher").GetComponent<Launcher>();

		// アルファーで見えなくしておく.

		Color	color = this.GetComponent<Renderer>().material.color;

		color.a = 0.0f;

		this.GetComponent<Renderer>().material.color = color;

		//

		this.GetComponent<Rigidbody>().useGravity = false;

		this.is_touched = false;
		this.is_launched = false;

		this.time = 0.0f;
	}

	void Update ()
	{
		// 発射される前だったら（フェードイン中だったら）…….
		if(!this.is_launched) {

			float	delay = 0.5f;

			// アルファーでフェードインする.

			Color color = this.GetComponent<Renderer>().material.color;

			// [0.0f ～ delay] の範囲を [0.0f ～ 1.0f] に変換する
			float	t = Mathf.InverseLerp(0.0f, delay, this.time);

			t = Mathf.Min(t, 1.0f);

			color.a = Mathf.Lerp(0.0f, 1.0f, t);

			this.GetComponent<Renderer>().material.color = color;

			// 一定時間が経過したら、発射.
			if(this.time >= delay) {

				this.GetComponent<Rigidbody>().useGravity = true;
				this.GetComponent<Rigidbody>().velocity = this.velocity;

				this.is_launched = true;
			}
		}

		this.time += Time.deltaTime;

		DebugPrint.print(this.GetComponent<Rigidbody>().velocity.ToString());
	}

	// 画面外に出たとき.
	void	OnBecameInvisible()
	{
		// ボールが削除されることをランチャーに通知.
		this.launcher.OnBallDestroy();

		// プレイヤーが触れていなかった（空振り）なら、ミス.
		if(!this.is_touched) {

			if(this.launcher != null) {

				this.launcher.setResult(false);
			}
		}

		// ゲームオブジェクトを削除.
		Destroy(this.gameObject);
	}

	// 他のオブジェクトと衝突したとき.
	void OnCollisionEnter(Collision collision)
	{
		// 衝突した相手がプレイヤーだったら…….
		if(collision.gameObject.tag == "Player") {

			if(collision.gameObject.GetComponent<Player>().isLanded()) {

				// プレイヤーが着地中だったらミス.

				this.launcher.setResult(false);

				// プレイヤーが触ったことを覚えておく.
				this.is_touched = true;

			} else {

				// プレイヤーがジャンプ中だったら成功.

				this.launcher.setResult(true);
				this.is_touched = true;
			}
		}
	}
}
