using UnityEngine;
using System.Collections;

// MeshCollider が必要.
[RequireComponent(typeof(MeshCollider))]
public class PieceControl : MonoBehaviour {

	// ゲームカメラ.
	private	GameObject	game_camera;


	public PazzleControl	pazzle_control = null;

	public GameControl		game_control = null;

	// -------------------------------------------------------- //

	// ドラッグするときに、マウスカーソルの位置が常に最初につかんだ場所に
	// なるようにピースを移動する
	// （false にすると、マウスカーソルの位置＝ピースの中心になる）.
	private static bool	IS_ENABLE_GRAB_OFFSET = true;


	private static float	HEIGHT_OFFSET_BASE = 0.1f;

	private static float	SNAP_SPEED_MIN = 0.01f*60.0f;
	private static float	SNAP_SPEED_MAX = 0.8f*60.0f;

	// -------------------------------------------------------- //

	// マウスでつかんだ位置.
	private	Vector3		grab_offset = Vector3.zero;

	// ドラッグ中？.
	private	bool		is_now_dragging = false;

	// 初期位置＝正解の位置.
	public Vector3		finished_position;

	// スタート時の位置（ばらばらにばらまかれた状態）.
	public Vector3		start_position;

	public float		height_offset = 0.0f;

	public float		roll = 0.0f;

	// スナップする距離.
	//（スナップ＝正解の位置に近い場所で離されたときに、正解の位置に吸着する処理）.
	static float		SNAP_DISTANCE = 0.5f;

	// ピースの状態.
	enum STEP {

		NONE = -1,

		IDLE = 0,		// 未正解.
		DRAGING,		// ドラッグ中.
		FINISHED,		// 正解の位置に置かれている（もうドラッグできない）.
		RESTART,		// 最初から.
		SNAPPING,		// スナップ中.

		NUM,
	};

	// ピースの現在の状態.
	private STEP step      = STEP.NONE;

	private STEP next_step = STEP.NONE;

	// スナップ時の目標位置.
	private Vector3		snap_target;

	// スナップ後に移行する状態.
	private STEP		next_step_snap;

	// -------------------------------------------------------- //

	void 	Awake()
	{
		// 初期位置＝正解の位置を覚えておく.
		this.finished_position = this.transform.position;

		// （仮）で初期化
		// 後でシャッフルした位置に置きなおす.
		this.start_position = this.finished_position;
	}

	void	Start()
	{
		// カメラのインスタンスを探しておく.
		this.game_camera = GameObject.FindGameObjectWithTag("MainCamera");

		// Start() は　PazzleControl.Start() での処理がすべて終わったあとに呼ばれるので、
		// position がすでに移動済みのものになってしまう.
		//this.initial_position = this.transform.position;

		this.game_control =  GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();

	}
	void 	Update()
	{
		Color	color = Color.white;

		// 状態遷移

		switch(this.step) {

			case STEP.NONE:
			{
				this.next_step = STEP.RESTART;
			}
			break;

			case STEP.IDLE:
			{
				if(this.is_now_dragging) {

					// ドラッグ開始.

					this.next_step = STEP.DRAGING;
				}
			}
			break;

			case STEP.DRAGING:
			{
				if(this.is_in_snap_range()) {

					// スナップできるところ（正解の位置に近い）にいるとき.

					bool	is_snap = false;

					// ボタンが離されたときにスナップする.
					if(!this.is_now_dragging) {

						is_snap = true;
					}
		
					if(is_snap) {

						// このピースはもうおしまい.

						this.next_step        = STEP.SNAPPING;
						this.snap_target      = this.finished_position;
						this.next_step_snap   = STEP.FINISHED;

						this.game_control.playSe(GameControl.SE.ATTACH);
					}

				} else {
	
					// スナップしないところ（正解の位置から遠い）にいるとき.

					if(!this.is_now_dragging) {

						// ボタンが離された.

						this.next_step = STEP.IDLE;

						this.game_control.playSe(GameControl.SE.RELEASE);
					}
				}
			}
			break;

			case STEP.SNAPPING:
			{
				// 目標の位置にたどり着いたらおしまい.

				if((this.transform.position - this.snap_target).magnitude < 0.0001f) {

					this.next_step = this.next_step_snap;
				}
			}
			break;
		}

		// 状態遷移時の、初期化処理.

		while(this.next_step != STEP.NONE) {

			this.step = this.next_step;

			this.next_step = STEP.NONE;

			switch(this.step) {

				case STEP.IDLE:
				{
					this.SetHeightOffset(this.height_offset);
				}
				break;

				case STEP.RESTART:
				{
					this.transform.position = this.start_position;

					this.SetHeightOffset(this.height_offset);

					this.next_step = STEP.IDLE;
				}
				break;

				case STEP.DRAGING:
				{
					this.begin_dragging();

					// ドラッグ開始されたことを、パズルの管理クラスに通達する.
					this.pazzle_control.PickPiece(this);

					this.game_control.playSe(GameControl.SE.GRAB);
				}
				break;

				case STEP.FINISHED:
				{
					// 正解の場所に近いところで離された.
		
					// 正解の位置にスナップする.
					this.transform.position = this.finished_position;
		
					// このピースが正解の位置のおかれたことを、パズルの管理クラスに通知する.
					this.pazzle_control.FinishPiece(this);

				}
				break;
			}
		}

		// 各状態の実行処理.
		
		this.transform.localScale = Vector3.one;

		switch(this.step) {

			case STEP.DRAGING:
			{
				this.do_dragging();

				// スナップ範囲（正解の位置に十分近い）に入ったら、色を明るくする.
				if(this.is_in_snap_range()) {
	
					color *= 1.5f;
				}
	
				this.transform.localScale = Vector3.one*1.1f;
			}
			break;

			case STEP.SNAPPING:
			{
				// 目標位置に向かって移動する.

				Vector3	next_position, distance, move;

				// イージングっぽい動きになるようにする.

				distance = this.snap_target - this.transform.position;

				// 次の場所＝今の場所と目標位置の中間地点.
				distance *= 0.25f*(60.0f*Time.deltaTime);

				next_position = this.transform.position + distance;

				move = next_position - this.transform.position;

				float	snap_speed_min = PieceControl.SNAP_SPEED_MIN*Time.deltaTime;
				float	snap_speed_max = PieceControl.SNAP_SPEED_MAX*Time.deltaTime;

				if(move.magnitude < snap_speed_min) {

					// 移動量が一定以下になったらおしまい.
					// 目標位置ぴったりに移動させる.
					// 終了判定は状態遷移チェックで行い、ここでは位置の調整のみ行う.
					// 
					this.transform.position = this.snap_target;

				} else {

					// 移動速度が速すぎる場合は調整する.
					if(move.magnitude > snap_speed_max) {

						move *= snap_speed_max/move.magnitude;

						next_position = this.transform.position + move;
					}

					this.transform.position = next_position;
				}
			}
			break;
		}

		this.GetComponent<Renderer>().material.color = color;

		// バウンディングボックスを描画する.
		//
		//PazzleControl.DrawBounds(this.GetBounds(this.transform.position));

		//
	}

