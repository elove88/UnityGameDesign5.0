using UnityEngine;
using System.Collections;

// おにの出現を制御する.
public class LevelControl {

	// -------------------------------------------------------------------------------- //
	// プレハブ.

	public GameObject	OniGroupPrefab = null;

	// -------------------------------------------------------------------------------- //

	public SceneControl		scene_control = null;
	public PlayerControl	player = null;

	// オニが発生する位置
	// プレイヤーのX座標がこのラインを超えたら、プレイヤーの前方に
	// オニを発生させる.
	private float		oni_generate_line;

	// プレイヤーの appear_margin 前方の位置にオニが発生する.
	private float		appear_margin = 15.0f;

	// １グループのオニの数（＝一度に出現するオニの数）.
	private int			oni_appear_num = 1;

	// 連続成功のカウント.
	private int			no_miss_count = 0;

	// おにのタイプ.
	public enum GROUP_TYPE {

		NONE = -1,

		SLOW = 0,			// おそい.
		DECELERATE,			// 途中で減速.
		PASSING,			// ふたつのグループで追い抜き.
		RAPID,				// 超短い間隔で.

		NORMAL,				// ふつう.

		NUM,
	};

	public GROUP_TYPE		group_type      = GROUP_TYPE.NORMAL;
	public GROUP_TYPE		group_type_next = GROUP_TYPE.NORMAL;

	private	bool	can_dispatch = false;

	// ランダム制御（通常のゲーム）.
	public	bool	is_random = true;

	// 次のグループの発生位置（ノーマルのとき　プレイヤーの位置からのオフセット）.
	private float			next_line = 50.0f;

	// 次のグループのスピード（ノーマルのとき）.
	private	float			next_speed = OniGroupControl.SPEED_MIN*5.0f;

	// 残りのノーマル発生回数.
	private int				normal_count = 5;

	// 残りのイベント発生回数.
	private int				event_count = 1;

	// 発生中のイベント.
	private GROUP_TYPE		event_type = GROUP_TYPE.NONE;
	
	// -------------------------------------------------------------------------------- //

	public static float	INTERVAL_MIN = 20.0f;			// おにが出現する間隔の最短値.
	public static float	INTERVAL_MAX = 50.0f;			// おにが出現する間隔の最長値.

	// -------------------------------------------------------------------------------- //

	public void	create()
	{
		// ゲーム開始直後に最初のオニが発生するよう、
		// 発生位置をプレイヤーの後方に初期化しておく.
		this.oni_generate_line = this.player.transform.position.x - 1.0f;

	}

	public void OnPlayerMissed()
	{
		// 一度に出現するオニの数をリセットする.
		this.oni_appear_num = 1;

		this.no_miss_count = 0;
	}

