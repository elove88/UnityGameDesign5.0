using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
//ゲームのフェーズの遷移を管理するクラス
public class PhaseManager : MonoBehaviour {
	public string currentPhase{
		get{ return m_currentPhase; }
	}
	public GameObject[] guiList;
	// Use this for initialization
	void Start () {
		m_musicManager   = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
	}
	
	// Update is called once per frame
	void Update () {
		switch (currentPhase){
		case "Play" :
			if( m_musicManager.IsFinished() ){
				SetPhase("GameOver");
			}
			break;
		}
	}
	public void SetPhase(string nextPhase){
		switch(nextPhase){
		//スタートメニュー
		case "Startup":
			DeactiveateAllGUI();
			ActivateGUI("StartupMenuGUI");
			break;
		//説明
		case "OnBeginInstruction":
			DeactiveateAllGUI();
			ActivateGUI("InstructionGUI");
			ActivateGUI("OnPlayGUI");
			break;
		//メインゲーム
		case "Play":
		{
			DeactiveateAllGUI();
			ActivateGUI("OnPlayGUI");
			//csvから曲データ読み込み
			TextReader textReader
				= new StringReader(
					System.Text.Encoding.UTF8.GetString((Resources.Load("SongInfo/songInfoCSV") as TextAsset).bytes )
				);
			SongInfo songInfo = new SongInfo();
			SongInfoLoader loader=new SongInfoLoader();
			loader.songInfo=songInfo;
			loader.ReadCSV(textReader);
			m_musicManager.currentSongInfo = songInfo;

			foreach (GameObject audience in GameObject.FindGameObjectsWithTag("Audience"))
			{
				audience.GetComponent<SimpleActionMotor>().isWaveBegin = true;
			}
			//イベント(ステージ演出等)開始
			GameObject.Find("EventManager").GetComponent<EventManager>().BeginEventSequence();
			//スコア評価開始
			m_scoringManager.BeginScoringSequence();
			//リズムシーケンス描画開始
			OnPlayGUI onPlayGUI = GameObject.Find("OnPlayGUI").GetComponent<OnPlayGUI>();
			onPlayGUI.BeginVisualization();
			onPlayGUI.isDevelopmentMode = false;
			//演奏開始
			m_musicManager.PlayMusicFromStart();
		}
			break;
		case "DevelopmentMode":
		{
			DeactiveateAllGUI();
			ActivateGUI("DevelopmentModeGUI");
			ActivateGUI("OnPlayGUI");
			//csvから曲データ読み込み
			TextReader textReader
				= new StringReader(
					System.Text.Encoding.UTF8.GetString((Resources.Load("SongInfo/songInfoCSV") as TextAsset).bytes )
				);
			SongInfo songInfo = new SongInfo();
			SongInfoLoader loader=new SongInfoLoader();
			loader.songInfo=songInfo;
			loader.ReadCSV(textReader);
			m_musicManager.currentSongInfo = songInfo;

			foreach (GameObject audience in GameObject.FindGameObjectsWithTag("Audience"))
			{
				audience.GetComponent<SimpleActionMotor>().isWaveBegin = true;
			}
			//イベント(ステージ演出等)開始
			GameObject.Find("EventManager").GetComponent<EventManager>().BeginEventSequence();
			//スコア評価開始
			m_scoringManager.BeginScoringSequence();
			//リズムシーケンス描画開始
			OnPlayGUI onPlayGUI = GameObject.Find("OnPlayGUI").GetComponent<OnPlayGUI>();
			onPlayGUI.BeginVisualization();
			onPlayGUI.isDevelopmentMode = true;
			//developモード専用GUIシーケンス描画開始
			GameObject.Find("DevelopmentModeGUI").GetComponent<DevelopmentModeGUI>().BeginVisualization();
			//演奏開始
			m_musicManager.PlayMusicFromStart();
		}
			break;
		case "GameOver":
		{
			DeactiveateAllGUI();
			ActivateGUI("ShowResultGUI");
			ShowResultGUI showResult = GameObject.Find("ShowResultGUI").GetComponent<ShowResultGUI>();
			//スコア依存のメッセージを表示
			Debug.Log( m_scoringManager.scoreRate );
			Debug.Log(ScoringManager.failureScoreRate);
			if (m_scoringManager.scoreRate <= ScoringManager.failureScoreRate)
			{
				showResult.comment = showResult.comment_BAD;
				GameObject.Find("Vocalist").GetComponent<BandMember>().BadFeedback();
				
			}
			else if (m_scoringManager.scoreRate >= ScoringManager.excellentScoreRate)
			{
				showResult.comment = showResult.comment_EXCELLENT;
				GameObject.Find("Vocalist").GetComponent<BandMember>().GoodFeedback();
				GameObject.Find("AudienceVoice").GetComponent<AudioSource>().Play();
			}
			else
			{
				showResult.comment = showResult.comment_GOOD;
				GameObject.Find("Vocalist").GetComponent<BandMember>().GoodFeedback();
			}
		}
			break;
		case "Restart":
		{
			Application.LoadLevel("Main");
		}
			break;
		default:
			Debug.LogError("unknown phase: " + nextPhase);
			break;
		}//end of switch

		m_currentPhase = nextPhase;
	}
	private void DeactiveateAllGUI(){
		foreach( GameObject gui in guiList ){
			gui.SetActive(false);
		}
	}
	private void ActivateGUI(string guiName)
	{
		foreach (GameObject gui in guiList)
		{
			if (gui.name == guiName) gui.SetActive(true);
		}
	}
	//private Variables
	MusicManager m_musicManager;
	ScoringManager m_scoringManager;
	string m_currentPhase = "Startup";
}
