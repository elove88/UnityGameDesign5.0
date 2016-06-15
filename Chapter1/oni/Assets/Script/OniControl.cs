using UnityEngine;
using System.Collections;

public class OniControl : MonoBehaviour {

	// プレイヤー.
	public PlayerControl		player = null;

	// カメラ.
	public GameObject	main_camera = null;

	// コリジョンボックスの大きさ（１辺の長さ）.
	public static float collision_size = 0.5f;

	// まだ生きてる？.
	private bool	is_alive = true;

	// 生成された時の位置.
	private Vector3	initial_position;

	// 左右に蛇行するときの、蛇行の周期.
	public float	wave_angle_offset = 0.0f;

	// 左右に蛇行するときの、蛇行の幅.
	public float	wave_amplitude = 0.0f;

	// おにの状態.
	enum STEP {

		NONE = -1,

		RUN = 0,			// 走って逃げてる.
		DEFEATED,			// 斬られて吹き飛び中.

		NUM,
	};

	// 現在の状態.
	private	STEP		step      = STEP.NONE;

	// 次に遷移する状態.
	private	STEP		next_step = STEP.NONE;

	// [sec] 状態が遷移してからの時間.
	private float		step_time = 0.0f;

	// DEFEATED, FLY_TO_STACK 開始時のときの速度ベクトル.
	private Vector3		blowout_vector = Vector3.zero;
	private Vector3		blowout_angular_velocity = Vector3.zero;

	// -------------------------------------------------------------------------------- //

	void 	Start()
	{
		// 生成された時の位置.
		this.initial_position = this.transform.position;

		this.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);

		this.GetComponent<Collider>().enabled = false;

		// 回転速度の制限を制限なしにしておく.
		this.GetComponent<Rigidbody>().maxAngularVelocity = float.PositiveInfinity;

		// モデルのセンターが少し下にあるので、重心をずらしておく.
		this.GetComponent<Rigidbody>().centerOfMass = new Vector3(0.0f, 0.5f, 0.0f);

	}
	void	Update()
	{
		this.step_time += Time.deltaTime;

		// 状態遷移のチェック.
		// （今のところ、外部からのリクエスト以外で遷移することはない）.

		switch(this.step) {

			case STEP.NONE:
			{
				this.next_step = STEP.RUN;
			}
			break;
		}

		// 初期化.
		// 状態が遷移したときの、初期化処理.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.DEFEATED:
				{
					this.GetComponent<Rigidbody>().velocity = this.blowout_vector;

					// 回転の角速度.
					this.GetComponent<Rigidbody>().angularVelocity = this.blowout_angular_velocity;

					// 親子関係を外しておく.
					// 親（OniGroup）が削除されるといっしょに削除されてしまうので.
					this.transform.parent = null;
			
					// カメラの座標系内で動くようにする
					// （カメラの動きと連動するようになります）.
					if(SceneControl.IS_ONI_BLOWOUT_CAMERA_LOCAL) {
			
						this.transform.parent = this.main_camera.transform;
					}

					// やられモーションを再生する.
					this.transform.GetChild(0).GetComponent<Animation>().Play("oni_yarare");

					this.is_alive = false;
				}
				break;
			}

			this.step = this.next_step;

			this.next_step = STEP.NONE;

			this.step_time = 0.0f;
		}

		// 各状態での実行処理.

		Vector3	new_position = this.transform.position;

		float low_limit = this.initial_position.y;

		switch(this.step) {

			case STEP.RUN:
			{
				// 生きている間は地面の下に落ちないようにする.

				if(new_position.y < low_limit) {
		
					new_position.y = low_limit;
				}
	
				// 左右に蛇行する.
	
				float	wave_angle = 2.0f*Mathf.PI*Mathf.Repeat(this.step_time, 1.0f) + this.wave_angle_offset;
	
				float	wave_offset = this.wave_amplitude*Mathf.Sin(wave_angle);
	
				new_position.z = this.initial_position.z + wave_offset;
	
				// 向き（Y軸回転）もそれっぽく.
				if(this.wave_amplitude > 0.0f) {
	
					this.transform.rotation = Quaternion.AngleAxis(180.0f - 30.0f*Mathf.Sin(wave_angle + 90.0f), Vector3.up);
				}

			}
			break;

			case STEP.DEFEATED:
			{
				// 死んだ直後に地面に潜ってしまうことがあったので、速度が上向き
				// （＝死んだ直後）のときは、地面の下に落ちないようにする.
				if(new_position.y < low_limit) {
	
					if(this.GetComponent<Rigidbody>().velocity.y > 0.0f) {
	
						new_position.y = low_limit;
					}
				}
	
				// 少し後ろに流されているように見せたい.
				if(this.transform.parent != null) {
	
					this.GetComponent<Rigidbody>().velocity += -3.0f*Vector3.right*Time.deltaTime;
				}
			}
			break;

		}

		this.transform.position = new_position;

		// 不要になったら削除する.
		//
		// ・画面外にでたとき
		// ・やられた後
		// ・SEの再生が止まっている
		//
		// OnBecameInvisible() は画面外に出た瞬間しか呼ばれないので、
		// 『画面外でしばらく音が鳴り続けた後』削除したいようなときには使えない.
		//

		do {

			// 画面外でオニ（オニグループ）を発生させるので、発生した瞬間も
			// 呼ばれてしまう。なので、this.is_alive をチェックして、死亡状態で
			// 画面外に出たときのみ、削除するようにする.
			if(this.GetComponent<Renderer>().isVisible) {

				break;
			}

			if(this.is_alive) {

				break;
			}

			// SE を再生している間は削除しない.
			if(this.GetComponent<AudioSource>().isPlaying) {

				if(this.GetComponent<AudioSource>().time < this.GetComponent<AudioSource>().clip.length) {

					break;
				}
			}

			//

			Destroy(this.gameObject);

		} while(false);
	}

	// モーションの再生スピードを設定する.
	public void setMotionSpeed(float speed)
	{
		this.transform.GetChild(0).GetComponent<Animation>()["oni_run1"].speed = speed;
		this.transform.GetChild(0).GetComponent<Animation>()["oni_run2"].speed = speed;
	}

	// 攻撃を受けたときの処理を開始する.
	public void AttackedFromPlayer(Vector3 blowout, Vector3	angular_velocity)
	{
		this.blowout_vector           = blowout;
		this.blowout_angular_velocity = angular_velocity;

		// 親子関係を外しておく.
		// 親（OniGroup）が削除されるといっしょに削除されてしまうので.
		this.transform.parent = null;

		this.next_step = STEP.DEFEATED;
	}

}
