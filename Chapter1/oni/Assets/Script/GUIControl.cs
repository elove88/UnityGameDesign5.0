using UnityEngine;
using System.Collections;

public class GUIControl : MonoBehaviour {

	public SceneControl		scene_control = null;
	public ScoreControl		score_control = null;
	
	// 評価の文字.
	private float	gui_eval_scale = 1.0f;		// スケール.
	private float	gui_eval_alpha = 1.0f;		// アルファー.

	public static float	EVAL_ZOOM_TIME = 0.4f;
	
	// 『はじめ』の文字.
	public float	start_texture_x		= 0.0f;
	public float	start_texture_y		= 50.0f;
	
	// 評価の文字.
	public float	defeat_base_texture_x	=    0.0f;
	public float	defeat_base_texture_y	=   70.0f;
	public float	defeat_texture_x		=   70.0f;
	public float	defeat_texture_y		=   70.0f;
	public float	eval_base_texture_x		=    0.0f;
	public float	eval_base_texture_y		=  -40.0f;
	public float	eval_texture_x			=   70.0f;
	public float	eval_texture_y			=  -40.0f;
	public float	total_texture_x			=    0.0f;
	public float	total_texture_y			=    0.0f;
	
	public Texture	defeat_base_texture = null;
	public Texture	eval_base_texture = null;
	
	public Texture	result_excellent_texture = null;		// 『優』
	public Texture	result_good_texture = null;				// 『良』
	public Texture	result_nomal_texture = null;			// 『可』
	public Texture	result_bad_texture = null;				// 『不可』
	public Texture	result_mini_excellent_texture = null;	// 『優』
	public Texture	result_mini_good_texture = null;		// 『良』
	public Texture	result_mini_nomal_texture = null;		// 『可』
	public Texture	result_mini_bad_texture = null;			// 『不可』

	
	// 『戻る』の文字.
	public float	return_texture_x	= 0.0f;
	public float	return_texture_y	= -150.0f;	
	// -------------------------------------------------------------------------------- //

	void	Start()
	{
		this.scene_control = GetComponent<SceneControl>();
		this.score_control = GetComponent<ScoreControl>();
		
		this.score_control.setNumForce( this.scene_control.result.oni_defeat_num );
	}
	
	void	Update()
	{
		//if(this.scene_control.IsDrawScore()) {
			
			this.score_control.setNum( this.scene_control.result.oni_defeat_num );
		//}
	}

