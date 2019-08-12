using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Snake.Script.Controller
{
	public class MenuController : MonoBehaviour {

	[SerializeField] private Button m_Play;
	void Start () {
		m_Play.OnClickAsObservable().Subscribe
		(
			_ => SceneManager.LoadSceneAsync("02.gameplay")
		).AddTo(this);
	}
}
}
