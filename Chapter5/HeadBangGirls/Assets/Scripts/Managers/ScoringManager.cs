using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//プレイヤーのアクション等からスコアの加点/減点を管理
public class ScoringManager : MonoBehaviour {
	public static float timingErrorToleranceGood = 0.22f;			// タイミングのずれがこれ以下だったら Good
	public static float timingErrorTorelanceExcellent = 0.12f;		// タイミングのずれがこれ以下だったら Excellent
	public static float missScore = -1.0f;
	public static float goodScore = 2.0f;
	public static float excellentScore = 4.0f;
	public static float failureScoreRate = 0.3f;//途中判定ポイントで"失敗"として判定される得点率(得点/理論上の最高得点)
	public static float excellentScoreRate = 0.85f;//途中判定ポイントで"優秀"として判定される得点率(得点/理論上の最高得点)
	public static float missHeatupRate = -0.08f;
	public static float goodHeatupRate = 0.01f;
	public static float bestHeatupRate = 0.02f;
	public static float temperThreshold = 0.5f;//演出の変化の有無などを分ける盛り上がりのしきい値
	public bool outScoringLog=true;
	//現在の合計スコア
	public float score{
		get{ return m_score; }
	}
	private float m_score;

	//盛り上がりの数値化 0.0 - 1.0
	public float temper
	{
		get { return m_temper; }
		set { m_temper = Mathf.Clamp(value, 0, 1); }
	}
	float m_temper = 0;
	//現フレームでのスコア変動合計値
	public float scoreJustAdded{
		get{ return m_additionalScore; }
	}

	//現在の得点率(得点/理論上の最高得点)
	public float scoreRate
	{
		get { return m_scoreRate; }
	}
	private float m_scoreRate = 0;

	//スコアの評価開始
	public void BeginScoringSequence(){
		m_scoringUnitSeeker.SetSequence(m_musicManager.currentSongInfo.onBeatActionSequence);
	}
	// Use this for initialization
    void Start()
    {
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
		m_playerAction = GameObject.Find("PlayerAvator").GetComponent<PlayerAction>();
		m_bandMembers = GameObject.FindGameObjectsWithTag("BandMember");
		m_audiences = GameObject.FindGameObjectsWithTag("Audience");
		m_noteParticles = GameObject.FindGameObjectsWithTag("NoteParticle");
		m_phaseManager = GameObject.Find("PhaseManager").GetComponent<PhaseManager>();
		//GUIオブジェクトはInactiveな可能性があるので、Findで直接アクセスできない。
		m_onPlayGUI    = m_phaseManager.guiList[1].GetComponent<OnPlayGUI>();
#if UNITY_EDITOR 
        m_logWriter = new StreamWriter("Assets/PlayLog/scoringLog.csv");
#endif
    }
	public void Seek(float beatCount){
		m_scoringUnitSeeker.Seek( beatCount );
		m_previousHitIndex=-1;
	}
	// 一番近いActionInfoのインデックスを確認
	public int	GetNearestPlayerActionInfoIndex(){

		SongInfo	song = m_musicManager.currentSongInfo;
		int 		nearestIndex = 0;

		if(m_scoringUnitSeeker.nextIndex == 0) {

			// シーク位置が先頭だった場合、ひとつ前のマーカーは無いので比較しない.
			nearestIndex = 0;

		} else if(m_scoringUnitSeeker.nextIndex >= song.onBeatActionSequence.Count) {

			// シーク位置が配列のサイズより大きいとき（最後のマーカーの時刻を過ぎていたとき）

			nearestIndex = song.onBeatActionSequence.Count - 1;

		} else {

			// 前後のタイミングとのずれを比較.

			OnBeatActionInfo	crnt_action = song.onBeatActionSequence[m_scoringUnitSeeker.nextIndex];			// シーク位置.
			OnBeatActionInfo	prev_action = song.onBeatActionSequence[m_scoringUnitSeeker.nextIndex - 1];		// シーク位置のひとつ前.

			float				act_timing = m_playerAction.lastActionInfo.triggerBeatTiming;

			if( crnt_action.triggerBeatTiming - act_timing < act_timing - prev_action.triggerBeatTiming) {

				// シーク位置（m_scoringUnitSeeker.nextIndex）の方が近かった.
				nearestIndex = m_scoringUnitSeeker.nextIndex;

			} else {

				// シーク位置のひとつ前（m_scoringUnitSeeker.nextIndex）の方が近かった.
				nearestIndex = m_scoringUnitSeeker.nextIndex - 1;
			}
		}

		return(nearestIndex);
	}

