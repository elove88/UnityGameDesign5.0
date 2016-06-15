using UnityEngine;
using System.Collections;

public class OniGroupControl : MonoBehaviour {

	// プレイヤー.
	public PlayerControl	player = null;

	// カメラ.
	public GameObject	main_camera = null;

	// シーンコントロール.
	public SceneControl	scene_control = null;

	// オニのプレハブ
	public GameObject[]	OniPrefab;
	
	// 影のプレハブ
	public GameObject	ShadowPrefab;
	
	public AudioClip[]	YarareLevel1;
	public AudioClip[]	YarareLevel2;
	public AudioClip[]	YarareLevel3;

	// グループに属する OniPrefab のインスタンス.
	public OniControl[]	onis;

	// -------------------------------------------------------------------------------- //

	// コリジョンボックスの大きさ（１辺の長さ）.
	public static float collision_size = 2.0f;

	// グループに属するオニの数.
	private	int		oni_num;
	
	// 今までのオニの最大数.
	static private int	oni_num_max = 0;

	// グループ全体が進む速度.
	public float	run_speed = SPEED_MIN;

	// プレイヤーとぶつかった？.
	public bool	is_player_hitted = false;

	// -------------------------------------------------------------------------------- //

	// タイプ.

	public enum TYPE {

		NONE = -1,

		NORMAL = 0,			// ふつう.

		DECELERATE,			// 途中で減速.
		LEAVE,				// 画面右に急いで退場（プレイヤーがミスした直後）.
		NUM,
	};

	public TYPE		type = TYPE.NORMAL;

	// スピード制御の情報（TYPE = DECELERATE のとき）.
	public struct Decelerate {

		public bool		is_active;			// 減速動作中？.
		public float	speed_base;			// 減速動作を開始する前のスピード.
		public float	timer;
	};

	public Decelerate	decelerate;

	// -------------------------------------------------------------------------------- //

	public static float		SPEED_MIN = 2.0f;			// 移動スピードの最低値.
	public static float		SPEED_MAX = 10.0f;			// 移動スピードの最高値.
	public static float		LEAVE_SPEED = 10.0f;		// 退場する時のスピード.

	// -------------------------------------------------------------------------------- //

	void	Start()
	{
		// コリジョンを表示する（デバッグ用）.
		this.gameObject.GetComponent<Renderer>().enabled = SceneControl.IS_DRAW_ONI_GROUP_COLLISION;

		this.decelerate.is_active = false;
		this.decelerate.timer     = 0.0f;
	}

	void	Update()
	{
		this.speed_control();

		this.transform.rotation = Quaternion.identity;

		// 退場モードのときは、画面外にでたら削除する.
		// （renderer を disable にしているので、OnBecameInvisible
		// 　は使えない）.
		//
		if(this.type == TYPE.LEAVE) {

			// グループのおに全部が画面外だったら、グループごと削除する.

			bool	is_visible = false;

			foreach(var oni in this.onis) {

				if(oni.GetComponent<Renderer>().isVisible) {

					is_visible = true;
					break;
				}
			}

			if(!is_visible) {

				Destroy(this.gameObject);
			}
		}
	}

	void FixedUpdate()
	{
		Vector3	new_position = this.transform.position;

		new_position.x += this.run_speed*Time.deltaTime;

		this.transform.position = new_position;
	}

	// 走るスピードの制御.
	private void	speed_control()
	{
		switch(this.type) {

			case TYPE.DECELERATE:
			{
				// プレイヤーとの距離がこれ以下になったら、減速動作を始める.
				//
				const float	decelerate_start = 8.0f;

				if(this.decelerate.is_active) {

					// １．加速して逃げる.
					// ２．プレイヤーと同じ速度でしばらく粘る.
					// ３．やっぱだめだ～という感じで一気に減速.

					float	rate;

					const float		time0 = 0.7f;
					const float		time1 = 0.4f;
					const float		time2 = 2.0f;

					const float		speed_max = 30.0f;
					      float		speed_min = OniGroupControl.SPEED_MIN;

					float	time = this.decelerate.timer;

					do {

						// 加速する.

						if(time < time0) {

							rate = Mathf.Clamp01(time/time0);
	
							rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate)) + 1.0f)/2.0f;

							this.run_speed = Mathf.Lerp(this.decelerate.speed_base, speed_max, rate);

							this.set_oni_motion_speed(2.0f);

							break;
						}
						time -= time0;

						// プレイヤーと同じ速度まで減速.

						if(time < time1) {

							rate = Mathf.Clamp01(time/time1);
	
							rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate)) + 1.0f)/2.0f;

							this.run_speed = Mathf.Lerp(speed_max, PlayerControl.RUN_SPEED_MAX, rate);

