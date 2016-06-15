using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using System.Collections;
//シーク機能付き開発モードのGUI挙動
public class ShowResultGUI : MonoBehaviour
{
	public string title = "title";
	public string scoreLabel = "Score : ";
	public string commentLabel = "Comment : ";
	public string comment_EXCELLENT = "comment shown here";
	public string comment_GOOD = "comment shown here";
	public string comment_BAD = "comment shown here";
	public string comment = "comment shown here";
	public GUISkin guiStyle;
	// Use this for initialization
	void Start()
	{
		m_scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
	}

	// Update is called once per frame
	void Update()
	{

	}
	void OnGUI()
	{
		GUI.skin = guiStyle;
		GUI.Box(new Rect(10.0f, 10.0f, Screen.width - 20.0f, Screen.height - 20.0f), title);
		GUI.Label( new Rect(20, 100, 200, 40), scoreLabel );
		GUI.Label( new Rect(20, 140, 200, 40), m_scoringManager.score.ToString() );
		GUI.Label( new Rect(20, 180, 200, 40), commentLabel );
		GUILayout.BeginArea(new Rect(20.0f, 220.0f, Screen.width - 20.0f, Screen.height - 40.0f));
		GUILayout.Label(comment);
		GUILayout.EndArea();
		if( GUI.Button( new Rect( (Screen.width - 150)/2.0f, 360, 150, 40 ), "Return to Menu" ) ){
			GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("Restart");
		}
	}
	ScoringManager m_scoringManager;
}
