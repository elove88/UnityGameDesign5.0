using UnityEngine;
using System.Collections;

public class SimpleSpriteGUI {

	public Texture		texture;
	public Vector3		position;
	public Vector3		scale;
	public float		angle;

	public Vector3		pivot;
	public Matrix4x4	matrix;
	public Matrix4x4	matrix_trans_rect;

	public Rect			rect;

	public bool			is_visible;

	private bool		is_update_matrix;

	// ---------------------------------------------------------------- //

	public void	create()
	{
		this.position = Vector3.zero;
		this.scale    = Vector3.one;
		this.angle    = 0.0f;

		this.pivot    = Vector3.zero;
		this.matrix   = Matrix4x4.identity;

		this.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

		this.is_update_matrix = true;

		this.is_visible = true;
	}
	public void	draw()
	{
		if(this.is_visible) {

			if(this.is_update_matrix) {
	
				this.calc_matrix();
	
				this.calc_rect();
	
				this.is_update_matrix = false;
			}
	
			GUI.matrix = this.matrix;

			GUI.DrawTexture(this.rect, this.texture);
		}
	}

	public void	setTexture(Texture texture)
	{
		this.texture = texture;

		this.pivot = new Vector3(texture.width/2.0f, texture.height/2.0f, 0.0f);

		this.calc_rect();
	}
	public void	setPosition(Vector3 position)
	{
		this.position = position;

		this.is_update_matrix = true;
	}
	public void	setScale(Vector3 scale)
	{
		this.scale = scale;

		this.is_update_matrix = true;
	}
	public void	setAngle(float angle)
	{
		this.angle = angle;

		this.is_update_matrix = true;
	}

	// ---------------------------------------------------------------- //

	private void	calc_matrix()
	{

		Vector3		position;

		position = new Vector3(this.position.x, -this.position.y, this.position.z);
		position -= this.pivot;
		position += new Vector3(Screen.width/2.0f, Screen.height/2.0f, 0.0f);

		// Rect 変換用.

		this.matrix_trans_rect = Matrix4x4.identity;

		this.matrix_trans_rect *= Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);

		// テクスチャーの中心を基準に、回転とスケールをかける.
		//
		this.matrix_trans_rect *= Matrix4x4.TRS( this.pivot,  Quaternion.identity,                               Vector3.one);
		this.matrix_trans_rect *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(this.angle, Vector3.forward), Vector3.one);
		this.matrix_trans_rect *= Matrix4x4.TRS(-this.pivot,  Quaternion.identity,                               Vector3.one);

		this.matrix_trans_rect *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(-this.angle, Vector3.forward), Vector3.one);

		this.matrix_trans_rect *= Matrix4x4.TRS( this.pivot,  Quaternion.identity, Vector3.one);
		this.matrix_trans_rect *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, this.scale);
		this.matrix_trans_rect *= Matrix4x4.TRS(-this.pivot,  Quaternion.identity, Vector3.one);

		// 描画用.

		this.matrix = Matrix4x4.identity;
		
		this.matrix *= Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
		
		// テクスチャーの中心を基準に、回転とスケールをかける.
		//
		this.matrix *= Matrix4x4.TRS( this.pivot,  Quaternion.identity,                               Vector3.one);
		this.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(this.angle, Vector3.forward), this.scale);
		this.matrix *= Matrix4x4.TRS(-this.pivot,  Quaternion.identity,                               Vector3.one);
		
		this.matrix *= this.matrix_trans_rect.inverse;

	}
	private void	calc_rect()
	{
#if false
//		this.rect = new Rect(0.0f, 0.0f, this.texture.width, this.texture.height);
		this.rect = new Rect(-this.texture.width/2.0f, -this.texture.height/2.0f, this.texture.width, this.texture.height);
#else
		Vector3		lt, rb;

		lt = new Vector3(0.0f, 0.0f, 0.0f);
		rb = new Vector3(this.texture.width, this.texture.height, 0.0f);

		lt = this.matrix_trans_rect.MultiplyPoint(lt);
		rb = this.matrix_trans_rect.MultiplyPoint(rb);

		this.rect = new Rect(lt.x, lt.y, rb.x - lt.x, rb.y - lt.y);
#endif
	}
}