							break;
						}
						time -= time1;

						// ものすごく遅い速度まで減速.

						if(time < time2) {

							rate = Mathf.Clamp01(time/time2);
	
							rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, rate)) + 1.0f)/2.0f;

							this.run_speed = Mathf.Lerp(PlayerControl.RUN_SPEED_MAX, speed_min, rate);

							this.set_oni_motion_speed(1.0f);

							break;
						}
						time -= time2;

						//

						this.run_speed = speed_min;

					} while(false);

					this.decelerate.timer += Time.deltaTime;

				} else {

					float	distance = this.transform.position.x - this.player.transform.position.x;

					if(distance < decelerate_start) {

						this.decelerate.is_active  = true;
						this.decelerate.speed_base = this.run_speed;
						this.decelerate.timer      = 0.0f;
					}
				}
			}
			break;

			case TYPE.LEAVE:
			{
				this.run_speed = LEAVE_SPEED;
			}
			break;

		}

	}

	// オニのグループを生成する.
	public void	CreateOnis(int oni_num, Vector3 base_position)
	{
		this.oni_num = oni_num;
		oni_num_max = Mathf.Max( oni_num_max, oni_num );
		
		this.onis = new OniControl[this.oni_num];
		
		Vector3		average		= new Vector3( 0.0f, 0.0f, 0.0f );
		Vector3		position;

		for(int i = 0;i < this.oni_num;i++) {

			GameObject	go = Instantiate(this.OniPrefab[i%this.OniPrefab.Length]) as GameObject;

			this.onis[i] = go.GetComponent<OniControl>();

			// オニの位置をばらつかせる.

			position = base_position;

			if(i == 0) {

				// かならず一つはプレイヤーと正面からぶつかるようにしたいので、
				// ０番目はオフセットをつけない.				
			
			} else {

				// 乱数を使って、位置をばらつかせる.

				Vector3	splat_range;
				
				// グループ内のオニの数（一度に出現する数）が多いほど、ばらまく範囲が広くなるように.
				splat_range.x = OniControl.collision_size*(float)(oni_num - 1);
				splat_range.z = OniControl.collision_size*(float)(oni_num - 1)/2.0f;

				// ばらまく範囲が広くなりすぎないように
				// プレイヤーが刀の一振りで切れる範囲に収める.
				splat_range.x = Mathf.Min(splat_range.x, OniGroupControl.collision_size);
				splat_range.z = Mathf.Min(splat_range.z, OniGroupControl.collision_size/2.0f);
			
				position.x += Random.Range(0.0f, splat_range.x);
				position.z += Random.Range(-splat_range.z, splat_range.z);
			}

			position.y = 0.0f;
			
			
			this.onis[i].transform.position = position;
			this.onis[i].transform.parent = this.transform;

			this.onis[i].player      = this.player;
			this.onis[i].main_camera = this.main_camera;

			this.onis[i].wave_amplitude    = (i + 1)*0.1f;
			this.onis[i].wave_angle_offset = (i + 1)*Mathf.PI/4.0f;
			
			average		+= this.onis[i].transform.localPosition;
		}
		
		
		GameObject	shadow = Instantiate(this.ShadowPrefab) as GameObject;
		
		// 平均の位置へ影を置く
		average		/= this.oni_num;
		// 高さは固定
		average.y	= 15.0f;
		
		shadow.transform.parent			= this.transform;
		shadow.transform.localPosition	= average;
	}

	private static int	count = 0;

	// プレイヤーの攻撃を受けたとき.
	public void OnAttackedFromPlayer()
	{

		// 倒したオニの数を増やす.
		// （↓の中で評価の計算もしているので、先に実行しておく）.
		this.scene_control.AddDefeatNum(this.oni_num);

		// オニをばらばらに吹き飛ばす.
		//
		// 円すいの表面にそうような形で、それぞれのオニを吹き飛ばす方向を決めます.
		// 評価が高いほど円すいが末広がりになって、より広い範囲にばら撒かれます.
		// プレイヤーの速度が速いと、円すいが前のめりになります.

		Vector3			blowout;				// オニが吹き飛ぶ方向（速度ベクトル）
		Vector3			blowout_up;				// ↑の、垂直成分
		Vector3			blowout_xz;				// ↑の、水平成分

		float			y_angle;
		float 			blowout_speed;
		float			blowout_speed_base;

		float			forward_back_angle;		// 円すいの前後の傾き.

		float			base_radius;			// 円すいの底面の半径.

		float			y_angle_center;
		float			y_angle_swing;			// 円弧の中心（モーションの左右によって決まる値）.

		float			arc_length;				// 円弧の長さ（円周）.

		switch(this.scene_control.evaluation) {

			default:
			case SceneControl.EVALUATION.OKAY:
			{
				base_radius = 0.3f;

				blowout_speed_base = 10.0f;

				forward_back_angle = 40.0f;

				y_angle_center = 180.0f;
				y_angle_swing  = 10.0f;
			}
			break;

			case SceneControl.EVALUATION.GOOD:
			{
				base_radius = 0.3f;

				blowout_speed_base = 10.0f;

				forward_back_angle = 0.0f;

				y_angle_center = 0.0f;
				y_angle_swing = 60.0f;
			}
			break;

			case SceneControl.EVALUATION.GREAT:
			{
				base_radius = 0.5f;

				blowout_speed_base = 15.0f;

				forward_back_angle = -20.0f;

				y_angle_center = 0.0f;
				y_angle_swing = 30.0f;
			}
			break;
		}

		forward_back_angle += Random.Range(-5.0f, 5.0f);

		arc_length = (this.onis.Length - 1)*30.0f;

		arc_length = Mathf.Min(arc_length, 120.0f);

		// プレイヤーのモーション（右切り、左切り）で、左右に飛ばす方向を変える.

		y_angle = y_angle_center;

		y_angle += -arc_length/2.0f;

		if(this.player.attack_motion == PlayerControl.ATTACK_MOTION.RIGHT) {

			y_angle += y_angle_swing;

		} else {

			y_angle -= y_angle_swing;
		}

		y_angle += ((OniGroupControl.count*7)%11)*3.0f;

		// グループに属するオニ全部をやられたことにする.
		foreach(OniControl oni in this.onis) {

			//

			blowout_up = Vector3.up;

			blowout_xz = Vector3.right*base_radius;

			blowout_xz = Quaternion.AngleAxis(y_angle, Vector3.up)*blowout_xz;

			blowout = blowout_up + blowout_xz;

			blowout.Normalize();

			// 円すいを前後に傾ける.

			blowout = Quaternion.AngleAxis(forward_back_angle, Vector3.forward)*blowout;

			// 吹き飛びの速度.

			blowout_speed = blowout_speed_base*Random.Range(0.8f, 1.2f);

			blowout *= blowout_speed;

			if(!SceneControl.IS_ONI_BLOWOUT_CAMERA_LOCAL) {

				// グローバルで吹き飛ぶ（カメラの動きと連動しない）ときは、
				// プレイヤーの速度を足す.
				blowout += this.player.GetComponent<Rigidbody>().velocity;
			}

			// 回転.

			Vector3	angular_velocity = Vector3.Cross(Vector3.up, blowout);

			angular_velocity.Normalize();
			angular_velocity *= 3.14f*8.0f*blowout_speed/15.0f*Random.Range(0.5f, 1.5f);

			//angular_velocity = Quaternion.AngleAxis(Random.Range(-30.0f, 30.0f), Vector3.up)*angular_velocity;

			//

			oni.AttackedFromPlayer(blowout, angular_velocity);

			//Debug.DrawRay(this.transform.position, blowout*2.0f, Color.white, 1000.0f);

			//

			y_angle += arc_length/(this.onis.Length - 1);

		}

		// やられ声のSEを鳴らす.
		// たくさん鳴るときれいに聞こえないので、いっこだけ.
		//
		if(this.onis.Length > 0)
		{
			AudioClip[]	yarareSE = null;
			
			if( this.onis.Length >= 1 && this.onis.Length < 3 )
			{
				yarareSE = this.YarareLevel1;
			}
			else if( this.onis.Length >= 3 && this.onis.Length < 8 )
			{
				yarareSE = this.YarareLevel2;
			}
			else if( this.onis.Length >= 8 )
			{
				yarareSE = this.YarareLevel3;
			}
			
			if( yarareSE != null )
			{
				int index = Random.Range( 0, yarareSE.Length );
				
				this.onis[0].GetComponent<AudioSource>().clip = yarareSE[index];
				this.onis[0].GetComponent<AudioSource>().Play();
			}
		}

		OniGroupControl.count++;

		// インスタンスを削除する.
		//
		// Destroy(this) とすると　OniGroupPrefab のインスタンスではなく、スクリプト（OniGroupControl）
		// を削除してしまうので、注意すること.
		//
		Destroy(this.gameObject);

	}

	// -------------------------------------------------------------------------------- //

	// プレイヤーがぶつかったときの処理.
	public void	onPlayerHitted()
	{
		this.scene_control.result.score_max += this.scene_control.eval_rate_okay * oni_num_max * this.scene_control.eval_rate;
		this.is_player_hitted = true;
	}

	// 退場を開始する.
	public void	beginLeave()
	{
		this.GetComponent<Collider>().enabled = false;
		this.type = TYPE.LEAVE;
	}

	// おにのモーションの再生スピードをセットする.
	private void	set_oni_motion_speed(float speed)
	{
		foreach(OniControl oni in this.onis) {

			oni.setMotionSpeed(speed);
		}
	}

}
