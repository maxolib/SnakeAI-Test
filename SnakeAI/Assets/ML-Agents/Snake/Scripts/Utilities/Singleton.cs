using UnityEngine;

namespace Snake.Scripts.Utilities
{
	public class Singleton<T> : MonoBehaviour
	{
		//-----------------------------------------------------------------

		public static T Instance;
	
		//-----------------------------------------------------------------

		protected virtual void Awake() => Instance    =   GetComponent<T>();

		//-----------------------------------------------------------------
	}
}