using UnityEngine;
using System.Collections;

namespace Seiro.Splines
{

	/// <summary>
	/// スプラインに沿って移動する
	/// </summary>
	public class SplineWalker : MonoBehaviour
	{

		[SerializeField]
		BezierSpline _spline;
		[SerializeField, Range(1f, 60f)]
		float _duration = 10f;
		[SerializeField]
		bool _lookForward;
		[SerializeField]
		SplineWalkerMode _mode;

		bool _goingForward = true;

		float _progress;

		private void Update()
		{
			if (_goingForward)
			{
				_progress += Time.deltaTime / _duration;
				if (_progress > 1f)
				{
					if (_mode == SplineWalkerMode.Once)
					{
						_progress = 1f;
					}
					else if (_mode == SplineWalkerMode.Loop)
					{
						_progress = 0f;
					}
					else
					{
						_progress = 2f - _progress;
						_goingForward = false;
					}
				}
			}
			else
			{
				_progress -= Time.deltaTime / _duration;
				if (_progress < 0f)
				{
					_progress = -_progress;
					_goingForward = true;
				}
			}
			Vector3 position = _spline.GetPoint(_progress);
			transform.localPosition = position;
			if (_lookForward)
			{
				transform.LookAt(position + _spline.GetDirection(_progress));
			}
		}
	}
}