	public void	oniAppearControl()
	{
	#if false
		for(int i = 0;i < 4;i++) {

			if(Input.GetKeyDown((KeyCode)(KeyCode.Alpha1 + i))) {

				this.group_type_next = (GROUP_TYPE)i;

				this.is_random = false;
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha0)) {

			this.is_random = !this.is_random;
		}
	#endif

		// プレイヤーが一定距離進むごとに、オニのグループを発生させる.

		if(this.can_dispatch) {

			// つぎのグループの発生準備ができている.

		} else {

			// つぎのグループの発生準備ができていない.

			if(this.is_one_group_only()) {

				// 特別パターンのときは、画面からおにがいなくなるのを待つ.

				if(GameObject.FindGameObjectsWithTag("OniGroup").Length == 0) {

					this.can_dispatch = true;
				}

			} else {

				// 通常パターンのときは、すぐに出せる.
				this.can_dispatch = true;
			}

			if(this.can_dispatch) {

				// 出現させる準備ができたら、プレイヤーの現在位置から出現位置を計算する.

				if(this.group_type_next == GROUP_TYPE.NORMAL) {

					this.oni_generate_line = this.player.transform.position.x + this.next_line;

				} else if(this.group_type_next == GROUP_TYPE.SLOW) {

					this.oni_generate_line = this.player.transform.position.x + 50.0f;

				} else {

					this.oni_generate_line = this.player.transform.position.x + 10.0f;
				}
			}
		}

		// プレイヤーが一定距離進んだら、次のグループを発生させる.

		do {
			if(this.scene_control.oni_group_num >= this.scene_control.oni_group_appear_max )
			{
				break;
			}
			
			if(!this.can_dispatch) {

				break;
			}

			if(this.player.transform.position.x <= this.oni_generate_line) {

				break;
			}

			//

			this.group_type = this.group_type_next;

			switch(this.group_type) {
	
				case GROUP_TYPE.SLOW:
				{
					this.dispatch_slow();
				}
				break;
	
				case GROUP_TYPE.DECELERATE:
				{
					this.dispatch_decelerate();
				}
				break;

				case GROUP_TYPE.PASSING:
				{
					this.dispatch_passing();
				}
				break;

				case GROUP_TYPE.RAPID:
				{
					this.dispatch_rapid();
				}
				break;

				case GROUP_TYPE.NORMAL:
				{
					this.dispatch_normal(this.next_speed);
				}
				break;
			}
	
			// 次回出現するグループのオニの数を更新しておく
			// （だんだん増える）.
			this.oni_appear_num++;
	
			this.oni_appear_num = Mathf.Min(this.oni_appear_num, SceneControl.ONI_APPEAR_NUM_MAX);

			this.can_dispatch = false;

			this.no_miss_count++;

			this.scene_control.oni_group_num++;
			
			if(this.is_random) {

				// 次に出現するグループを選ぶ.
				this.select_next_group_type();
			}

		} while(false);
	}

	// 画面にひとつしかだせないグループ？.
	public bool	is_one_group_only()
	{
		bool	ret;

		do {

			ret = true;

			if(this.group_type == GROUP_TYPE.PASSING || this.group_type_next == GROUP_TYPE.PASSING) {

				break;
			}
			if(this.group_type == GROUP_TYPE.DECELERATE || this.group_type_next == GROUP_TYPE.DECELERATE) {

				break;
			}
			if(this.group_type == GROUP_TYPE.SLOW || this.group_type_next == GROUP_TYPE.SLOW) {

				break;
			}

			ret = false;

		} while(false);

		return(ret);
	}

	public void select_next_group_type()
	{

		// ノーマルとイベントの遷移チェック.

		if(this.event_type != GROUP_TYPE.NONE) {

			this.event_count--;

			if(this.event_count <= 0) {

				this.event_type = GROUP_TYPE.NONE;

				this.normal_count = Random.Range(3, 7);
			}

		} else {

			this.normal_count--;

			if(this.normal_count <= 0) {

				// イベントを発生させる.

				this.event_type = (GROUP_TYPE)Random.Range(0, 4);

				switch(this.event_type) {

					default:
					case GROUP_TYPE.DECELERATE:
					case GROUP_TYPE.PASSING:
					case GROUP_TYPE.SLOW:
					{
						this.event_count = 1;
					}
					break;

					case GROUP_TYPE.RAPID:
					{
						this.event_count = Random.Range(2, 4);
					}
					break;
				}
			}
		}

		// ノーマル、イベントのグループを発生させる.

		if(this.event_type == GROUP_TYPE.NONE) {

			// ノーマルタイプのグループ.

			float		rate;
	
			rate = (float)this.no_miss_count/10.0f;
	
			rate = Mathf.Clamp01(rate);
	
			this.next_speed = Mathf.Lerp(OniGroupControl.SPEED_MAX, OniGroupControl.SPEED_MIN, rate);	

			this.next_line = Mathf.Lerp(LevelControl.INTERVAL_MAX, LevelControl.INTERVAL_MIN, rate);

			this.group_type_next = GROUP_TYPE.NORMAL;

		} else {

			// イベントタイプのグループ.

			this.group_type_next = this.event_type;
		}

	}

	// ノーマルパターン.
	public void dispatch_normal(float speed)
	{
		Vector3	appear_position = this.player.transform.position;

		// プレイヤーの前方、ぎりぎり画面外ぐらいの位置で発生する.
		appear_position.x += appear_margin;
		
		this.create_oni_group(appear_position, speed, OniGroupControl.TYPE.NORMAL);
	}

