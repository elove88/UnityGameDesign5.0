using UnityEngine;
using System.Collections;

public class DaishaControl : MonoBehaviour {

	private Vector3		base_position = Vector3.zero;

	private struct ObjectInfo {

		public GameObject	go;
		public float		angle;

		public Vector3		init_position;
	};

	private	ObjectInfo	u_frame;
	private ObjectInfo	panel;

	// Use this for initialization
	void Start ()
	{
		this.base_position = this.transform.position;

		this.u_frame.go    = GameObject.Find("/u_frame");
		this.u_frame.angle = 0.0f;

		this.panel.go    = GameObject.Find("/panel");
		this.panel.angle = 0.0f;

#if true
		// 親子構造を気にしない

		this.u_frame.init_position = this.u_frame.go.transform.position;
		this.panel.init_position   = this.panel.go.transform.position;
#else
		// 親子構造を考える

		this.u_frame.init_position = this.gameObject.transform.InverseTransformPoint(this.u_frame.go.transform.position);

		this.panel.init_position = this.u_frame.go.transform.InverseTransformPoint(this.panel.go.transform.position);
#endif
	}

	// Update is called once per frame
	void Update ()
	{
		this.get_input();

#if true
		// 親子構造を気にしない

		// 台車の動き
		this.transform.position = this.base_position;

		// U字枠の回転
		this.u_frame.go.transform.position = this.u_frame.init_position;
		this.u_frame.go.transform.rotation = Quaternion.AngleAxis(this.u_frame.angle, Vector3.forward);

		// パネルの回転
		this.panel.go.transform.position = this.panel.init_position;
		this.panel.go.transform.rotation = Quaternion.AngleAxis(this.panel.angle, Vector3.forward);
#else
		// 親子構造を考える

		// ０リセット
		{
			this.panel.go.transform.position = Vector3.zero;
			this.panel.go.transform.rotation = Quaternion.identity;
		}

		// 台車の動き
		{
			this.panel.go.transform.Translate(this.base_position);

			this.transform.position = this.panel.go.transform.position;
			this.transform.rotation = this.panel.go.transform.rotation;
		}

		// U字枠の回転
		{
			this.panel.go.transform.Translate(this.u_frame.init_position);
			this.panel.go.transform.Rotate(Vector3.forward, this.u_frame.angle);

			this.u_frame.go.transform.position = this.panel.go.transform.position;
			this.u_frame.go.transform.rotation = this.panel.go.transform.rotation;
		}

		// パネルの回転
		{
			this.panel.go.transform.Translate(this.panel.init_position);
			this.panel.go.transform.Rotate(Vector3.forward, this.panel.angle);
		}
#endif
	}

	private void	get_input()
	{

		float	base_move_speed = 2.0f;

		if(Input.GetKey(KeyCode.LeftArrow)) {

			this.base_position.x -= base_move_speed*Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.RightArrow)) {

			this.base_position.x += base_move_speed*Time.deltaTime;
		}

		//

		if(Input.GetKey(KeyCode.DownArrow)) {

			this.u_frame.angle -= 60.0f*Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.UpArrow)) {

			this.u_frame.angle += 60.0f*Time.deltaTime;
		}

		//

		if(Input.GetKey(KeyCode.Z)) {

			this.panel.angle += 60.0f*Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.X)) {

			this.panel.angle -= 60.0f*Time.deltaTime;
		}
	}
}
