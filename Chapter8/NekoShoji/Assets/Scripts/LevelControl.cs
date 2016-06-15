using UnityEngine;
using System.Collections;

public class LevelControl : MonoBehaviour {

	public enum LEVEL {

		NONE = -1,

		EASY = 0,
		NORMAL = 1,
		HARD = 2,

		NUM,
	};

	public LEVEL	level = LEVEL.EASY;

	public SceneControl	scene_control = null;

	private bool	random_bool_prev;

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		switch(GlobalParam.GetInstance().difficulty()) {

			case 0:
			{	
				this.level = LEVEL.EASY;
			}
			break;
		
			case 1:
			{	
				this.level = LEVEL.NORMAL;
			}
			break;
			
			case 2:
			{	
				this.level = LEVEL.HARD;
			}
			break;

			default:
			{	
				this.level = LEVEL.EASY;
			}
			break;
		}

		this.scene_control = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SceneControl>();

		this.random_bool_prev = Random.Range(0, 2) == 0 ? true : false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// 障子が閉まり始める距離.
	public float getCloseDistance()
	{
		// ちいさな値にすると障子が閉まり始めるタイミングが遅くなって、難しくなります.
		
		float	close_distance = 14.0f;
		
		int		paper_num = this.scene_control.getPaperNum();

		switch(this.level) {
		
			case LEVEL.EASY:
			{
				close_distance = 14.0f;
			}
			break;
			
			case LEVEL.NORMAL:
			{
				close_distance = 14.0f;
			}
			break;
			
			case LEVEL.HARD:
			{
				if(paper_num >= 8) {

					close_distance = 12.0f;

				} else if(paper_num >= 5) {

					close_distance = 12.0f;

				} else {

					close_distance = FloorControl.SHUTTER_POSITION_Z;
				}
			}
			break;

			default:
			{
				close_distance = 14.0f;
			}
			break;
		}


		return(close_distance);
	}
	
	// 障子の閉まり方パターンを取得する.
	public void	getClosingPattern(out FloorControl.CLOSING_PATTERN_TYPE out_type, out bool out_is_flip, out FloorControl.ClosingPatternParam out_param)
	{
		int		paper_num   = this.scene_control.getPaperNum();
		bool	random_bool = Random.Range(0, 2) == 0 ? true : false;

		out_param.as_float = 0.0f;
		out_param.as_bool  = false;

		switch(this.level) {

			case LEVEL.EASY:
			{
				// easy は normal のみ.

				out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

				out_is_flip = false;
			}
			break;

			case LEVEL.NORMAL:
			{
				StaticArray<FloorControl.CLOSING_PATTERN_TYPE>	out_type_cadidates = new StaticArray<FloorControl.CLOSING_PATTERN_TYPE>((int)FloorControl.CLOSING_PATTERN_TYPE.NUM);

				if(9 >= paper_num && paper_num >= 8) {

					// １、２枚目は NORMAL.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.NORMAL);

					out_is_flip = false;

				} else if(paper_num == 7) {

					// 残り７枚のときは OVERSHOOT.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.OVERSHOOT);

					out_is_flip = false;

				} else if(paper_num == 6) {

					// 残り６枚のときは SECONDTIME.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.SECONDTIME);

					out_is_flip = false;

				} else if(paper_num == 5) {

					// 残り５枚のときは ARCODION.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.ARCODION);

					out_is_flip = false;

				} else if(4 >= paper_num && paper_num >= 3) {

					// 残り４～３枚のときは DELAY（is_flip はランダム）.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.DELAY);

					out_is_flip = random_bool;

					if(paper_num == 4) {

						// 残り４枚のときは右から.

						out_param.as_bool = false;

					} else {

						// 残り３枚のときは右から（ふすまの裏から）.
						out_param.as_bool = true;
					}

				} else if(paper_num == 2) {

					// 残り２枚の時は必ず FALLDOWN.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.FALLDOWN);

					out_is_flip = false;

				} else {

					// 残り１枚の時は必ず FLIP（is_flip はランダム）.

					out_type_cadidates.push_back(FloorControl.CLOSING_PATTERN_TYPE.FLIP);

					out_is_flip = random_bool;
				}

				out_type = out_type_cadidates[Random.Range(0, out_type_cadidates.size())];
			}
			break;

			case LEVEL.HARD:
			{
				if(paper_num >= 8) {

					// 残り９～８枚のときは NORMAL.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

					if(paper_num == 9) {

						out_is_flip = random_bool;

					} else {

						out_is_flip = !this.random_bool_prev;
					}

				} else if(paper_num >= 5) {

					// 残り７～５枚のときは SLOW.
					// 閉まり終わるのがだんだん遅くなる.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.SLOW;

					if(paper_num == 7) {

						out_is_flip        = random_bool;
						out_param.as_float = 1.5f;

					} else if(paper_num == 6) {

						out_is_flip        = !this.random_bool_prev;
						out_param.as_float = 1.7f;

						// 次回のために上書きしておく.
						// （7, 6, 5 で必ず交互になるように）.
						random_bool = !this.random_bool_prev;

					} else {

						out_is_flip        = !this.random_bool_prev;
						out_param.as_float = 2.0f;
					}

				} else {

					// 残り４～１枚のときは SUPER_DELAY.

					out_type = FloorControl.CLOSING_PATTERN_TYPE.SUPER_DELAY;

					out_is_flip = random_bool;

					if(paper_num >= 4) {

						out_param.as_float = 0.6f;

					} else if(paper_num >= 3) {

						out_param.as_float = 0.7f;

					} else {

						out_param.as_float = 0.9f;
					}
				}
			}
			break;

			default:
			{
				out_type = FloorControl.CLOSING_PATTERN_TYPE.NORMAL;

				out_is_flip = false;
			}
			break;
		}

		this.random_bool_prev = random_bool;
	}

	// 『何回空振り（やぶった後の穴を通過）したら鉄板になるか』を取得する.
	public int	getChangeSteelCount()
	{
		// -1 のときは鉄板にならない.
		int	count = -1;

		int	paper_num = this.scene_control.getPaperNum();

		switch(this.level) {
		
			case LEVEL.EASY:
			{
				// easy は鉄板化なし.
				count = -1;
			}
			break;

			case LEVEL.NORMAL:
			{
				// hardは鉄板化なし.
				count = -1;
			}
			break;

			case LEVEL.HARD:
			{
				// （仮）です.
				// のこり枚数が少なくなるほど鉄板化しやすくなるようにしてください.

				if(paper_num >= 8) {
				
					count = -1;
				
				} else if(paper_num >= 6) {

					count = 5;

				} else if(paper_num >= 3) {

					count = 2;

				} else { 

					count = 1;
				}

			}
			break;

			default:
			{
				count = -1;
			}
			break;
		}

		return(count);
	}
}
