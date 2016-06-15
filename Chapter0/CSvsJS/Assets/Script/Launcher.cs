using UnityEngine;
using System.Collections;

public class Launcher : MonoBehaviour {

	public GameObject	ball_prefab;				// ボールのプレハブ.
	private Player		player;						// プレイヤー.
	private string		result = "";				// 直前の結果.

	private bool		is_launch_ball = false;		// 「ボール発射して」フラグ.

	private string[ ]	good_mess;					// 成功のときのメッセージ.
	private int			good_mess_index;			// 次に表示する成功メッセージ.

	// ---------------------------------------------------------------- //

	void Start ()
	{
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

		this.is_launch_ball = true;

		//

		this.good_mess = new string[4];

		this.good_mess[0] = "Nice!";
		this.good_mess[1] = "Okay!";
		this.good_mess[2] = "Yatta!";
		this.good_mess[3] = "*^o^*v";

		this.good_mess_index = 0;

	}

	void Update ()
	{
		// 「ボール発射して」フラグが立っていて、プレイヤーが着地中なら…….	
		if(this.is_launch_ball && this.player.isLanded()) {

			//

			GameObject ball = Instantiate(ball_prefab) as GameObject;

			ball.transform.position = new Vector3(5.0f, 2.0f, 0.0f);	

			//

			float		x_speed;
			float		y_speed;
			float		height;

			//　X方向のスピードと最高到達点の高さをランダムで求める.

			// 前回の値とある程度差がつくよう、あえて整数版を使う.
			x_speed = -Random.Range(2, 7)*2.5f;

			height  =  Random.Range(2.0f, 3.0f);

			// ボールの初速度のY成分を、X方向のスピードとプレイヤー位置での高さから求める.
			y_speed = this.calc_ball_y_speed(x_speed, height, ball.transform.position);

			Vector3		velocity = new Vector3(x_speed, y_speed, 0.0f);

			ball.GetComponent<Ball>().velocity = velocity;

			// 「ボール発射して」フラグを下す.
			this.is_launch_ball = false;
		}
	}

	// ボールの初速度のY成分を、X方向のスピードとプレイヤー位置での高さから求める.
	private float	calc_ball_y_speed(float x_speed, float height, Vector3 ball_position)
	{
		float		t;
		float		y_speed;

		// プレイヤーの位置にたどり着くまでの時間.
		t = (this.player.transform.position.x - ball_position.x)/x_speed;

		// y = v*t - 0.5f*g*t*t
		// から v を求める.
		y_speed = ((height - ball_position.y) - 0.5f*Physics.gravity.y*t*t)/t;

		return(y_speed);
	}

	// 成功/失敗をセットする.
	public void setResult(bool is_success)
	{
		if(is_success) {

			// 成功なら、成功メッセージを順番に表示.

			this.result = this.good_mess[this.good_mess_index];

			this.good_mess_index = (this.good_mess_index + 1)%4;

		} else {

			this.result = "Miss!";
		}

		// 一定時間経過後に、結果表示を消す.
		StartCoroutine(clearResult());
	}

	// 一定時間経過後に、結果表示を消すためのコルーチン.
	private IEnumerator clearResult()
	{
		yield return new WaitForSeconds(0.5f);

		this.result = "";
	}

	public void OnGUI()
	{
		GUI.Label(new Rect(200, 200, 200, 20), this.result);
	}

	// ボールが削除されたとき.
	public void OnBallDestroy()
	{
		// 「ボール発射して」フラグを立てる
		is_launch_ball = true;
	}
}
