using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 索敵レーザーの当たり判定に使うMeshColliderを作成.
// ----------------------------------------------------------------------------
public class ScoutingLaserMeshController : MonoBehaviour {
	
	private Mesh mesh;
	private MeshFilter meshFilter;
	private MeshCollider meshCollider;
	private ScoutingLaser scoutingLaser;
	
	private static float PIECE_ANGLE = 5.0f;		// 1ポリゴンの角度(円の滑らかさ).　
	private static float FAN_RADIUS = 10.0f;		// 円の半径
	
	void Start () {

		mesh = new Mesh();
		meshFilter = GetComponent<MeshFilter>();
		meshCollider = GetComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;
		// .
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser").GetComponent<ScoutingLaser>();
		
	}
	
	void Update () {
	}
	
	public void clearShape()
	{
		mesh.Clear();
		meshFilter.mesh = mesh;
		// mesh を変更した後は false -> true にしないと反映されない.
		meshCollider.enabled = false;
		meshCollider.enabled = true;
	}

	public void makeFanShape( float[] angle )
	{
		float startAngle;					// 円の開始角度.
		float endAngle;						// 円の終了角度.
		float pieceAngle = PIECE_ANGLE;		// 1ポリゴンの角度(円の滑らかさ).
		float radius = FAN_RADIUS;			// 円の半径.
		
		startAngle = angle[0];
		endAngle = angle[1];

		// --------------------------------------------------------------------
		// 準備.
		// --------------------------------------------------------------------

		if ( Mathf.Abs ( startAngle - endAngle ) > 180f )
		{
			// 0度 <-> 359度 を跨いだと見なし、+360度する.
			if ( startAngle < 180f )
			{
				startAngle += 360f;
			}
			if ( endAngle < 180f )
			{
				endAngle += 360f;
			}
		}
		
		Vector3[]	circleVertices;			// 円を構成する各ポリゴンの頂点座標.	
		int[]		circleTriangles;		// ポリゴンの面情報(頂点接続情報).

		// 角度は 開始 < 終了 となるようにする.
		if ( startAngle > endAngle )
		{
			float tmp = startAngle;
			startAngle = endAngle;
			endAngle = tmp;
		}

		// 三角形の数.
		int	triangleNum = (int)Mathf.Ceil(( endAngle - startAngle ) / pieceAngle );

		// 配列を確保.
		circleVertices = new Vector3[triangleNum + 1 + 1];
		circleTriangles = new int[triangleNum*3];

		// --------------------------------------------------------------------
		// ポリゴンを作成.
		// --------------------------------------------------------------------

		// 頂点.

		circleVertices[0] = Vector3.zero;


		for( int i = 0; i < triangleNum + 1; i++ )
		{

			float currentAngle = startAngle + (float)i*pieceAngle;

			// 終値を超えないように.
			currentAngle = Mathf.Min( currentAngle, endAngle );

			circleVertices[1 + i] = Quaternion.AngleAxis( currentAngle, Vector3.up ) * Vector3.forward * radius;
		}

		// インデックス.

		for( int i = 0; i < triangleNum; i++ )
		{
			circleTriangles[i*3 + 0] = 0;
			circleTriangles[i*3 + 1] = i + 1;
			circleTriangles[i*3 + 2] = i + 2;
		}

		// --------------------------------------------------------------------
		// メッシュを作成.
		// --------------------------------------------------------------------

		mesh.Clear();
		
		mesh.vertices = circleVertices;
		mesh.triangles = circleTriangles;

		mesh.Optimize();
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		
		meshFilter.mesh = mesh;

		// mesh を変更した後は false -> true にしないと反映されない.
		meshCollider.enabled = false;
		meshCollider.enabled = true;
	}

	void OnTriggerEnter( Collider collider )
	{
		scoutingLaser.Lockon( collider );
	}

	void OnTriggerExit()
	{
	}
	
	void OnTriggerStay( Collider collider )
	{
	}

}