	// おそいパターン.
	public void dispatch_slow()
	{
		Vector3	appear_position = this.player.transform.position;

		// プレイヤーの前方、ぎりぎり画面外ぐらいの位置で発生する.
		appear_position.x += appear_margin;
		
		float		rate;

		rate = (float)this.no_miss_count/10.0f;

		rate = Mathf.Clamp01(rate);

		this.create_oni_group(appear_position, OniGroupControl.SPEED_MIN*5.0f, OniGroupControl.TYPE.NORMAL);
	}

	// 超短いパターン.
	public void dispatch_rapid()
	{
		Vector3	appear_position = this.player.transform.position;

		// プレイヤーの前方、ぎりぎり画面外ぐらいの位置で発生する.
		appear_position.x += appear_margin;
		
		//this.create_oni_group(appear_position, OniGroupControl.SPEED_MIN, OniGroupControl.TYPE.NORMAL);
		this.create_oni_group(appear_position, this.next_speed, OniGroupControl.TYPE.NORMAL);
	}

	// 途中で減速パターン.
	public void dispatch_decelerate()
	{
		Vector3	appear_position = this.player.transform.position;

		// プレイヤーの前方、ぎりぎり画面外ぐらいの位置で発生する.
		appear_position.x += appear_margin;
		
		this.create_oni_group(appear_position, 9.0f, OniGroupControl.TYPE.DECELERATE);
	}

	// 途中でおに同士で追い抜きが発生するパターン.
	public void dispatch_passing()
	{
		float	speed_low  = 2.0f;
		float	speed_rate = 2.0f;
		float	speed_high = (speed_low - this.player.GetComponent<Rigidbody>().velocity.x)/speed_rate + this.player.GetComponent<Rigidbody>().velocity.x;

		// 遅いおにが速いおにに追い抜かれる位置（0.0 プレイヤーの位置 ～ 1.0 画面右端）.
		float	passing_point = 0.5f;

		Vector3	appear_position = this.player.transform.position;

		// ふたつのグループが途中で交差するように、発生位置を調整する.

		appear_position.x = this.player.transform.position.x + appear_margin;
		
		this.create_oni_group(appear_position, speed_high, OniGroupControl.TYPE.NORMAL);

		appear_position.x = this.player.transform.position.x + appear_margin*Mathf.Lerp(speed_rate, 1.0f, passing_point);
		
		this.create_oni_group(appear_position, speed_low, OniGroupControl.TYPE.NORMAL);
	}

	// -------------------------------------------------------------------------------- //

	// オニのグループを発生させる.
	private void create_oni_group(Vector3 appear_position, float speed, OniGroupControl.TYPE type)
	{
		// -------------------------------------------------------- //
		// グループ全体のコリジョン（当たり判定）を生成する.	

		Vector3	position = appear_position;

		// OniGroupPrefab のインスタンスを生成します.
		// "as GameObject" を末尾につけると、生成されたオブジェクトは
		// GameObject オブジェクトになります.
		//
		GameObject 	go = GameObject.Instantiate(this.OniGroupPrefab) as GameObject;

		OniGroupControl new_group = go.GetComponent<OniGroupControl>();

		// 地面に接する高さ.
		position.y = OniGroupControl.collision_size/2.0f;

		position.z = 0.0f;

		new_group.transform.position = position;

		new_group.scene_control  = this.scene_control;
		new_group.main_camera    = this.scene_control.main_camera;
		new_group.player         = this.player;
		new_group.run_speed      = speed;
		new_group.type           = type;

		// -------------------------------------------------------- //
		// グループに属するオニの集団を生成する.

		Vector3	base_position = position;

		int		oni_num = this.oni_appear_num;

		// コリジョンボックスの左端によせる.
		base_position.x -= (OniGroupControl.collision_size/2.0f - OniControl.collision_size/2.0f);

		// 地面に接する高さ.
		base_position.y = OniControl.collision_size/2.0f;

		// オニを発生させる.
		new_group.CreateOnis(oni_num, base_position);

	}
}
