using UnityEngine;
using System.Collections;
//シーク機能付き開発モードのGUI挙動
public class DevelopmentModeGUI : MonoBehaviour {
	//演奏開始時の処理
	public void BeginVisualization()
	{
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_actionInfoRegionSeeker.SetSequence(m_musicManager.currentSongInfo.onBeatActionRegionSequence);
		m_actionInfoRegionSeeker.Seek(0);

	}
	public void Seek(float beatCount)
	{
		m_actionInfoRegionSeeker.Seek(beatCount);
	}
	// Use this for initialization
	void Start () {
		m_musicManager=GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager=GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
		m_eventManager=GameObject.Find("EventManager").GetComponent<EventManager>();
		//GUIオブジェクトはInactiveな可能性があるので、Findで直接アクセスできない。
		m_onPlayGUI = GameObject.Find("PhaseManager").GetComponent<PhaseManager>().guiList[1].GetComponent<OnPlayGUI>();
		m_playerAction=GameObject.Find("PlayerAvator").GetComponent<PlayerAction>();
		m_seekSlider.is_now_dragging    = false;
		m_seekSlider.dragging_poisition = 0.0f;
	}

	// Update is called once per frame
	void Update () {
		m_actionInfoRegionSeeker.ProceedTime(
			m_musicManager.beatCountFromStart - m_musicManager.previousBeatCountFromStart
		);

		m_seekSlider.is_button_down = Input.GetMouseButton(0);
	}

	void OnGUI(){

		GUI.Label(new Rect( 5, 100, 150, 40 ),"Current");

		// シークスライダーの制御.
		SeekSliderControl();

		GUI.TextArea(
			new Rect( 250, 100, 200, 40 ),
			((int)m_musicManager.beatCountFromStart).ToString() + "/" + ((int)m_musicManager.length).ToString()
		);

		// シーク中だけ、シークバー上の位置を表示する.
		if(this.m_seekSlider.is_now_dragging) {

			GUI.Label(new Rect( 252, 120, 200, 40 ), ((int)this.m_seekSlider.dragging_poisition).ToString());
		}

		//
		if( GUI.Button( new Rect( (Screen.width - 150)/2.0f, 350, 150, 40 ), "End" ) ){
			GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("Restart");
		}

		// 直前の入力のタイミングがどれくらいずれていたかを表示する.
		GUI.Label(new Rect( 5, 400, 150, 40 ),"Input Gap:" + m_scoringManager.m_lastResult.timingError);

		GUI.Label(
			new Rect( 5, 420, 150, 40 ),
			"Previous Input:"
			+ m_playerAction.lastActionInfo.triggerBeatTiming.ToString());
		GUI.Label(new Rect( 5, 440, 150, 40 ),
			"Nearest(beat):"
			+ m_musicManager.currentSongInfo.onBeatActionSequence[m_scoringManager.m_lastResult.markerIndex].triggerBeatTiming.ToString());
		GUI.Label(
			new Rect( 150, 440, 150, 40 ),
			"Nearest(index):"
			+ m_musicManager.currentSongInfo.onBeatActionSequence[m_scoringManager.m_lastResult.markerIndex].line_number.ToString());
		
		// 関連するパート名を表示
		if( m_musicManager.currentSongInfo.onBeatActionRegionSequence.Count>0 ){
			//現在のパートのインデックスを確認
			int currentReginIndex = m_actionInfoRegionSeeker.nextIndex - 1;
			if (currentReginIndex < 0)
				currentReginIndex = 0;
			//前回入力時のパートを表示
			if (m_playerAction.currentPlayerAction != PlayerActionEnum.None)
			{	
				previousHitRegionName
					= m_musicManager.currentSongInfo.onBeatActionRegionSequence[currentReginIndex].name;
			}
			GUI.Label(new Rect(150, 420, 250, 40), "region ...:" + previousHitRegionName);
			//現在のパートを表示
			GUI.Label(new Rect(5, 460, 150, 40), "Current:" + m_musicManager.beatCountFromStart);
			GUI.Label(new Rect(150, 460, 250, 40), "region ...:" + m_musicManager.currentSongInfo.onBeatActionRegionSequence[currentReginIndex].name);
		}

	}

	// シークスライダーの制御.
	private void	SeekSliderControl()
	{
		Rect	slider_rect = new Rect( (Screen.width - 100)/2.0f, 100, 130, 40 );

		if(!m_seekSlider.is_now_dragging) {

			float	new_position 
				= GUI.HorizontalSlider( slider_rect, m_musicManager.beatCount, 0, m_musicManager.length );

			// ドラッグ開始.
			if(new_position != m_musicManager.beatCount) {

				m_seekSlider.dragging_poisition = new_position;
				m_seekSlider.is_now_dragging = true;
			}


		} else {

			m_seekSlider.dragging_poisition 
				= GUI.HorizontalSlider( slider_rect, m_seekSlider.dragging_poisition, 0, m_musicManager.length );

			// ボタンが離された（ドラッグ終了）.
			if(!m_seekSlider.is_button_down) {

				m_musicManager.Seek( m_seekSlider.dragging_poisition );

				m_eventManager.Seek( m_seekSlider.dragging_poisition );
				m_scoringManager.Seek( m_seekSlider.dragging_poisition );
				m_onPlayGUI.Seek( m_seekSlider.dragging_poisition );

				Seek(m_seekSlider.dragging_poisition);

				// ドラッグ終了.
				m_seekSlider.is_now_dragging = false;
			}
		}
	}


	SequenceSeeker<SequenceRegion> m_actionInfoRegionSeeker = new SequenceSeeker<SequenceRegion>();
	MusicManager 	m_musicManager;
	ScoringManager	m_scoringManager;
	EventManager	m_eventManager;
	OnPlayGUI		m_onPlayGUI;
	PlayerAction	m_playerAction;
	string	previousHitRegionName = "";

	// シークスライダー
	private struct SeekSlider {

		public bool		is_now_dragging;		// ドラッグ中？.
		public float	dragging_poisition;		// ドラッグ位置.
		public bool		is_button_down;			// マウスの左ボタン.Input.GetMouseButton(0) の結果
												// ドキュメントに
												// Note also that the Input flags are not reset until "Update()", 
												// so its suggested you make all the Input Calls in the Update Loop
												// とあるので、念のため（実際には大丈夫っぽい？）.
	};
	private SeekSlider	m_seekSlider;

}
