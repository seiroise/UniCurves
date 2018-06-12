using UnityEngine;
using System.Collections;

namespace Seiro.Splines
{

	public class SplineDecorator : MonoBehaviour
	{

		[SerializeField]
		BezierSpline _spline;
		[SerializeField]
		int _frequency;
		[SerializeField]
		bool _lookForward;
		[SerializeField]
		Transform[] _items;

		void Awake()
		{
			if (_frequency <= 0 || _items == null || _items.Length == 0)
			{
				return;
			}
			float stepSize = 1f / (_frequency * _items.Length);
			for (int p = 0, f = 0; f < _frequency; ++f)
			{
				for (int i = 0; i < _items.Length; ++i, ++p)
				{
					Transform item = Instantiate(_items[i]);
					Vector3 position = _spline.GetPoint(p * stepSize);
					item.transform.localPosition = position;
					if (_lookForward)
					{
						item.transform.LookAt(position + _spline.GetDirection(p * stepSize));
					}
					item.transform.SetParent(transform);
				}
			}
		}
	}
}