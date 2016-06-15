
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


/// <summary>スクリプトファイルのパーサ</summary>
class ScriptParser
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>ファイルをパースしてイベントの配列を作る</summary>
	public Event[] parseAndCreateEvents( string[] lines )
	{
		bool isInsideOfBlock = false;
		Regex tabSplitter = new Regex( "\\t+" );             // 複数のタブで分割できるようにする
		List< string > commandLines = new List< string >();  // イベントのコマンド行データ
		List< int > commandLineNumbers = new List< int >();  // イベントのコマンド行データのファイル中の行番号
		List< Event > events = new List< Event >();

		string eventName = "";
		int    lineCount = 0;
		int    beginLineCount = 0;

		foreach ( string line in lines )
		{
			// テキストファイル中の現在行
			lineCount++;

			// コメント除去
			int index = line.IndexOf( ";;" );	// "//" や "--" は使用される可能性があるので変えました
			string str = index < 0 ? line : line.Substring( 0, index );
			// 前後のスペースを無視
			str = str.Trim();

			// 以上で空行になった場合は次へ
			if ( str.Length < 1 ) continue;

			// [] で囲まれてるのが、直後のイベントの名前
			if ( str.Length >= 3 )
			{
				if ( str[0] == '[' && str[ str.Length - 1 ] == ']' )
				{
					eventName = str.Substring( 1, str.Length - 2);
				}
			}

			switch ( str.ToLower() )
			{
			// イベントブロックの開始
			case "begin":
				if ( isInsideOfBlock )
				{
					Debug.LogError( "Unclosed Event ("  + beginLineCount + ")" );
					return new Event[ 0 ];  // begin の重複エラー
				}
				beginLineCount = lineCount;

				isInsideOfBlock = true;
				break;

			// イベントブロックの終了
			case "end":
				// コマンド行データを分解
				List< string[] > commands = new List< string[] >();
				foreach ( string cl in commandLines )
				{
					string[] tabSplit = tabSplitter.Split( cl );
					commands.Add( tabSplit );
				}
				// コマンド行データをクリア
				commandLines.Clear();

				// イベントを作ってリストへ追加
				Event ev = createEvent( commands.ToArray(), eventName, commandLineNumbers.ToArray(), beginLineCount);
				if ( ev != null )
				{
					ev.setLineNumber( beginLineCount );
					events.Add( ev );
				}
				// イベントデータを初期化
				commandLineNumbers.Clear();
				eventName = "";

				isInsideOfBlock = false;
				break;

			// それ以外
			default:
				if ( isInsideOfBlock )
				{
					// イベントブロック内のみ認識
					commandLines.Add( str );
					commandLineNumbers.Add( lineCount );
				}
				break;
			}
		}

		// Begin が閉じてない.
		if ( isInsideOfBlock )
		{
			Debug.LogError( "Unclosed Event ("  + beginLineCount + ")" );
		}

		return events.ToArray();
	}


	//==============================================================================================
	// 非公開メソッド

	/// <summary>イベントのコマンドデータからイベントを生成する</summary>
	private Event createEvent( string[][] commands, string eventName, int[] numbers, int beginLineCount )
	{
		List< string >         targets     = new List< string >();
		List< EventCondition > conditions  = new List< EventCondition >();
		List< string[] >       actions     = new List< string[] >();
		List< int >            lineNumbers = new List< int >();

		DebugManager           debug_manager = GameObject.FindGameObjectWithTag( "DebugManager" ).GetComponent< DebugManager >();

		bool                   isPrologue = false;
		bool                   doContinue = false;

		int                    i = 0;

		foreach ( string[] commandParams in commands )
		{
			switch ( commandParams[ 0 ].ToLower() )
			{
			// ターゲットオブジェクト
			case "target":
				if ( commandParams.Length >= 2 )
				{
					targets.Add( commandParams[ 1 ] );
				}
				else
				{
					Debug.LogError( "Failed to add a target." );
				}
				break;

			// プロローグイベント
			case "prologue":
				isPrologue = true;
				break;

			// 発生条件
			case "condition":
				if ( commandParams.Length >= 4 )
				{
					// 指定されたオブジェクトを探す
					// ToDo: 現在の実装では非表示のオブジェクトが見つけられないので対策を検討する
					GameObject go = GameObject.Find( commandParams[ 1 ] );
					BaseObject bo = go != null ? go.GetComponent< BaseObject >() : null;

					if ( bo != null )
					{
						EventCondition ec = new EventCondition( bo, commandParams[ 2 ], commandParams[ 3 ] );
						conditions.Add( ec );

						// （デバッグ用）ウォッチする条件変数を追加する.
						debug_manager.addWatchConditionVariable( commandParams[ 1 ], commandParams[ 2 ] );
					}
					else
					{
						Debug.LogError( "Failed to add a condition." );
					}
				}
				else
				{
					Debug.LogError( "Failed to add a condition." );
				}
				break;

			// 継続評価
			case "continue":
				doContinue = true;
				break;

			// それ以外はアクション (この段階ではパラメータを保存しておくだけ)
			default:
				actions.Add( commandParams );
				lineNumbers.Add( numbers[ i ] );
				break;
			}

			++i;
		}

		if ( isPrologue )
		{
			// プロローグイベントはターゲットオブジェクトが関係しないのでクリアしておく
			targets.Clear();
		}
		else
		{
			// プロローグイベントではないときはターゲットオブジェクトが最低 2 つはないと成り立たない
			if ( targets.Count < 2 )
			{
				Debug.LogError( "Failed to create an event." );
				return null;
			}
		}

		if ( actions.Count < 1 )
		{
			// イベント処理は最低 1 つないと無意味
			Debug.LogError( "Failed to create an event at " + beginLineCount + ".");
			return null;
		}

		Event ev = new Event( targets.ToArray(), conditions.ToArray(), actions.ToArray(), isPrologue, doContinue, eventName );
		ev.setActionLineNumbers( lineNumbers.ToArray() );

		return ev;
	}
}
