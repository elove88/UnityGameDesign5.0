using UnityEngine;
using System.Collections;

public class ShojiControl : MonoBehaviour {

	public struct HoleIndex {

		public int	x;
		public int	y;

	};

	// 初期位置X,Y.
	public float init_x = 0.0f;
	public float init_y = 2.0f;
	public float init_z = 5.0f;
	
	// 床の幅（Z方向）.
	public static float WIDTH = 15.0f;
	
	// プレイヤーとの距離 (DIST < WIDTH).
	public float DIST = 10.0f;
	
	// 自オブジェクト移動距離.
	public static float MOVE = 0.1f;
	
	// 自オブジェクトニュートラルポジション.
	public Vector3 neutral_position;

	public GameObject paperPrefab;

	public SyoujiPaperControl[,] papers;

	public static float	TILE_SIZE_X = 0.85f;
	public static float	TILE_SIZE_Y = 0.94f;
	public static float	TILE_ORIGIN_X = -0.85f;
	public static float	TILE_ORIGIN_Y =  1.92f;

	public static int	TILE_NUM_X = 3;
	public static int	TILE_NUM_Y = 3;

	public int	paper_num = TILE_NUM_X*TILE_NUM_Y;		// 紙の残り枚数.

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {
	
		// 自オブジェクトニュートラルポジション.
		this.neutral_position = this.transform.position;

		this.papers = new SyoujiPaperControl[TILE_NUM_X, TILE_NUM_Y];

		for(int x = 0;x < TILE_NUM_X;x++) {

			for(int y = 0;y < TILE_NUM_Y;y++) {
				
				GameObject	go = Instantiate(this.paperPrefab) as GameObject;

				go.transform.parent = this.transform;

				//

				Vector3	position = go.transform.localPosition;

				position.x = TILE_ORIGIN_X + x*TILE_SIZE_X;
				position.y = TILE_ORIGIN_Y + y*TILE_SIZE_Y;
				position.z = 0.0f;

				go.transform.localPosition = position;

				//

				SyoujiPaperControl	paper_control = go.GetComponent<SyoujiPaperControl>();

				paper_control.shoji_control = this;
				paper_control.hole_index.x = x;
				paper_control.hole_index.y = y;

				this.papers[x, y] = paper_control;
			}
		}

		//

		this.paper_num = this.papers.Length;
	}
	
	// Update is called once per frame
	void Update () {

	}

	// 紙が破れたときの処理.
	public void	onPaperBreak()
	{
		this.paper_num--;

		this.paper_num = Mathf.Max(0, this.paper_num);

		// 空振りカウンター（破れた後に通過した回数）をリセットする.
		//
		// 1.紙Aをやぶる
		// 2.紙Aを通過（紙Aの空振りカウンターが増加）
		// 3.紙Bをやぶる
		// 4.紙Bを通過　←このタイミングで紙Aが鉄板化してしまうことがあるので.

		for(int x = 0;x < TILE_NUM_X;x++) {

			for(int y = 0;y < TILE_NUM_Y;y++) {

				this.papers[x, y].resetThroughCount();
			}
		}
	}

	// 紙の残り枚数を取得する.
	public int		getPaperNum()
	{
		return(this.paper_num);
	}

	// 『格子の穴』インデックスが有効？.
	public bool	isValidHoleIndex(HoleIndex hole_index)
	{
		bool	ret = false;

		do {

			ret = false;

			if(hole_index.x < 0 || TILE_NUM_X <= hole_index.x) {

				break;
			}
			if(hole_index.y < 0 || TILE_NUM_Y <= hole_index.y) {

				break;
			}
			
			ret = true;

		} while(false);

		return(ret);
	}

	// いちばん近い『格子の穴』を取得する.
	public HoleIndex	getClosetHole(Vector3 position)
	{
		HoleIndex hole_index;

		position = this.transform.InverseTransformPoint(position);

		hole_index.x = Mathf.RoundToInt((position.x - TILE_ORIGIN_X)/TILE_SIZE_X);
		hole_index.y = Mathf.RoundToInt((position.y - TILE_ORIGIN_Y)/TILE_SIZE_Y);

		return(hole_index);
	}

	// 『格子の穴』の位置座標を取得する.
	public Vector3	getHoleWorldPosition(int hole_pos_x, int hole_pos_y)
	{
		Vector3	position;

		position.x = (float)hole_pos_x*TILE_SIZE_X + TILE_ORIGIN_X;
		position.y = (float)hole_pos_y*TILE_SIZE_Y + TILE_ORIGIN_Y;
		position.z = 0.0f;

		position = this.transform.TransformPoint(position);

		return(position);
	}

}
