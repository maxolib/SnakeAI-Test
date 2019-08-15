using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Snake.Scripts.Utilities;
using MLAgents;
using UnityEngine.UI;
using TMPro;

public class SnakeAgent : Agent {
	// -------------------------------------------------------------------------------------------------
	private const float DURATION_NEXT_TURN = 0.01f;
	[SerializeField] private GameObject m_Ground;
	[SerializeField] private GameObject m_Snake;
	[SerializeField] private GameObject m_Food;
	[SerializeField] private Transform m_TailTransform;
	[SerializeField] private Canvas m_UI;
	[SerializeField] private TextMeshProUGUI m_ScoreText;

	// -------------------------------------------------------------------------------------------------
	private SnakeAcademy _SnakeAcademy;
	private int _Size;
	private bool _IsPlay;
	private State _State;
	private float _Timer;
	private IntReactiveProperty _score = new IntReactiveProperty();
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
		
		Observable.EveryUpdate().Where(_ => _Timer > _SnakeAcademy.resetParameters["Speed"]).Subscribe
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
					_score.SetValueAndForceNotify(_score.Value + 1);
				}
				RenderState();
			}
		).AddTo(this);

		_score.Subscribe
		(
			_value =>
			{
				m_ScoreText.text = _value + "";
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
		/*
		SnakeBrainLearing
	 	AddVectorObs(_State.Head);
	 	AddVectorObs(_State.Food);
	 	AddVectorObs(Math.Abs(headX - foodX));
	 	AddVectorObs(Math.Abs(headY - foodY));
		AddVectorObs(_State.Tails.Count);
		*/


		// Snake10x10Learning
		var headX = _State.Head.x / _State.Width;
		var headY = _State.Head.y / _State.Height;
		var foodX = _State.Food.x / _State.Width;
		var foodY = _State.Food.y / _State.Height;
	 	AddVectorObs(headX);
	 	AddVectorObs(headY);
	 	AddVectorObs(foodX);
	 	AddVectorObs(foodY);
	 	AddVectorObs(Math.Abs(headX - foodX));
	 	AddVectorObs(Math.Abs(headY - foodY));
		AddVectorObs(_State.Tails.Count);

		// Obs State
		for(int i = 0; i < _State.Width; i++)
		{
			for(int j = 0; j < _State.Height; j++)
			{
				AddVectorObs(_State.StateBlock[i, j].GetHashCode() / 5f);
			}
		}
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
		var size = (int) _SnakeAcademy.resetParameters["StateSize"];
		var groundSize = (size/2f) - 0.5f;
		_State = new State(size, size);
		m_Ground.transform.position = new Vector3(groundSize + transform.position.x, groundSize + transform.position.y	, 0);
		m_Ground.transform.localScale = new Vector3(size, size, 0);
		m_UI.transform.localPosition = new Vector3(groundSize, groundSize, 0);
		m_UI.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
		_score.SetValueAndForceNotify(0);
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
