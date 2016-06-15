using UnityEngine;
using System.Collections;

public class PazzleControl : MonoBehaviour {

	public GameControl		game_control = null;

	private int		piece_num;				// ピースの数（全部）.
	private int		piece_finished_num;		// 完成したピースの数.

	enum STEP {

		NONE = -1,

		PLAY = 0,	// パズル解き中.
		CLEAR,		// クリアー演出中.

		NUM,
	};

	private STEP	step      = STEP.NONE;
	private STEP	next_step = STEP.NONE;

	private float		step_timer = 0.0f;
	private float		step_timer_prev = 0.0f;

	// -------------------------------------------------------- //

	// 全てのピース.
	private PieceControl[]	all_pieces;

	// 考え中のピース。手前に表示される順に並んでいる.
	private PieceControl[]	active_pieces;

	// ピースをシャッフルして配置する場所（範囲）.
	private Bounds	shuffle_zone;

	// [degree] パズル全体を回転する角度.
	private float	pazzle_rotation = 37.0f;

	// ピースをシャッフルして配置する。グリッドのマス目の数（１辺）.
	private int		shuffle_grid_num = 1;

	// 『できた！』を表示する？.
	private bool	is_disp_cleared = false;

	// -------------------------------------------------------- //

	void	Start()
	{

		this.game_control =  GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();

		// ピースのオブジェクトの数を数える.

		this.piece_num = 0;

		for(int i = 0;i < this.transform.childCount;i++) {

			GameObject	piece = this.transform.GetChild(i).gameObject;

			if(!this.is_piece_object(piece)) {

				continue;
			}

			this.piece_num++;
		}

		//

		this.all_pieces    = new PieceControl[this.piece_num];
		this.active_pieces = new PieceControl[this.piece_num];

		// 各ピースに PieceControl コンポーネント（スクリプト "PieceControl.cs"）を
		// 追加する.

		for(int i = 0, n = 0;i < this.transform.childCount;i++) {

			GameObject	piece = this.transform.GetChild(i).gameObject;

			if(!this.is_piece_object(piece)) {

				continue;
			}

			piece.AddComponent<PieceControl>();

			piece.GetComponent<PieceControl>().pazzle_control = this;

			//

			this.all_pieces[n++] = piece.GetComponent<PieceControl>();
		}

		this.piece_finished_num = 0;

		// ピースの描画順を設定する.
		//
		this.set_height_offset_to_pieces();

		// ベースの描画順を設定する.
		//
		for(int i = 0;i < this.transform.childCount;i++) {

			GameObject	game_object = this.transform.GetChild(i).gameObject;

			if(this.is_piece_object(game_object)) {

				continue;
			}

			game_object.GetComponent<Renderer>().material.renderQueue = this.GetDrawPriorityBase();
		}

		// ピースをシャッフルして配置する場所（範囲）を求める.
		//
		this.calc_shuffle_zone();


		this.is_disp_cleared = false;
	}

