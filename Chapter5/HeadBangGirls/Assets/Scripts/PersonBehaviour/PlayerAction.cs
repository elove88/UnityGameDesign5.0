using UnityEngine;
using System.Collections;
//プレイヤーのアクションの種類を表す列挙
public enum PlayerActionEnum{
	None,
	HeadBanging,
	Jump
};
//プレイヤーのアクション
public class PlayerAction : MonoBehaviour {
	public AudioClip headBangingSoundClip_GOOD;
	public AudioClip headBangingSoundClip_BAD;
	//プレイヤーの現在のアクション
	public PlayerActionEnum currentPlayerAction{
		get{ return m_currentPlayerAction; }
	}
	//プレイヤーが最後にとったアクション
	public OnBeatActionInfo lastActionInfo{
		get{ return m_lastActionInfo; }
	}
	// Use this for initialization
	void Start () {
		m_musicManager = GameObject.Find("MusicManager").GetComponent<MusicManager>();
	}
	
	// Update is called once per frame
	void Update () {
		m_currentPlayerAction = m_newPlayerAction;
		m_newPlayerAction = PlayerActionEnum.None;
	}
	public void DoAction(PlayerActionEnum actionType){
		m_newPlayerAction = actionType;

		OnBeatActionInfo actionInfo = new OnBeatActionInfo();
		actionInfo.triggerBeatTiming = m_musicManager.beatCountFromStart;
		actionInfo.playerActionType = m_newPlayerAction;
		m_lastActionInfo = actionInfo;

		if(actionType == PlayerActionEnum. HeadBanging){
			gameObject.GetComponent<SimpleSpriteAnimation>().BeginAnimation(2, 1, false);
		}
		else if (actionType == PlayerActionEnum.Jump)
		{	
			gameObject.GetComponent<SimpleActionMotor>().Jump();
			gameObject.GetComponent<SimpleSpriteAnimation>().BeginAnimation(1, 1, false);
		}
	}
	//入力に対応したアクションを行う
	//Private variables
	MusicManager m_musicManager;
	OnBeatActionInfo m_lastActionInfo=new OnBeatActionInfo();
	PlayerActionEnum m_currentPlayerAction;
	PlayerActionEnum m_newPlayerAction;
}
