using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//曲の情報や演奏の開始/停止等の管理
public class MusicManager : MonoBehaviour {
	private SongInfo m_currentSongInfo;
	//現フレームの曲再生位置
	public float beatCountFromStart{
		get{ return m_beatCountFromStart;}
	}
	public float beatCount{
		get{ return m_beatCountFromStart;}
	}
	//前フレームの曲再生位置
	public float previousBeatCountFromStart{
		get{ return m_previousBeatCountFromStart;}
	}
	public float previousBeatCount{
		get{ return m_previousBeatCountFromStart;}
	}
	//曲の長さ(拍単位)
	public float length{
		get{ return m_audioSource.clip.length * m_currentSongInfo.beatPerSecond ; }
	}
	//曲情報
	public SongInfo currentSongInfo{
		set{ m_currentSongInfo = value; }
		get{ return m_currentSongInfo; }
	}

	void Awake()
	{
		Application.targetFrameRate = 60;
	}
	// Use this for initialization
	void Start() {
		//Assume gomeObject has AudioSource component
		m_audioSource = gameObject.GetComponent<AudioSource>();
		m_musicFinished = false;
	}
	// Update is called once per frame
	void Update () {
		//演奏中は常に曲の再生位置をチェック
		if (m_audioSource.isPlaying)
		{
			m_previousBeatCountFromStart = m_beatCountFromStart;
			m_beatCountFromStart = m_audioSource.time * m_currentSongInfo.beatPerSecond;
			m_isPlayPreviousFrame = true;
		}
		else
		{
			if (m_isPlayPreviousFrame
				&& !(0 < m_audioSource.timeSamples && m_audioSource.timeSamples < m_audioSource.clip.samples)
			)
			{
				m_musicFinished = true;
			}
			m_isPlayPreviousFrame = false;
		}
	}
	//再生位置指定
	public void Seek(float beatCount){
		m_audioSource.time =  beatCount / m_currentSongInfo.beatPerSecond;
		m_beatCountFromStart = m_previousBeatCountFromStart = beatCount;
	}
	public void PlayMusicFromStart(){
		m_musicFinished=false;
		m_isPlayPreviousFrame=false;
		m_beatCountFromStart=0;
		m_previousBeatCountFromStart=0;
		m_audioSource.Play();
	}
	public bool IsPlaying(){
		return m_audioSource.isPlaying;
	}
	public bool IsFinished(){
		return m_musicFinished;
	}
	
	//Variables
	AudioSource m_audioSource;
	float m_beatCountFromStart=0;
	float m_previousBeatCountFromStart=0;
	bool m_isPlayPreviousFrame=false;
	bool m_musicFinished=false;
}