	// Update is called once per frame
	void Update () {

		m_additionalScore = 0;

		float additionalTemper = 0;
		bool hitBefore = false;
		bool hitAfter = false;

		if( m_musicManager.IsPlaying() ){

			float	delta_count = m_musicManager.beatCount - m_musicManager.previousBeatCount;

			m_scoringUnitSeeker.ProceedTime(delta_count);
			// プレイヤーが入力したタイミングの直後、また直前（近い方）のマーカーの
			// インデックスを取得する.
			if(m_playerAction.currentPlayerAction != PlayerActionEnum.None){
				int nearestIndex = GetNearestPlayerActionInfoIndex();

				SongInfo song = m_musicManager.currentSongInfo;

				OnBeatActionInfo marker_act = song.onBeatActionSequence[nearestIndex];
				OnBeatActionInfo player_act = m_playerAction.lastActionInfo;

				m_lastResult.timingError = player_act.triggerBeatTiming - marker_act.triggerBeatTiming;
				m_lastResult.markerIndex = nearestIndex;

				if (nearestIndex == m_previousHitIndex){
					// 一度判定済みのマーカーに対して、再度入力されたとき.
					m_additionalScore = 0;

				} else {

					// 初めてクリックされたマーカー.
					// タイミングの判定をする.
					m_additionalScore = CheckScore(nearestIndex, m_lastResult.timingError, out additionalTemper);
				}

				if (m_additionalScore > 0){

					// 入力成功.

					// 同じマーカーを二回判定してしまわないよう、最後に判定された
					// マーカーを覚えておく.
					m_previousHitIndex = nearestIndex;

					// 判定に使われたのが
					// ・シーク位置のマーカー(hitAftere)
					// ・シーク位置のいっこ前のマーカー(hitBefore)
					// なのか、判定する.
					//
					if (nearestIndex == m_scoringUnitSeeker.nextIndex)
						hitAfter = true;
					else
						hitBefore = true;

					//成功時の演出
					OnScoreAdded(nearestIndex);
				} else{

					// 入力失敗（タイミングが大きくずれていた）.

					//アクションをとったのに加点が無ければ減点
					m_additionalScore = missScore;

					additionalTemper = missHeatupRate;
				}
				m_score += m_additionalScore;

				temper += additionalTemper;
				m_onPlayGUI.RythmHitEffect(m_previousHitIndex, m_additionalScore);
				// デバッグ用ログ出力.
				DebugWriteLogPrev();
				DebugWriteLogPost(hitBefore, hitAfter);
			}
			if (m_scoringUnitSeeker.nextIndex > 0)
				m_scoreRate = m_score / (m_scoringUnitSeeker.nextIndex * excellentScore);
		}
	}

	// 入力の結果を判定する（うまい／へた／ミス）.
	float CheckScore(int actionInfoIndex, float timingError, out float heatup){

		float	score = 0;

		timingError = Mathf.Abs(timingError);

		do {

			// Good の範囲より大きいとき → ミス.
			if(timingError >= timingErrorToleranceGood) {

				score  = 0.0f;
				heatup = 0;
				break;
			}
			
			// Good と Excellent の間のとき → Good.
			if(timingError >= timingErrorTorelanceExcellent) {

				score  = goodScore;
				heatup = goodHeatupRate;
				break;
			}

			// Excellent の範囲のとき → Excellent.
			score  = excellentScore;
			heatup = bestHeatupRate;

		} while(false);

		return(score);
	}

	// デバッグ用ログ出力.
	private	void	DebugWriteLogPrev()
	{
#if UNITY_EDITOR
		if( m_scoringUnitSeeker.isJustPassElement ){
			if(outScoringLog){
				OnBeatActionInfo onBeatActionInfo
					= m_musicManager.currentSongInfo.onBeatActionSequence[m_scoringUnitSeeker.nextIndex-1];
				m_logWriter.WriteLine(
					onBeatActionInfo.triggerBeatTiming.ToString() + ","
					+ "IdealAction,,"
					+ onBeatActionInfo.playerActionType.ToString()
				);
				m_logWriter.Flush();
			}
		}
#endif
	}
	private void	OnScoreAdded(int nearestIndex){
		SongInfo song = m_musicManager.currentSongInfo;
		if (song.onBeatActionSequence[nearestIndex].playerActionType == PlayerActionEnum.Jump
			&& temper > temperThreshold)
		{
			foreach (GameObject bandMember in m_bandMembers)
			{
				bandMember.GetComponent<BandMember>().Jump();
			}
			foreach (GameObject audience in m_audiences)
			{
				audience.GetComponent<Audience>().Jump();
			}
			foreach (GameObject noteParticle in m_noteParticles)
			{
				noteParticle.GetComponent<ParticleSystem>().Emit(20);
			}
		}
		else if (song.onBeatActionSequence[nearestIndex].playerActionType == PlayerActionEnum.HeadBanging)
		{
			foreach (GameObject bandMember in m_bandMembers)
			{
				bandMember.GetComponent<SimpleSpriteAnimation>().BeginAnimation(1, 1);
			}
		}
	}
	// デバッグ用ログ出力.
	private void	DebugWriteLogPost(bool hitBefore, bool hitAfter)
	{
#if UNITY_EDITOR
		if(outScoringLog){
			string relation="";
			if(hitBefore){
				relation = "HIT ABOVE";
			}
			if(hitAfter){
				relation = "HIT BELOW";
			}
			string scoreTypeString = "MISS";
			if( m_additionalScore>=excellentScore )
				scoreTypeString = "BEST";
			else if( m_additionalScore>=goodScore )
				scoreTypeString = "GOOD";
			m_logWriter.WriteLine(
				m_playerAction.lastActionInfo.triggerBeatTiming.ToString() + ","
				+ " PlayerAction,"
				+ relation + " " + scoreTypeString + ","
				+ m_playerAction.lastActionInfo.playerActionType.ToString() + ","
				+ "Score=" + m_additionalScore
			);
			m_logWriter.Flush();
		}
#endif
	}

	//Private
	SequenceSeeker<OnBeatActionInfo> m_scoringUnitSeeker
		= new SequenceSeeker<OnBeatActionInfo>();
	float			m_additionalScore;
	MusicManager	m_musicManager;
	PlayerAction	m_playerAction;
	OnPlayGUI		m_onPlayGUI;
	int				m_previousHitIndex = -1;
	GameObject[]	m_bandMembers;
	GameObject[]    m_audiences;
	GameObject[]    m_noteParticles;
    TextWriter		m_logWriter;
	PhaseManager m_phaseManager;
	// プレイヤーの入力の結果.
	public struct Result {

		public float	timingError;		// タイミングのずれ（マイナス…早い　プラス…遅い）
		public int		markerIndex;		// 比較されたマーカーのインデックス
	};

	// 直前のプレイヤーの入力の、結果.
	public Result	m_lastResult;
}

