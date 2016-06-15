using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//ゲームプレイ中に表示するGUIの挙動
public class OnPlayGUI : MonoBehaviour {
	public Texture messageTexture_Best;
	public Texture messageTexture_Good;
	public Texture messageTexture_Miss;
	public Texture headbangingIcon;
	public Texture beatPositionIcon;
	public Texture hitEffectIcon;
	public Texture temperBar;
	public Texture temperBarFrame;

	public static float markerEnterOffset = 2.5f;	//マーカーの表示を開始するタイミング(何ビート後のアクションが出現するか)
	public static float markerLeaveOffset =-1.0f;	//マーカーの表示を終了するタイミング(何ビート後のアクションが出現するか)

	public static int rythmHitEffectShowFrameDuration = 20;
	public static int messatShowFrameDuration = 40;

	public bool isDevelopmentMode=false;
	public Vector2 		markerOrigin = new Vector2(20.0f, 300.0f);

	public GUISkin	guiSkin;

	public void BeginVisualization(){
		m_musicManager=GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager=GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
		m_seekerBack.SetSequence(m_musicManager.currentSongInfo.onBeatActionSequence);
		m_seekerFront.SetSequence(m_musicManager.currentSongInfo.onBeatActionSequence);
		m_seekerBack.Seek(markerLeaveOffset);
		m_seekerFront.Seek(markerEnterOffset);
	}
    public void RythmHitEffect(int actionInfoIndex, float score)
    {	m_lastInputScore = score;
		m_rythmHitEffectCountDown = rythmHitEffectShowFrameDuration;
		m_messageShowCountDown=messatShowFrameDuration;
		if(score<0){
			m_playerAvator.GetComponent<AudioSource>().clip
				= m_playerAvator.GetComponent<PlayerAction>().headBangingSoundClip_BAD;
			messageTexture = messageTexture_Miss;
		}
		else if(score<=ScoringManager.goodScore){
			m_playerAvator.GetComponent<AudioSource>().clip
				= m_playerAvator.GetComponent<PlayerAction>().headBangingSoundClip_GOOD;
			messageTexture = messageTexture_Good;
		}
		else{
			m_playerAvator.GetComponent<AudioSource>().clip
				= m_playerAvator.GetComponent<PlayerAction>().headBangingSoundClip_GOOD;
			messageTexture = messageTexture_Best;
		}
		m_playerAvator.GetComponent<AudioSource>().Play();
    }

