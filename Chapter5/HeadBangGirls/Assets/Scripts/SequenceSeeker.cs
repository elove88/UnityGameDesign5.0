using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

//シーケンスを走査し、直近の要素のインデックスを取得するクラス
public class SequenceSeeker<ElementType>
	where ElementType: MusicalElement
{	//走査するシーケンスデータをセット
	public void SetSequence( List<ElementType> sequence ){
		m_sequence = sequence;
		m_nextIndex=0;
		m_currentBeatCount=0;
		m_isJustPassElement=false;
	}
	//一番近い次の要素を示すインデックス番号
	public int nextIndex{
			get{return m_nextIndex;}
	}
	//要素のトリガー位置を通過した時にtrue
	public bool isJustPassElement{
			get{return m_isJustPassElement;}
	}

	//毎フレーム処理
	public void ProceedTime(float deltaBeatCount){

		// 現在時刻を進める.
		m_currentBeatCount += deltaBeatCount;
		// 「シーク位置が進んだ」瞬間を表すフラグを落としておく.
		m_isJustPassElement = false;

		int		index = find_next_element(m_nextIndex);

		// 次のエレメントが見つかった.
		if(index!=m_nextIndex){

			// シーク位置を更新.
			m_nextIndex = index;

			// 「シーク位置が進んだ」瞬間を表すフラグをセット.
			m_isJustPassElement=true;
		}
	}
	//走査
	public void Seek(float beatCount){

		m_currentBeatCount = beatCount;

		int		index = find_next_element(0);

		m_nextIndex = index;
	}

	// m_currentBeatCount の直後にあるエレメントを探す.
	//
	private int	find_next_element(int start_index)
	{
		// 『最後のマーカーの時刻を過ぎていた』ことを表す値で初期化しておく.
		int ret = m_sequence.Count;

		for (int i = start_index;i < m_sequence.Count; i++)
		{
			// m_currentBeatCount よりも後ろにあるマーカーだった＝見つかった.
			if(m_sequence[i].triggerBeatTiming > m_currentBeatCount)
			{
				ret = i;
				break;
			}
		}

		return(ret);
	}

//private variables
	int		m_nextIndex = 0;				//シーク位置（＝現在時刻からみて、次にあるエレメントのインデックス）.
	float	m_currentBeatCount = 0;			//現在時刻.
	bool	m_isJustPassElement = false;	//シーク位置が進んだフレームのみ、true になる.

	List<ElementType> m_sequence;			//シークするシーケンスデータ.
}

