using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//ステージ演出等のイベント管理
public class EventManager : MonoBehaviour {
	// Use this for initialization
	void Start(){
		m_musicManager=GameObject.Find("MusicManager").GetComponent<MusicManager>();
	}
	public void BeginEventSequence(){
		m_seekUnit.SetSequence(m_musicManager.currentSongInfo.stagingDirectionSequence);
	}
	public void Seek(float beatCount){
		m_seekUnit.Seek( beatCount );
		m_previousIndex=m_seekUnit.nextIndex;
		//シーク時には現行のリストをクリア
		for ( LinkedListNode<StagingDirection> it = m_activeEvents.First; it != null; it = it.Next) {
			it.Value.OnEnd();
			m_activeEvents.Remove(it);
		}
	}
	void Update () {

		SongInfo	song = m_musicManager.currentSongInfo;

		if( m_musicManager.IsPlaying() )
		{
			//前フレームから現フレームへの間にヒットしたステージ演出の取得

			m_previousIndex = m_seekUnit.nextIndex;

			m_seekUnit.ProceedTime(m_musicManager.beatCount - m_musicManager.previousBeatCount);

			// 「直前のシーク位置」と「更新後のシーク位置」の間にあるイベントを実行開始.
			for(int i = m_previousIndex;i < m_seekUnit.nextIndex;i++){

				// イベントデータをコピーする.
				StagingDirection clone = song.stagingDirectionSequence[i].GetClone() as StagingDirection;

				clone.OnBegin();

				// 「実行中のイベントリスト」に追加.
				m_activeEvents.AddLast(clone);
			}
		}

		// 「実行中のイベント」の実行.
		for ( LinkedListNode<StagingDirection> it = m_activeEvents.First; it != null; it = it.Next) {

			StagingDirection	activeEvent = it.Value;

			activeEvent.Update();

			// 実行が終了した？.
			if(activeEvent.IsFinished()) {

				activeEvent.OnEnd();

				// 「実行中のイベントリスト」から削除する.
				m_activeEvents.Remove(it);
			}
		}
	}

	//private variables

	MusicManager m_musicManager;

	// シークユニット.
	SequenceSeeker<StagingDirection> m_seekUnit
		= new SequenceSeeker<StagingDirection>();

	// 実行中のイベント.
	LinkedList<StagingDirection> m_activeEvents
		= new LinkedList<StagingDirection>();

	int		m_previousIndex=0;			// 直前のシーク位置.
}

