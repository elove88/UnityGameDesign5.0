using UnityEngine;
using System.Collections;

public class RoomControl : MonoBehaviour {

	public GameObject	roomPrefab = null;
	public GameObject	shojiPrefab = null;

	private FloorControl[]	rooms;

	// カメラ.
	private GameObject main_camera = null;

	public static float	MODEL_LENGTH   = 15.0f;
	public static float	MODEL_Z_OFFSET = 0.0f;

	public static float	RESTART_Z_OFFSET = 5.0f;		//リスタート位置のオフセット.

	public static int	MODEL_NUM = 3;

	private int		start_model_index = 0;				// いちばん手前にあるモデルのインデックス.

	private LevelControl	level_control;
	private	ShojiControl	shoji_control;				// 障子（ひとつを使いまわす）.
	private	SceneControl	scene_control;

	private int		room_count = 0;						// 進んだ部屋の数.
	private	bool	is_closed = false;					// 障子が閉まった？（次の部屋へ進むごとにリセット）.

	// ---------------------------------------------------------------- //
	
	// Sound
	public AudioClip CLOSE_SOUND = null;
	public AudioClip CLOSE_END_SOUND = null;
	
	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		this.rooms = new FloorControl[MODEL_NUM];

		for(int i = 0;i < 3;i++) {
	
			this.rooms[i] = (Instantiate(this.roomPrefab) as GameObject).GetComponent<FloorControl>();

			this.rooms[i].transform.position = new Vector3(0.0f, 0.0f, MODEL_Z_OFFSET + (float)i*MODEL_LENGTH);
		}

		this.start_model_index = 0;

		this.rooms[(this.start_model_index + 0)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 1)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 2)%MODEL_NUM].setClose();

		this.shoji_control = (Instantiate(this.shojiPrefab) as GameObject).GetComponent<ShojiControl>();

		this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);

		//

		// カメラのインスタンスを探しておく.
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");

		this.scene_control = this.main_camera.GetComponent<SceneControl>();

		this.level_control = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelControl>();
	}

	// Update is called once per frame
	void 	Update()
	{
		FloorControl	room = this.rooms[this.start_model_index];

		// 一番後ろのモデルがカメラの後ろに来たら、奥に移動させる.
		//
		if(room.transform.position.z + MODEL_LENGTH < this.main_camera.transform.position.z) {

			// 一番後ろのモデルを奥に移動させる.

			Vector3		new_position = room.transform.position;

			new_position.z += MODEL_LENGTH*MODEL_NUM;

			room.transform.position = new_position;

			//

			this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(null);

			// 「一番手前にあるモデルのインデックス」を進める.
			//
			this.start_model_index = (this.start_model_index + 1)%MODEL_NUM;


			// 一番手前の部屋　→　障子をアタッチして、開けた状態にしておく.

			if(this.scene_control.step == SceneControl.STEP.GAME) {

				this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);
				this.rooms[(this.start_model_index + 0)%MODEL_NUM].setOpen();
				
			} else {

				this.shoji_control.gameObject.SetActive(false);
			}

			// 二番目の部屋　→　ふすまを開き始める.

			this.rooms[(this.start_model_index + 1)%MODEL_NUM].beginOpen();

			// 三番目番目の部屋　→　ふすまが閉めきった状態にする.

			this.rooms[(this.start_model_index + 2)%MODEL_NUM].setClose();

			// 何度も通過した穴を鉄板にする.

			foreach(var paper_control in this.shoji_control.papers) {

				if(this.level_control.getChangeSteelCount() > 0) {

					if(paper_control.through_count >= this.level_control.getChangeSteelCount()) {

						paper_control.beginSteel();
					}
				}
			}

			//

			this.room_count++;
			this.is_closed = false;
		}

		// カメラが近づいたら、閉める.
		//
		// 本当はプレイヤーを見るべきだけど、部屋モデルのリピート処理がカメラを.
		// みている関係上、こちらもカメラを見る.

		float	close_distance = MODEL_LENGTH - this.level_control.getCloseDistance();

		if(room.transform.position.z + close_distance < this.main_camera.transform.position.z) {

			do {

				if(this.is_closed) {

					break;
				}

				// スタート直後は閉まらないようにする.
				if(this.room_count < 1) {

					break;
				}

				// 障子を閉める.
	
				if(this.scene_control.step == SceneControl.STEP.GAME) {
	
					FloorControl.CLOSING_PATTERN_TYPE	type;
					bool								is_flip;
					FloorControl.ClosingPatternParam	param;

					this.level_control.getClosingPattern(out type, out is_flip, out param);

					this.rooms[(this.start_model_index + 0)%MODEL_NUM].setClosingPatternType(type, is_flip, param);
					this.rooms[(this.start_model_index + 0)%MODEL_NUM].beginCloseShoji();
				}

				this.is_closed = true;

			} while(false);
		}

	#if false
		// デバッグ機能.
		// フルキーの数字キーで、障子の登場パターンを再生する.

		for(int i = (int)KeyCode.Alpha1;i <= (int)KeyCode.Alpha9;i++) {

			if(Input.GetKeyDown((KeyCode)i)) {

				FloorControl.CLOSING_PATTERN_TYPE	type = (FloorControl.CLOSING_PATTERN_TYPE)(i - (int)KeyCode.Alpha1);

				bool	is_flip = Input.GetKey(KeyCode.RightShift);

				this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);
				this.rooms[(this.start_model_index + 0)%MODEL_NUM].setClosingPatternType(type, is_flip);
				this.rooms[(this.start_model_index + 0)%MODEL_NUM].beginCloseShoji();
			}
		}
	#endif
	}

	public void	onRestart()
	{
		this.room_count = 0;
		this.is_closed = false;

		this.rooms[(this.start_model_index + 0)%MODEL_NUM].attachShouji(this.shoji_control);
		this.rooms[(this.start_model_index + 0)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 1)%MODEL_NUM].setOpen();
		this.rooms[(this.start_model_index + 2)%MODEL_NUM].setClose();
	}

	// ミスした後のリスタート位置を取得する.
	public Vector3	getRestartPosition()
	{
		Vector3	position;

		position = this.rooms[this.start_model_index].transform.position;

		position.z += RESTART_Z_OFFSET;

		return(position);
	}

	// 紙ののこり枚数を取得する.
	public int	getPaperNum()
	{
		return(this.shoji_control.getPaperNum());
	}
}