	void	OnGUI()
	{
		//スコア
		if(this.scene_control.IsDrawScore()) {
			
			this.score_control.draw();
		}
		
		// 『開始』の文字.
		//this.scene_control.drawTitle();
		
		// 『はじめ』の文字.
		if(this.scene_control.step == SceneControl.STEP.START) {

			TitleSceneControl.drawTexture(this.scene_control.StartTexture, start_texture_x,  start_texture_y, 1.0f, 1.0f, 0.0f, 1.0f);
		}

		// オニを切った数の評価の表示.
		if(this.scene_control.step == SceneControl.STEP.RESULT_DEFEAT) {
		
			TitleSceneControl.drawTexture(defeat_base_texture, defeat_base_texture_x, defeat_base_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
			TitleSceneControl.drawTexture(GetDefeatRankTexture(), defeat_texture_x,  defeat_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
		}
		
		// オニを切った数とオニを切ったタイミングの評価の表示.
		if(this.scene_control.step == SceneControl.STEP.RESULT_EVALUATION) {
		
			TitleSceneControl.drawTexture(defeat_base_texture, defeat_base_texture_x, defeat_base_texture_y, 1.0f, 1.0f, 0.0f, 1.0f);
			TitleSceneControl.drawTexture(GetDefeatRankTexture(), defeat_texture_x,  defeat_texture_y, 1.0f, 1.0f, 0.0f, 1.0f);
			TitleSceneControl.drawTexture(eval_base_texture, eval_base_texture_x, eval_base_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
			TitleSceneControl.drawTexture(GetEvalRankTexture(), eval_texture_x,	eval_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
		}
		
		// 最終評価の表示.
		if(this.scene_control.step >= SceneControl.STEP.RESULT_TOTAL) {
		
			TitleSceneControl.drawTexture(GetTotalRankTexture(), total_texture_x,	total_texture_y, this.gui_eval_scale, this.gui_eval_scale, 0.0f, this.gui_eval_alpha);
		}

		// 『戻る』の文字.
		if(this.scene_control.step >= SceneControl.STEP.GAME_OVER) {

			TitleSceneControl.drawTexture(this.scene_control.ReturnButtonTexture, return_texture_x, return_texture_y);
		}
		
		// ---------------------------------------------------------------- //
		// デバッグ用
#if fasle 
		SceneControl	scene = this.scene_control;

		GUI.color  = Color.white; 
		GUI.matrix = Matrix4x4.identity;

		float	x = 100;
		float	y = 100;

		float	dy = 16;

		GUI.Label(new Rect(x, y, 100, 100), scene.attack_time.ToString());
		y += dy;

		GUI.Label(new Rect(x, y, 100, 100), scene.evaluation.ToString());
		y += dy;

		if(this.scene_control.level_control.is_random) {

			GUI.Label(new Rect(x, y, 150, 100), "RANDOM(" + scene.level_control.group_type_next.ToString() + ")");

		} else {

			GUI.Label(new Rect(x, y, 150, 100), scene.level_control.group_type_next.ToString());
		}
		//this.scene_control.GetEvaluationTexture();
		y += dy;

		//GUI.Label(new Rect(x, y, 100, 100), this.game_timer.ToString());
		//y += 20;

		//

		SceneControl.IS_AUTO_ATTACK = GUI.Toggle(new Rect(x, y, 100, 20), SceneControl.IS_AUTO_ATTACK, "auto");
		y += 50;
		/*
		if(GUI.Toggle(new Rect(x, y, 100, 100), this.evaluation_auto_attack == EVALUATION.GREAT, "great")) {

			this.evaluation_auto_attack = EVALUATION.GREAT;
		}
		y += 20;

		if(GUI.Toggle(new Rect(x, y, 100, 100), this.evaluation_auto_attack == EVALUATION.GOOD, "good")) {

			this.evaluation_auto_attack = EVALUATION.GOOD;
		}
		y += 20;*/

		scene.evaluation_auto_attack = (SceneControl.EVALUATION)GUI.Toolbar(new Rect(x, y, 200, 20), (int)scene.evaluation_auto_attack, SceneControl.evaluation_str);
		y += dy;

		// リザルト.

		x = 300;
		y = 100;

		GUI.Label(new Rect(x, y, 100, 100), scene.result.oni_defeat_num.ToString());
		y += dy;

		for(int i = 0;i < (int)SceneControl.EVALUATION.NUM;i++) {

			GUI.Label(new Rect(x, y, 100, 100), ((SceneControl.EVALUATION)i).ToString() + " " + scene.result.eval_count[i].ToString());
			y += dy;
		}

		GUI.Label(new Rect(x, y, 100, 100), "rank " + scene.result.rank.ToString());
		y += dy;

		if(0 <= (int)scene.evaluation_auto_attack && (int)scene.evaluation_auto_attack <= 2) {

			scene.result.rank = (int)scene.evaluation_auto_attack;
		}
#endif

	}
	
	// 切った数の評価のテクスチャーを取得.
	private Texture GetDefeatRankTexture()
	{
		return GetResultMiniTexture( this.scene_control.result_control.getDefeatRank() );
	}
	
	// 切り評価のテクスチャーを取得.
	private Texture GetEvalRankTexture()
	{
		return GetResultMiniTexture( this.scene_control.result_control.getEvaluationRank() );
	}
	
	private Texture GetResultMiniTexture( int idx )
	{
		Texture[]	texList = { result_mini_bad_texture, result_mini_nomal_texture, result_mini_good_texture, result_mini_excellent_texture };
		return texList[idx];
	}
	
	// トータルスコアのテクスチャーを取得.
	public Texture GetTotalRankTexture()
	{
		Texture[]	texList = { result_bad_texture, result_nomal_texture, result_good_texture, result_excellent_texture };
		
		return texList[this.scene_control.result_control.getTotalRank()];
	}
	
	// 評価の文字のアニメーション.
	public void	updateEval(float time)
	{
		float	zoom_in_time = GUIControl.EVAL_ZOOM_TIME;
		float	rate;

		if(time < zoom_in_time) {

			rate = time/zoom_in_time;
			rate = Mathf.Pow(rate, 2.5f);
			this.gui_eval_scale = Mathf.Lerp(1.5f, 1.0f, rate);

		} else {

			this.gui_eval_scale = 1.0f;
		}

		if(time < zoom_in_time) {

			rate = time/zoom_in_time;
			rate = Mathf.Pow(rate, 2.5f);
			this.gui_eval_alpha = Mathf.Lerp(0.0f, 1.0f, rate);

		} else {

			this.gui_eval_alpha = 1.0f;
		}
	}

}