	void	Update()
	{
		// ---------------------------------------------------------------- //

		this.step_timer_prev = this.step_timer;

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 状態遷移チェック.

		switch(this.step) {

			case STEP.NONE:
			{
				this.next_step = STEP.PLAY;
			}
			break;

			case STEP.PLAY:
			{
				// ピースがすべて正解の位置に置かれたら、クリアー.
				if(this.piece_finished_num >= this.piece_num) {
		
					this.next_step = STEP.CLEAR;
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 遷移時の初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.PLAY:
				{
					// this.active_pieces = this.all_pieces としてしまうと配列への参照をコピー
					// してしまうので、中身をいっこづつコピーする.
					for(int i = 0;i < this.all_pieces.Length;i++) {

						this.active_pieces[i] = this.all_pieces[i];
					}

					this.piece_finished_num = 0;

					this.shuffle_pieces();

					foreach(PieceControl piece in this.active_pieces) {

						piece.Restart();
					}

					// ピースの描画順を設定する.
					//
					this.set_height_offset_to_pieces();
				}
				break;

				case STEP.CLEAR:
				{
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 実行処理.

		switch(this.step) {

			case STEP.CLEAR:
			{
				// 完成時のジングル.

				const float	play_se_delay = 0.40f;

				if(this.step_timer_prev < play_se_delay && play_se_delay <= this.step_timer) {

					this.game_control.playSe(GameControl.SE.COMPLETE);
					this.is_disp_cleared = true;
				}
			}
			break;
		}

		PazzleControl.DrawBounds(this.shuffle_zone);
	}

	// 『やりなおし』ボタンが押されたとき.
	public void	beginRetryAction()
	{
		this.next_step = STEP.PLAY;
	}

	// ピースがドラッグ開始されたときの処理.
	public void		PickPiece(PieceControl piece)
	{
		int	i, j;

		// クリックされたピースを、配列の先頭に移動させる.
		//
		// this.pieces[] は表示される順に並んでいるので、先頭に持ってくると
		// 一番上に表示されるようになる.

		for(i = 0;i < this.active_pieces.Length;i++) {

			if(this.active_pieces[i] == null) {

				continue;
			}

			if(this.active_pieces[i].name == piece.name) {

				// 『クリックされたピース』より前にあるピースを、いっこづつ後ろにずらす.
				//
				for(j = i;j > 0;j--) {

					this.active_pieces[j] = this.active_pieces[j - 1];
				}

				// クリックされたピースを先頭にもってくる.
				this.active_pieces[0] = piece;

				break;
			}
		}

		this.set_height_offset_to_pieces();
	}

	// ピースが正解の位置におかれたときの処理.
	public void		FinishPiece(PieceControl piece)
	{
		int	i, j;

		piece.GetComponent<Renderer>().material.renderQueue = this.GetDrawPriorityFinishedPiece();

		// クリックされたピースを配列から取り除く

		for(i = 0;i < this.active_pieces.Length;i++) {

			if(this.active_pieces[i] == null) {

				continue;
			}

			if(this.active_pieces[i].name == piece.name) {

				// 『クリックされたピース』より後ろにあるピースを、いっこづつ前にずらす.
				//
				for(j = i;j < this.active_pieces.Length - 1;j++) {

					this.active_pieces[j] = this.active_pieces[j + 1];
				}

				// 配列の末尾を空にする.
				this.active_pieces[this.active_pieces.Length - 1] = null;

				// 『正解済みのピース』の数を＋１する.
				this.piece_finished_num++;

				break;
			}
		}

		this.set_height_offset_to_pieces();
	}

	// ---------------------------------------------------------------------------------------- //

	private static float	SHUFFLE_ZONE_OFFSET_X = -5.0f;
	private static float	SHUFFLE_ZONE_OFFSET_Y =  1.0f;
	private static float	SHUFFLE_ZONE_SCALE =  1.1f;

	// ピースをシャッフルして配置する場所（範囲）を求める.
	private void	calc_shuffle_zone()
	{
		Vector3		center;

		// パピースをばらまく範囲の中心.

		center = Vector3.zero;

		foreach(PieceControl piece in this.all_pieces) {

			center += piece.finished_position;
		}
		center /= (float)this.all_pieces.Length;

		center.x += SHUFFLE_ZONE_OFFSET_X;
		center.z += SHUFFLE_ZONE_OFFSET_Y;

		// ピースを配置するグリッドのマス目の数.

		this.shuffle_grid_num = Mathf.CeilToInt(Mathf.Sqrt((float)this.all_pieces.Length));

		// ピースのバウンディングボックスの中で最大のもの＝１マスの大きさ.

		Bounds	piece_bounds_max = new Bounds(Vector3.zero, Vector3.zero);

		foreach(PieceControl piece in this.all_pieces) {

			Bounds bounds = piece.GetBounds(Vector3.zero);

			piece_bounds_max.Encapsulate(bounds);
		}

		piece_bounds_max.size *= SHUFFLE_ZONE_SCALE;

		this.shuffle_zone = new Bounds(center, piece_bounds_max.size*this.shuffle_grid_num);
	}

	// ピースの位置をシャッフルする.
	private void	shuffle_pieces()
	{
	#if true
		// ピースをグリッドに順番に並べる.

		int[]		piece_index = new int[this.shuffle_grid_num*this.shuffle_grid_num];

		for(int i = 0;i < piece_index.Length;i++) {

			if(i < this.all_pieces.Length) {

				piece_index[i] = i;

			} else {

				piece_index[i] = -1;
			}
		}

		// ピース二つをランダムに選んで、位置を交換する.

		for(int i = 0;i < piece_index.Length - 1;i++) {

			int	j = Random.Range(i + 1, piece_index.Length);

			int	temp = piece_index[j];

			piece_index[j] = piece_index[i];

			piece_index[i] = temp;
		}
	
		// 場所のインデックスから実際の座標に変換して配置する.

		Vector3	pitch;

		pitch = this.shuffle_zone.size/(float)this.shuffle_grid_num;

		for(int i = 0;i < piece_index.Length;i++) {

			if(piece_index[i] < 0) {

				continue;
			}

			PieceControl	piece = this.all_pieces[piece_index[i]];

			Vector3	position = piece.finished_position;

			int		ix = i%this.shuffle_grid_num;
			int		iz = i/this.shuffle_grid_num;

			position.x = ix*pitch.x;
			position.z = iz*pitch.z;

			position.x += this.shuffle_zone.center.x - pitch.x*(this.shuffle_grid_num/2.0f - 0.5f);
			position.z += this.shuffle_zone.center.z - pitch.z*(this.shuffle_grid_num/2.0f - 0.5f);

			position.y = piece.finished_position.y;

			piece.start_position = position;
		}

		// 少しづつ（グリッドのマス目内で）ランダムに位置をずらす.

		Vector3		offset_cycle = pitch/2.0f;
		Vector3		offset_add   = pitch/5.0f;

		Vector3		offset = Vector3.zero;

		for(int i = 0;i < piece_index.Length;i++) {

			if(piece_index[i] < 0) {

				continue;
			}

			PieceControl	piece = this.all_pieces[piece_index[i]];

			Vector3	position = piece.start_position;

			position.x += offset.x;
			position.z += offset.z;

			piece.start_position = position;

			//


			offset.x += offset_add.x;

			if(offset.x > offset_cycle.x/2.0f) {

				offset.x -= offset_cycle.x;
			}

			offset.z += offset_add.z;

			if(offset.z > offset_cycle.z/2.0f) {

				offset.z -= offset_cycle.z;
			}
		}

		// 全体を回転させる.

		foreach(PieceControl piece in this.all_pieces) {

			Vector3		position = piece.start_position;

			position -= this.shuffle_zone.center;

			position = Quaternion.AngleAxis(this.pazzle_rotation, Vector3.up)*position;

			position += this.shuffle_zone.center;

			piece.start_position = position;
		}

		this.pazzle_rotation += 90;

	#else
		// 単純に乱数で座標を決める場合.
		foreach(PieceControl piece in this.all_pieces) {

			Vector3	position;

			Bounds	piece_bounds = piece.GetBounds(Vector3.zero);

			position.x = Random.Range(this.shuffle_zone.min.x - piece_bounds.min.x, this.shuffle_zone.max.x - piece_bounds.max.x);
			position.z = Random.Range(this.shuffle_zone.min.z - piece_bounds.min.z, this.shuffle_zone.max.z - piece_bounds.max.z);

			position.y = piece.finished_position.y;

			piece.start_position = position;
		}
	#endif
	}

	// ピースの GameObject？.
	private bool is_piece_object(GameObject game_object)
	{
		bool	is_piece = false;

		do {

			// 名前に "base" が含まれるものは、台座のオブジェクト.
			if(game_object.name.Contains("base")) {

				continue;
			}

			//

			is_piece = true;

		} while(false);

		return(is_piece);
	}


	// ---------------------------------------------------------------------------------------- //

	// 全てのピースに、高さオフセットをセットする.
	private void	set_height_offset_to_pieces()
	{
		float	offset = 0.01f;

		int		n = 0;

		foreach(PieceControl piece in this.active_pieces) {

			if(piece == null) {

				continue;
			}

			// 描画順を指定する.
			// pieces の前にあるピース＝上にあるピースほど描画順があと（見える）になるように.
			//
			piece.GetComponent<Renderer>().material.renderQueue = this.GetDrawPriorityPiece(n);

			// クリックしたときに一番上にあるピースの OnMouseDown() が呼ばれるようにするために、
			// Y高さオフセットも指定しておく.
			// （描画プライオリティのみだと下にあるピースがクリックされてしまうことがある）.

			offset -= 0.01f/this.piece_num;

			piece.SetHeightOffset(offset);

			//

			n++;
		}
	}

	// 描画プライオリティを取得する（台座）.
	private int	GetDrawPriorityBase()
	{
		return(0);
	}

	// 描画プライオリティを取得する（正解の位置に置かれたピース）.
	private int	GetDrawPriorityFinishedPiece()
	{
		int	priority;

		priority = this.GetDrawPriorityBase() + 1;

		return(priority);
	}

	// 描画プライオリティを取得する（『やりなおし』ボタン）.
	public int	GetDrawPriorityRetryButton()
	{
		int	priority;

		priority = this.GetDrawPriorityFinishedPiece() + 1;

		return(priority);
	}

	// 描画プライオリティを取得する（正解の位置に置かれたピース）.
	private int	GetDrawPriorityPiece(int priority_in_pieces)
	{
		int	priority;

		// 『やりなおす』ボタンの分があるので、ひとつ余計にプラスする.
		priority = this.GetDrawPriorityRetryButton() + 1;

		// priority_in_pieces は０番が一番上で、renderQueueは値が小さい方が先に描画される.
		priority += this.piece_num - 1 - priority_in_pieces;

		return(priority);
	}

	// ---------------------------------------------------------------------------------------- //

	// パズルが完成した？.
	public bool	isCleared()
	{
		return(this.step == STEP.CLEAR);
	}

	// 『できた！』を表示する？.
	public bool	isDispCleard()
	{
		return(this.is_disp_cleared);
	}

	// ---------------------------------------------------------------------------------------- //

	public static void	DrawBounds(Bounds bounds)
	{
		Vector3[]	square = new Vector3[4];

		square[0] = new Vector3(bounds.min.x, 0.0f, bounds.min.z);
		square[1] = new Vector3(bounds.max.x, 0.0f, bounds.min.z);
		square[2] = new Vector3(bounds.max.x, 0.0f, bounds.max.z);
		square[3] = new Vector3(bounds.min.x, 0.0f, bounds.max.z);

		for(int i = 0;i < 4;i++) {

			Debug.DrawLine(square[i], square[(i + 1)%4], Color.white, 0.0f, false);
		}

	}
}
