using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seiro.Splines
{

	public class Line : MonoBehaviour
	{

		[SerializeField]
		Vector3 _p0, _p1;

		public Vector3 p0
		{
			get
			{
				return _p0;
			}
			set
			{
				_p0 = value;
			}
		}
		public Vector3 p1
		{
			get
			{
				return _p1;
			}
			set
			{
				_p1 = value;
			}
		}
	}
}