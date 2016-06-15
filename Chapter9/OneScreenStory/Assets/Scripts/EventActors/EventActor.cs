
/// <summary>イベント処理を行うアクターの基底クラス</summary>
abstract class EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>アクターが生成された際に最初に実行されるメソッド</summary>
	/// UnityEngine.MonoBehaviour.Start() の代わり
	public virtual void start( EventManager evman )
	{
	}

	/// <summary>アクターが破棄されるまで毎フレーム実行されるメソッド</summary>
	/// UnityEngine.MonoBehaviour.Update() の代わり
	public virtual void execute( EventManager evman )
	{
	}

	/// <summary>アクターが破棄されるまで GUI の描画タイミングで実行されるメソッド</summary>
	/// UnityEngine.MonoBehaviour.OnGUI() の代わり
	public virtual void onGUI( EventManager evman )
	{
	}

	/// <summary>アクターで行うべき処理が終わったかどうかを返す</summary>
	public virtual bool isDone()
	{
		// 基本はすぐ終わる
		return true;
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public virtual bool isWaitClick( EventManager evman )
	{
		// 基本は待つ
		return true;
	}
}
