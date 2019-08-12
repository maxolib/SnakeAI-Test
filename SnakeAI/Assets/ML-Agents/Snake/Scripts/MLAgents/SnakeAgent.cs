using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Snake.Scripts.Utilities;
using MLAgents;

public class SnakeAgent : Agent {
	// -------------------------------------------------------------------------------------------------
	private const float DURATION_NEXT_TURN = 0.01f;
	[SerializeField] private GameObject m_Ground;
	[SerializeField] private GameObject m_Snake;
	[SerializeField] private GameObject m_Food;
	[SerializeField] private Transform m_TailTransform;

	// -------------------------------------------------------------------------------------------------
	private SnakeAcademy _SnakeAcademy;
	private int _Size;
	private bool _IsPlay;
	private State _State;
	private float _Timer;
	private List<GameObject> _Tails;
	private State.DirectionType _Direction;

	// -------------------------------------------------------------------------------------------------

	public void Awake () {
		_SnakeAcademy = FindObjectOfType(typeof(SnakeAcademy)) as SnakeAcademy;

		SetStateObject();

		Observable.EveryUpdate().Where(_ => _IsPlay && !_State.GameOver).Subscribe
		(
			_ => 
			{
				_Timer += Time.deltaTime;
			}
		).AddTo(this);
		
		Observable.EveryUpdate().Where(_ => _Timer > _SnakeAcademy.Speed).Subscribe
		(
			_ => 
			{
				_Timer = 0f;
				_State.Next(_Direction);
				if(_State.GameOver)
				{
					SetReward(-1f);
					Done();
				}
				else if(_State.Eat)
				{
					SetReward(1f);
				}
				RenderState();
			}
		).AddTo(this);


		RenderState();
		_IsPlay = true;
	}

	public override void InitializeAgent()
	{

	}
	public void RenderState()
	{
		m_Snake.transform.position = SetTransform(_State.Head) + transform.position;
		m_Food.transform.position = SetTransform(_State.Food) + transform.position;
		if(_State.Eat)
		{
			var tailPosition = _State.Tails[_State.Tails.Count-1] + new Vector2(transform.position.x, transform.position.y);
			GameObject tail = Instantiate(m_Snake.gameObject, tailPosition, Quaternion.identity);
			tail.transform.SetParent(m_TailTransform);
			_Tails.Add(tail);
		}
		else
		{
			if (_State.Tails.Count > 0)
			{
				var tailPosition = _State.Tails[_State.Tails.Count-1] + new Vector2(transform.position.x, transform.position.y);
				GameObject tail = Instantiate(m_Snake.gameObject, tailPosition, Quaternion.identity);
				tail.transform.SetParent(m_TailTransform);
				_Tails.Add(tail);
				var temp = _Tails[0];
				_Tails.RemoveAt(0);
				Destroy(temp.gameObject);
			}
		}
	}

	public Vector3 SetTransform(Vector2 _index)
	{
		return new Vector3(_index.x, _index.y, 0);
	}


	public override void CollectObservations()
    {
	 	AddVectorObs(_State.Head);
	 	AddVectorObs(_State.Food);
		var distance = _State.Head - _State.Food;
	 	AddVectorObs(Math.Abs(distance.x));
	 	AddVectorObs(Math.Abs(distance.y));
		AddVectorObs(_State.Tails.Count);
		
	
    }

    public override void AgentAction(float[] vectorAction, string textAction)
	{
		int action = Mathf.FloorToInt(vectorAction[0]);
		switch (action)
		{
			case 0:
				// Do notthing
				break;
			case 1:
				_Direction = State.DirectionType.Up;
				break;
			case 2:
				_Direction = State.DirectionType.Down;
				break;
			case 3:
				_Direction = State.DirectionType.Right;
				break;
			case 4:
				_Direction = State.DirectionType.Left;
				break;
		}
    }

    public override void AgentReset()
    {
		SetStateObject();
		RenderState();
		_IsPlay = true;
    }

    public override void AgentOnDone()
    {

    }
	private void SetStateObject()
	{
		var size = _SnakeAcademy.StateSize;
		var groundSize = (size/2f) - 0.5f;
		_State = new State(size, size);
		m_Ground.transform.position = new Vector3(groundSize + transform.position.x, groundSize + transform.position.y	, 0);
		m_Ground.transform.localScale = new Vector3(size, size, 0);
		if(_Tails != null)
		{
			for(int i = _Tails.Count - 1; i >= 0; i--)
			{
				var tail = _Tails[i];
				_Tails.RemoveAt(i);
				Destroy(tail.gameObject);
			}
		}
		_Tails = new List<GameObject>();
	}
}