	// ドラッグ開始時の処理.
	private void begin_dragging()
	{
		do {

			// カーソル座標を、3D空間のワールド座標に変換する

			Vector3 world_position;
	
			if(!this.unproject_mouse_position(out world_position, Input.mousePosition)) {

				break;
			}

			if(PieceControl.IS_ENABLE_GRAB_OFFSET) {

				// オフセット（クリックした位置が、ピースの中心からどれぐらいずれているか）を求めておく.
				this.grab_offset = this.transform.position - world_position;
			}

		} while(false);
	}

	// ドラッグ中の処理.
	private void do_dragging()
	{
		do {

			// カーソル座標を、3D空間のワールド座標に変換する
			Vector3 world_position;

			if(!this.unproject_mouse_position(out world_position, Input.mousePosition)) {

				break;
			}

			// カーソル座標（３D）にオフセットを足して、ピースの中心座標を計算する.
			this.transform.position = world_position + this.grab_offset;

		} while(false);
	}

	// ピースのバウンディングボックスを取得する.
	public Bounds	GetBounds(Vector3 center)
	{
		// Mesh は Component ではないので、GetComponent<Mesh>() とはできない.

		Bounds	bounds = this.GetComponent<MeshFilter>().mesh.bounds;

		// 中心位置をセットする.
		// （min、max も自動的に再計算される）.
		bounds.center = center;

		return(bounds);
	}

	public void	Restart()
	{
		this.next_step = STEP.RESTART;
	}

	// マウスボタンが押されたとき.
	void 	OnMouseDown()
	{
		this.is_now_dragging = true;
	}

	// マウスボタンが離されたとき.
	void 	OnMouseUp()
	{
		this.is_now_dragging = false;
	}

	// 高さのオフセットをセットする.
	public void	SetHeightOffset(float height_offset)
	{
		Vector3		position = this.transform.position;

		this.height_offset = 0.0f;

		// 正解の位置に置かれる前のみ、有効.
		if(this.step != STEP.FINISHED || this.next_step != STEP.FINISHED) {

			this.height_offset = height_offset;
	
			position.y  = this.finished_position.y + PieceControl.HEIGHT_OFFSET_BASE;
			position.y += this.height_offset;
	
			this.transform.position = position;
		}
	}

	// マウスの位置を、３D空間のワールド座標に変換する.
	//
	// ・マウスカーソルとカメラの位置を通る直線
	// ・ 地面の当たり判定となる平面
	//　↑の二つが交わるところを求めます.
	//
	public bool		unproject_mouse_position(out Vector3 world_position, Vector3 mouse_position)
	{
		bool	ret;
		float	depth;

		// ピースの中心を通る、水平（法線がY軸。XZ平面）な面.
		Plane	plane = new Plane(Vector3.up, new Vector3(0.0f, this.transform.position.y, 0.0f));

		// カメラ位置とマウスカーソルの位置を通る直線.
		Ray		ray = this.game_camera.GetComponent<Camera>().ScreenPointToRay(mouse_position);

		// 上の二つが交わるところを求める.

		if(plane.Raycast(ray, out depth)) {

			world_position = ray.origin + ray.direction*depth;

			ret = true;

		} else {

			world_position = Vector3.zero;

			ret = false;
		}

		return(ret);
	}

	// スナップ可能な位置？（正解の位置に近い場所で離されたら、正解の位置に吸着する）.
	private bool	is_in_snap_range()
	{
		bool	ret = false;

		if(Vector3.Distance(this.transform.position, this.finished_position) < PieceControl.SNAP_DISTANCE) {

			ret = true;
		}

		return(ret);
	}
}
