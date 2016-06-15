using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// デバッグツール.
// ----------------------------------------------------------------------------
public class DebugTools : MonoBehaviour {
	
	private StageController stageController;
	
	void Start () {
		
		// ステージコントローラのインスタンスを取得.
		stageController =
			GameObject.Find ("StageController").GetComponent<StageController>();
	}
	
	void OnGUI () {
		
		// ステージセレクト.
		GUILayout.BeginArea(new Rect(0, 0, 50, 500));
		GUILayout.Label("STAGE: " + stageController.GetStage() );
		if ( GUILayout.Button("STAR") ) stageController.SetStage("START");
		if ( GUILayout.Button("PAT1") ) stageController.SetStage("PATROL1");
		if ( GUILayout.Button("ATT1") ) stageController.SetStage("ATTACK1");
		if ( GUILayout.Button("PAT2") ) stageController.SetStage("PATROL2");
		if ( GUILayout.Button("ATT2") ) stageController.SetStage("ATTACK2");
		if ( GUILayout.Button("PAT3") ) stageController.SetStage("PATROL3");
		if ( GUILayout.Button("ATT3") ) stageController.SetStage("ATTACK3");
		if ( GUILayout.Button("SILE") ) stageController.SetStage("SILENCE");
		if ( GUILayout.Button("BOSS") ) stageController.SetStage("BOSS");
		if ( GUILayout.Button("OVER") ) stageController.SetStage("GAMECLEAR");
		GUILayout.EndArea();
		
		// ゲームレベルセレクト.
		GUILayout.BeginArea(new Rect(55, 0, 105, 500));
		GUILayout.Label("LEVEL:"+stageController.GetLevelText());
		stageController.SetLevel(
			(int)GUILayout.HorizontalSlider (stageController.GetLevel(), 0, 3));
		GUILayout.EndArea();

	}
}
