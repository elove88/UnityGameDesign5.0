using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//生の入力情報を取得し、プレイヤーの挙動などを決定する
public class InputManager : MonoBehaviour {

	void Awake(){
		Application.targetFrameRate = 60;
	}

	// Use this for initialization
	void Start () {
		m_musicManager=GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_playerAction=GameObject.Find("PlayerAvator").GetComponent<PlayerAction>();
		m_scoringManager=GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
	}

	// Update is called once per frame
	void Update () {
		//ビートカウントの記録タイミングを、InputのUpdateのタイミングで行う。
		//MusicManagerのUpdateでビートカウントの記録を行うと入力とビートカウントが
		//最大1フレーム分ずれる
		//演奏中に画面クリックでプレイヤーのアクション
		if( Input.GetMouseButtonDown(0) && m_musicManager.IsPlaying() ){
			PlayerActionEnum actionType;
			if (m_scoringManager.temper < ScoringManager.temperThreshold){
				actionType=PlayerActionEnum.HeadBanging;
			}
			else{
				actionType
					=m_musicManager.currentSongInfo.onBeatActionSequence[
						m_scoringManager.GetNearestPlayerActionInfoIndex()
					].playerActionType;
			}
			m_playerAction.DoAction(actionType);
		}
	}

	//privaete variables
	MusicManager m_musicManager;
	PlayerAction m_playerAction;
	ScoringManager m_scoringManager;
}
