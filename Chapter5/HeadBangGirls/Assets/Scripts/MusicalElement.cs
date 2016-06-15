using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

//ステージ演出やスコア評価ユニットなど、音楽に合わせて行われる処理のベースクラス
public abstract class MusicalElement {
	//処理を開始できるタイミング
	public float triggerBeatTiming = 0;
	//パラメータ値の文字列配列(CSVなどの読み込みに使用) 
	public virtual void ReadCustomParameterFromString(string[] parameters){}
	//triggerBeatTimingの原点を指定した上でクローンを生成
	public virtual MusicalElement GetClone(){
		MusicalElement clone = this.MemberwiseClone() as MusicalElement;
		return clone;
	}
	public System.Xml.Schema.XmlSchema GetSchema(){return null;}
};
//音楽のパート(Aメロ、サビなど)情報を保持するクラス
public class SequenceRegion: MusicalElement{
	public float totalBeatCount;
	public string name;
	public float repeatPosition;
};

//プレイヤーが音楽に合わせて行うべきアクションの情報
public class OnBeatActionInfo : MusicalElement {
	public PlayerActionEnum playerActionType;//アクションの種類
	public string GetCustomParameterAsString_CSV(){
		return "SingleShot," + triggerBeatTiming.ToString() + "," + playerActionType.ToString();
	}

	public int	line_number;		// もとのテキスト中の行番号.
}