	// Use this for initialization
	void Start(){
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_scoringManager = GameObject.Find("ScoringManager").GetComponent<ScoringManager>();
		m_playerAvator = GameObject.Find("PlayerAvator");
	}
	public void Seek(float beatCount){
		m_seekerBack.Seek(beatCount + markerLeaveOffset);
		m_seekerFront.Seek(beatCount + markerEnterOffset);
	}
	// Update is called once per frame
	void Update () {
		if(m_musicManager.IsPlaying()){
			m_seekerBack.ProceedTime( m_musicManager.beatCountFromStart - m_musicManager.previousBeatCountFromStart);
			m_seekerFront.ProceedTime( m_musicManager.beatCountFromStart -m_musicManager.previousBeatCountFromStart);
		}
	}
	void OnGUI(){
		//スコア表示
		GUI.Box(new Rect(15,5,100,30),"");
		GUI.Label(new Rect(20,10,90,20),"Score: " + m_scoringManager.score);
		//ハイテンションのときの明滅色
		if (m_scoringManager.temper > ScoringManager.temperThreshold)
		{
			m_blinkColor.g = m_blinkColor.b
				= 0.7f + 0.3f * Mathf.Abs(
					Time.frameCount % Application.targetFrameRate - Application.targetFrameRate / 2
				) / (float)Application.targetFrameRate;
			GUI.color=m_blinkColor;
		}
		//盛り上がりゲージ表示
		Rect heatBarFrameRect=new Rect(180.0f, 20.0f, 100.0f, 20.0f);
		Rect heatBarRect = heatBarFrameRect;
		Rect heatBarLabelRect = heatBarFrameRect;
		heatBarRect.width *= m_scoringManager.temper;
		heatBarLabelRect.y = heatBarFrameRect.y-20;
		GUI.Label(heatBarLabelRect, "Temper");
		GUI.Box( heatBarFrameRect,"" );
		GUI.DrawTextureWithTexCoords( 
			heatBarRect, temperBar, new Rect(0.0f, 0.0f, 1.0f * m_scoringManager.temper, 1.0f)
		);

		GUI.color = Color.white;

		//このアイコンとアクションタイミングのアイコンが重なった時に入力
		float 	markerSize = ScoringManager.timingErrorToleranceGood * m_pixelsPerBeats;

		Graphics.DrawTexture(
			new Rect(markerOrigin.x - markerSize / 2.0f, markerOrigin.y - markerSize / 2.0f, markerSize, markerSize)
			, beatPositionIcon
		);

		if( m_musicManager.IsPlaying() ){

			SongInfo			song =  m_musicManager.currentSongInfo;

			// 表示を開始するマーカー.
			int	begin = m_seekerBack.nextIndex;

			// 表示を終了するマーカー.
			int end   = m_seekerFront.nextIndex;
			float x_offset;

			//アクションタイミングを示すアイコンを描画.
			for ( int drawnIndex = begin; drawnIndex < end; drawnIndex++) {
				OnBeatActionInfo	info = song.onBeatActionSequence[drawnIndex];

				float size = ScoringManager.timingErrorToleranceGood * m_pixelsPerBeats;
				//テンションが高いとき、ジャンプアクションのマーカーを大きくする
				if (m_scoringManager.temper > ScoringManager.temperThreshold && info.playerActionType == PlayerActionEnum.Jump)
				{
					size *= 1.5f;
				}

				// 表示位置までのX座標のオフセットを求める.
				x_offset = info.triggerBeatTiming - m_musicManager.beatCount;

				x_offset *= m_pixelsPerBeats;


				Rect drawRect = new Rect(
					markerOrigin.x - size/2.0f + x_offset,
					markerOrigin.y - size/2.0f ,
					size,
					size
				);
			
				GUI.DrawTexture( drawRect, headbangingIcon );
				GUI.color = Color.white;

				// テキストファイル中の行番号を表示する.
				if( isDevelopmentMode ){
					GUI.skin = this.guiSkin;
					GUI.Label(new Rect(drawRect.x, drawRect.y - 10.0f, 50.0f, 30.0f), info.line_number.ToString());
					GUI.skin = null;
				}
			}

			//アクションタイミングのヒットエフェクト
			if( m_rythmHitEffectCountDown>0  ){
				float scale=2.0f - m_rythmHitEffectCountDown / (float)rythmHitEffectShowFrameDuration;
				if( m_lastInputScore >= ScoringManager.excellentScore){
					scale *= 2.0f;
				}
				else if( m_lastInputScore > ScoringManager.missScore){
					scale *= 1.2f;
				}
				else{
					scale *= 0.5f;
				}
				float baseSize = 32.0f;
				Rect drawRect3 = new Rect(
					markerOrigin.x - baseSize * scale / 2.0f,
					markerOrigin.y - baseSize * scale / 2.0f,
					baseSize * scale,
					baseSize * scale
				);
				Graphics.DrawTexture(drawRect3, hitEffectIcon);
				m_rythmHitEffectCountDown--;
			}
			//メッセージを表示
			if( m_messageShowCountDown > 0 ){
				GUI.color=new Color(1, 1, 1, m_messageShowCountDown/40.0f );
				GUI.DrawTexture(new Rect(20,230,150,50),messageTexture,ScaleMode.ScaleAndCrop, true);
				GUI.color=Color.white;
				m_messageShowCountDown--;
			}
		}
	}
	//private Variables
	float	m_pixelsPerBeats = Screen.width * 1.0f/markerEnterOffset;
	int		m_messageShowCountDown=0;
	int		m_rythmHitEffectCountDown = 0;
	float	m_lastInputScore = 0;
	Color	m_blinkColor = new Color(1,1,1);


	// 時間的に進んでいるシークユニット（表示終了位置）.
	SequenceSeeker<OnBeatActionInfo> m_seekerFront = new SequenceSeeker<OnBeatActionInfo>();

	// 時間的に遅れているシークユニット（表示開始位置）.
	SequenceSeeker<OnBeatActionInfo> m_seekerBack = new SequenceSeeker<OnBeatActionInfo>();

	MusicManager	m_musicManager;
	ScoringManager	m_scoringManager;
	GameObject      m_playerAvator;
	Texture 		messageTexture;
}
