//using UnityEngine;
using System.Collections;

// std::vector っぽい配列（ただしサイズは固定、値型じゃないとだめかも）.
public class StaticArray<T> {


	public StaticArray(int max_num)
	{
		this.entity = new T[max_num];

		this.num_max     = max_num;
		this.num_current = 0;

	}

	//

	public void		push_back(T x)
	{
		if(this.num_current < this.num_max) {

			this.entity[this.num_current++] = x;
		}
	}
	public int		size()
	{
		return(this.num_current);
	}
	public int		capacity()
	{
		return(this.num_max);
	}
	public bool		full()
	{
		return(this.num_current >= this.num_max);
	}

	public void		resize(int size)
	{
		this.num_current = size;
	}
	public void		clear()
	{
		this.resize(0);
	}

	public void		erase_by_index(int index)
	{
		for(int i = index;i < this.num_current - 1;i++) {

			this.entity[i] = this.entity[i + 1];
		}

		this.num_current--;
	}

	public void		swap(int i, int j)
	{
		T	temp;

		temp           = this.entity[i];
		this.entity[i] = this.entity[j];
		this.entity[j] = temp;
	}

	public T	this[int i]
	{
		get { return(this.entity[i]); }
		set { this.entity[i] = value; }
	} 
    public IEnumerator GetEnumerator()
    {
		int		i = 0;

		for(i = 0;i < this.num_max;i++) {

			yield return(this.entity[i]);
		}
    }

	private int		num_max;
	private int		num_current;

	private T[]		entity;
};

