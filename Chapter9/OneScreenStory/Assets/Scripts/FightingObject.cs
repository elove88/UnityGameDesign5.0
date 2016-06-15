
using UnityEngine;


/// <summary>戦闘オブジェクト専用クラス</summary>
class FightingObject : DraggableObject
{
	//==============================================================================================
	// 内部データ型

	/// <summary>戦闘コマンドの進行状況</summary>
	private enum BattleStatus
	{
		BeforeBattle,
		ZoomingIn,
		Battle,
		AfterBattle,
		ZoomingOut,
		EndBattle
	}


	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>カメラマネージャオブジェクト</summary>
	protected CameraManager m_cameraManager = null;

	/// <summary>戦闘中に表示するエフェクト</summary>
	public ParticleSystem[] m_fightingEffects = null;

	/// <summary>スタートアップメソッド</summary>
	private void Start()
	{
		m_cameraManager = CameraManager.get ();
		m_objectManager = m_eventManager.GetComponent< ObjectManager >();
		m_soundManager  = SoundManager.get();
	}


	//==============================================================================================
	// 公開メソッド

	/// <summary>アクターから来たメッセージをフレーム毎に処理する</summary>
	public override bool updateMessage( string message, string[] parameters )
	{
		switch ( message )
		{
		// 戦闘コマンド
		case "battle":
			{
				switch ( m_battleStatus )
				{
				case BattleStatus.BeforeBattle:
					// テキストを消す
					TextManager textman = m_objectManager.GetComponent< TextManager >();
					if ( textman != null ) textman.hide();

					// 戦闘発生場所からカメラに向かって伸びるレイを求める
					BaseObject enemy = m_objectManager.find( parameters[ 0 ] );
					Vector3 myPosition    = transform.position + 0.5f * ( getYTop() + getYBottom() ) * Vector3.up;
					Vector3 enemyPosition = enemy.transform.position + 0.5f * ( enemy.getYTop() + enemy.getYBottom() ) * Vector3.up;
					Vector3 battlePosition = Vector3.Lerp( myPosition, enemyPosition, 0.5f );
					Quaternion qt = Quaternion.AngleAxis( 20.0f, Vector3.right );

					m_rayFromBattlePositionToCamera = new Ray( battlePosition, qt * -Vector3.forward );

					// 戦闘場所へズームイン
					m_cameraManager.moveTo(
						m_rayFromBattlePositionToCamera.GetPoint( /*5100.0f*/600.0f / Mathf.Sin( 32.92f * Mathf.Deg2Rad ) ),
						22.0f,
						m_cameraManager.getOriginalSize() * 0.5f,
						0.5f );

					m_battleStatus = BattleStatus.ZoomingIn;
					return true;  // 実行を継続

				case BattleStatus.ZoomingIn:
					// カメラ移動中
					if ( !m_cameraManager.isMoving() )
					{
						m_battleStatus = BattleStatus.Battle;
						m_currentFrame = 0;
					}
					return true;  // 実行を継続

				case BattleStatus.Battle:
					// アニメーション/サウンド再生
					if ( m_currentFrame == 0 )
					{
						foreach ( ParticleSystem particle in m_fightingEffects )
						{
							// 戦闘のエフェクトはキャラクターにかぶさるようにちょっと手前に表示
							particle.transform.position = m_rayFromBattlePositionToCamera.GetPoint( 100.0f );
							particle.Play();
						}

						// 戦闘音再生
						m_soundManager.playSE( "rpg_system07", true );
					}

					if ( ++m_currentFrame >= m_animationFrames )
					{
						m_battleStatus = BattleStatus.AfterBattle;
					}
					return true;  // 実行を継続

				case BattleStatus.AfterBattle:
					// アニメーション/サウンド停止
					foreach ( ParticleSystem particle in m_fightingEffects )
					{
						particle.Stop();
					}
					m_soundManager.stopSE();

					// カメラを元の位置に戻す
					m_cameraManager.moveTo(
						m_cameraManager.getOriginalPosition(),
						m_cameraManager.getOriginalRotationX(),
						m_cameraManager.getOriginalSize(),
						0.5f );

					m_battleStatus = BattleStatus.ZoomingOut;
					return true;  // 実行を継続

				case BattleStatus.ZoomingOut:
					// カメラ移動中
					if ( !m_cameraManager.isMoving() )
					{
						m_battleStatus = BattleStatus.EndBattle;
					}
					return true;  // 実行を継続

				case BattleStatus.EndBattle:
					// 終了
					m_battleStatus = BattleStatus.BeforeBattle;
					return false;

				default:
					return false;
				}
			}
		}

		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>オブジェクトマネージャオブジェクト</summary>
	private ObjectManager m_objectManager;

	/// <summary>サウンドマネージャオブジェクト</summary>
	private SoundManager m_soundManager;

	/// <summary>戦闘コマンドの進行状況</summary>
	private BattleStatus m_battleStatus = BattleStatus.BeforeBattle;

	/// <summary>戦闘発生場所からカメラに向かって伸びるレイ</summary>
	private Ray m_rayFromBattlePositionToCamera;

	/// <summary>戦闘アニメーションのフレーム番号</summary>
	private int m_currentFrame = 0;

	/// <summary>戦闘アニメーションのフレーム数</summary>
	private const int m_animationFrames = 300;
}
