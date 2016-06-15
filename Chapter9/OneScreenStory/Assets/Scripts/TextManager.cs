
using UnityEngine;


/// <summary>地の文・会話文の表示クラス</summary>
class TextManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>テキストオブジェクト</summary>
	public GUIText m_text = null;
	/// <summary>フォントサイズ</summary>
	public int m_fontSize = 0;

	/// <summary>左上 GUI テクスチャオブジェクト</summary>
	public GUITexture m_northWestTexture = null;
	/// <summary>左 GUI テクスチャオブジェクト</summary>
	public GUITexture m_westTexture = null;
	/// <summary>左下 GUI テクスチャオブジェクト</summary>
	public GUITexture m_southWestTexture = null;
	/// <summary>中央 GUI テクスチャオブジェクト</summary>
	public GUITexture m_centerTexture = null;
	/// <summary>右上 GUI テクスチャオブジェクト</summary>
	public GUITexture m_northEastTexture = null;
	/// <summary>右 GUI テクスチャオブジェクト</summary>
	public GUITexture m_eastTexture = null;
	/// <summary>右下 GUI テクスチャオブジェクト</summary>
	public GUITexture m_southEastTexture = null;
	/// <summary>吹き出し GUI テクスチャオブジェクト</summary>
	public GUITexture m_balloonTexture = null;

	/// <summary>吹き出し部分テクスチャの幅</summary>
	public float m_balloonWidth = 0.0f;
	/// <summary>吹き出し部分テクスチャの高さ</summary>
	public float m_balloonHeight = 0.0f;
	/// <summary>吹き出し部分とオブジェクトのすき間</summary>
	public float m_voidBetweenDialogAndObject = 0.0f;
	/// <summary>吹き出しを表示する際のスクリーンマージン</summary>
	public float m_screenMarginOfBalloon = 0.0f;

	/// <summary>地の文の背景色</summary>
	public Color m_textBackground = new Color32( 0, 0, 0, 160 );

	/// <summary>会話文を表示するときに鳴らすオーディオソース</summary>
	public AudioSource m_dialogSoundSource = null;


	//==============================================================================================
	// 公開メソッド

	/// <summary>地の文・会話文を隠す</summary>
	public void hide()
	{
		m_text.enabled             =
		m_northWestTexture.enabled =
		m_westTexture.enabled      =
		m_southWestTexture.enabled =
		m_centerTexture.enabled    =
		m_northEastTexture.enabled =
		m_eastTexture.enabled      =
		m_southEastTexture.enabled =
		m_balloonTexture.enabled   = false;
	}

	/// <summary>地の文を表示する</summary>
	public void showText( string text, Vector2 center, float marginX, float marginY, float radius )
	{
		// いったん隠す
		hide();

		// テキスト位置調整・表示
		m_text.text = text;
		Rect r = m_text.GetScreenRect();
		int textLines = text.Trim().Split( '\n' ).Length;
		float textWidth  = r.width;
		float textHeight = ( m_text.lineSpacing * ( textLines - 1 ) + 1.0f ) * m_fontSize;  // 最終行は lineSpacing を無視
		m_text.pixelOffset = new Vector2( center.x, center.y - ( r.height - textHeight ) / 2.0f );
		m_text.enabled     = true;

		// GUI テクスチャを表示
		showGUITexture( new Rect( center.x, center.y, textWidth + 2.0f * marginX, textHeight + 2.0f * marginY ), radius, m_textBackground );
	}

	/// <summary>会話文を表示する</summary>
	public void showDialog( BaseObject baseObject, string text, float marginX, float marginY, float radius )
	{
		// いったん隠す
		hide();

		// テキストの表示に必要な幅と高さを取得・計算
		m_text.text = text;
		Rect r = m_text.GetScreenRect();
		int textLines = text.Trim().Split( '\n' ).Length;
		float textWidth  = r.width;
		float textHeight = ( m_text.lineSpacing * ( textLines - 1 ) + 1.0f ) * m_fontSize;

		// マージンを入れた幅と高さを計算
		float wholeWidth  = textWidth  + 2.0f * marginX;
		float wholeHeight = textHeight + 2.0f * marginY;

		// GameObject のスクリーン座標を取得する
		Vector3 screenPointTop    = Camera.main.WorldToScreenPoint(
			baseObject.gameObject.transform.position + new Vector3( 0.0f, baseObject.getYTop(), 0.0f ) );
		Vector3 screenPointBottom = Camera.main.WorldToScreenPoint(
			baseObject.gameObject.transform.position + new Vector3( 0.0f, baseObject.getYBottom(), 0.0f ) );

		// GUI テクスチャの表示位置を計算する (dialogXY は中心座標, balloonXY は上部表示の場合は左下/下部表示の場合は左上座標)
		float dialogX  = screenPointTop.x + 0.2f * wholeWidth;  // ちょっとだけ右にずらすと吹き出しっぽくなる
		float balloonX = screenPointTop.x - m_balloonWidth / 2.0f;
		if ( dialogX - wholeWidth / 2.0f < m_screenMarginOfBalloon )
		{
			// 吹き出しが左に突き出してしまう
			dialogX = m_screenMarginOfBalloon + wholeWidth / 2.0f;
			{
				// 背景の円形部分の最大半径を計算
				float radiusMaxX = wholeWidth  / 2.0f;
				float radiusMaxY = wholeHeight / 2.0f;
				float radiusMax  = radiusMaxX < radiusMaxY ? radiusMaxX : radiusMaxY;
				// 最大半径より大きい指定は無効
				if ( radius > radiusMax ) { radius = radiusMax; }
				// balloonX 位置調整
				if ( balloonX < m_screenMarginOfBalloon + radius ) { balloonX = m_screenMarginOfBalloon + radius; }
			}
		}
		else if ( dialogX + wholeWidth / 2.0f + m_screenMarginOfBalloon > Camera.main.pixelWidth )
		{
			// 吹き出しが右に突き出してしまう
			dialogX = Camera.main.pixelWidth - m_screenMarginOfBalloon - wholeWidth / 2.0f;
			{
				// 背景の円形部分の最大半径を計算
				float radiusMaxX = wholeWidth  / 2.0f;
				float radiusMaxY = wholeHeight / 2.0f;
				float radiusMax  = radiusMaxX < radiusMaxY ? radiusMaxX : radiusMaxY;
				// 最大半径より大きい指定は無効
				if ( radius > radiusMax ) { radius = radiusMax; }
				// balloonX 位置調整
				if ( balloonX > Camera.main.pixelWidth - m_screenMarginOfBalloon - radius - m_balloonWidth )
				{ balloonX = Camera.main.pixelWidth - m_screenMarginOfBalloon - radius - m_balloonWidth; }
			}
		}

		// 吹き出しの位置を決定
		bool isUpper = true;	// 原則頭上
		if ( screenPointTop.y + m_voidBetweenDialogAndObject + m_balloonHeight + wholeHeight + m_screenMarginOfBalloon > Camera.main.pixelHeight )
		{
			// 吹き出しが上に突き出してしまう場合は足下
			isUpper = false;
		}
		float dialogY, balloonY;
		if ( isUpper )
		{
			// 吹き出しは上部に表示する
			balloonY = screenPointTop.y + m_voidBetweenDialogAndObject;
			dialogY  = balloonY + m_balloonHeight + wholeHeight / 2.0f;
		}
		else
		{
			// 吹き出しは下部に表示する
			balloonY = screenPointBottom.y - m_voidBetweenDialogAndObject;
			dialogY  = balloonY - m_balloonHeight - wholeHeight / 2.0f;
		}

		// GUI テクスチャを表示
		showGUITexture( new Rect( dialogX, dialogY, wholeWidth, wholeHeight ), radius, baseObject.getDialogBackgroundColor() );

		// 吹き出し部分の GUI テクスチャを表示
		m_balloonTexture.color      = baseObject.getDialogBackgroundColor();
		m_balloonTexture.pixelInset = new Rect( balloonX, balloonY, m_balloonWidth, isUpper ? m_balloonHeight : -m_balloonHeight );
		m_balloonTexture.enabled    = true;

		// テキストを表示
		m_text.pixelOffset = new Vector2( dialogX, dialogY - ( r.height - textHeight ) / 2.0f );
		m_text.enabled = true;

		// 会話文表示のときはサウンドを鳴らす
		m_dialogSoundSource.Play();
	}


	//==============================================================================================
	// 非公開メソッド

	/// <summary>吹き出し部分以外の GUITexture 背景オブジェクトを表示する</summary>
	private void showGUITexture( Rect rect, float radius, Color color )
	{
		// 最大許容半径 (幅もしくは高さの半分) を超えないようにする
		radius = Mathf.Min( radius, rect.width  / 2.0f, rect.height / 2.0f );

		// 左上
		float x      = rect.x - rect.width  / 2.0f;
		float y      = rect.y + rect.height / 2.0f - radius;
		float width  = radius;
		float height = radius;
		m_northWestTexture.color      = color;
		m_northWestTexture.pixelInset = new Rect( x, y, width, height );
		m_northWestTexture.enabled    = true;

		// 左
		height = rect.height - 2.0f * radius;
		y     -= height;
		m_westTexture.color      = color;
		m_westTexture.pixelInset = new Rect( x, y, width, height );
		m_westTexture.enabled    = true;

		// 左下
		height = -radius;
		m_southWestTexture.color      = color;
		m_southWestTexture.pixelInset = new Rect( x, y, width, height );
		m_southWestTexture.enabled    = true;

		// 中央
		x     += radius;
		y     -= radius;
		width  = rect.width - 2.0f * radius;
		height = rect.height;
		m_centerTexture.color      = color;
		m_centerTexture.pixelInset = new Rect( x, y, width, height );
		m_centerTexture.enabled    = true;

		// 右上
		x     += width  + radius;
		y     += height - radius;
		width  = -radius;
		height =  radius;
		m_northEastTexture.color      = color;
		m_northEastTexture.pixelInset = new Rect( x, y, width, height );
		m_northEastTexture.enabled    = true;

		// 右
		height = rect.height - 2.0f * radius;
		y     -= height;
		m_eastTexture.color      = color;
		m_eastTexture.pixelInset = new Rect( x, y, width, height );
		m_eastTexture.enabled    = true;

		// 右下
		height = -radius;
		m_southEastTexture.color      = color;
		m_southEastTexture.pixelInset = new Rect( x, y, width, height );
		m_southEastTexture.enabled    = true;
	}
}
