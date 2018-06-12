using UnityEngine;
using System;

namespace Seiro.Splines
{

	/// <summary>
	/// 複数の曲線の集まり
	/// </summary>
	public class BezierSpline : MonoBehaviour
	{

		[SerializeField]
		Vector3[] _points;
		[SerializeField]
		BezierControlPointMode[] _modes;
		[SerializeField]
		bool _loop;

		/// <summary>
		/// 曲線の数を返す
		/// </summary>
		/// <value>The curve count.</value>
		public int curveCount
		{
			get
			{
				return _points.Length / 3;
			}
		}

		/// <summary>
		/// 制御点の数を返す
		/// </summary>
		/// <value>The control point count.</value>
		public int controlPointCount
		{
			get
			{
				return _points.Length;
			}
		}

		public bool loop
		{
			get
			{
				return _loop;
			}
			set
			{
				_loop = value;
				if (value == true)
				{
					_modes[_modes.Length - 1] = _modes[0];
					SetControlPoint(0, _points[0]);
				}
			}
		}

		public void Reset()
		{
			_points = new Vector3[] {
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};

			_modes = new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		};
		}

		/// <summary>
		/// 指定した番号の制御点の取得
		/// </summary>
		/// <returns>The control point.</returns>
		/// <param name="index">Index.</param>
		public Vector3 GetControlPoint(int index)
		{
			return _points[index];
		}

		/// <summary>
		/// 指定した番号の制御点の値を設定する
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="point">Point.</param>
		public void SetControlPoint(int index, Vector3 point)
		{
			// 制御点の中央を移動する場合
			if (index % 3 == 0)
			{
				Vector3 delta = point - _points[index];
				if (_loop)
				{
					// ループの場合は始点と終点を動かす
					if (index == 0)
					{
						_points[1] += delta;
						_points[_points.Length - 2] += delta;
						_points[_points.Length - 1] = point;
					}
					else if (index == _points.Length - 1)
					{
						_points[0] = point;
						_points[1] += delta;
						_points[index - 1] += delta;
					}
					else
					{
						_points[index - 1] += delta;
						_points[index + 1] += delta;
					}
				}
				else
				{
					if (index > 0)
					{
						_points[index - 1] += delta;
					}
					if (index + 1 < _points.Length)
					{
						_points[index + 1] += delta;
					}
				}
			}

			_points[index] = point;
			EnforceMode(index);

		}

		/// <summary>
		/// 指定した番号に対応する制御点モードを取得する
		/// </summary>
		/// <returns>The control point mode.</returns>
		/// <param name="index">Index.</param>
		public BezierControlPointMode GetControlPointMode(int index)
		{
			return _modes[(index + 1) / 3];
		}

		/// <summary>
		/// 指定した番号に対応する制御点モードを設定する
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="mode">Mode.</param>
		public void SetControlPointMode(int index, BezierControlPointMode mode)
		{
			int modeIndex = (index + 1) / 3;
			_modes[modeIndex] = mode;
			if (_loop)
			{
				if (modeIndex == 0)
				{
					_modes[_modes.Length - 1] = mode;
				}
				else if (modeIndex == _modes.Length - 1)
				{
					_modes[0] = mode;
				}
			}
			EnforceMode(index);
		}

		/// <summary>
		/// 指定したインデックスのモードに合わせて対応する制御点の座標を矯正する
		/// </summary>
		/// <returns>The enforcemode.</returns>
		/// <param name="index">Index.</param>
		private void EnforceMode(int index)
		{
			int modeIndex = (index + 1) / 3;
			BezierControlPointMode mode = _modes[modeIndex];

			// modeがFreeの場合またはループしていない始点、終点の場合は何もしない
			if (mode == BezierControlPointMode.Free || !_loop && (modeIndex == 0 || modeIndex == _modes.Length - 1))
			{
				return;
			}

			// 選択した点がどの位置の制御点なのか計算する
			// fixedIndexが選択したインデックス
			int middleIndex = modeIndex * 3;
			int fixedIndex, enforcedIndex;
			if (index <= middleIndex)
			{
				fixedIndex = middleIndex - 1;
				if (fixedIndex < 0)
				{
					fixedIndex = _points.Length - 2;
				}
				enforcedIndex = middleIndex + 1;
				if (enforcedIndex >= _points.Length)
				{
					enforcedIndex = 1;
				}
			}
			else
			{
				fixedIndex = middleIndex + 1;
				if (fixedIndex >= _points.Length)
				{
					fixedIndex = 1;
				}
				enforcedIndex = middleIndex - 1;
				if (enforcedIndex < 0)
				{
					enforcedIndex = _points.Length - 2;
				}
			}

			// middleIndexの座標とfixedIndexの座標の差分を求める
			Vector3 middle = _points[middleIndex];
			Vector3 enforcedTangent = middle - _points[fixedIndex];
			if (mode == BezierControlPointMode.Aligned)
			{
				enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, _points[enforcedIndex]);
			}
			_points[enforcedIndex] = middle + enforcedTangent;
		}

		/// <summary>
		/// 指定した割合に対応する座標を取得する
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="t">T.</param>
		public Vector3 GetPoint(float t)
		{
			int i = GetCurveStartIndex(ref t);
			return transform.TransformPoint(
				Bezier.GetPoint(_points[i], _points[i + 1], _points[i + 2], _points[i + 3], t));
		}

		/// <summary>
		/// 指定した割合に対応する座標の加速度を取得する
		/// </summary>
		/// <returns>The velocity.</returns>
		/// <param name="t">T.</param>
		public Vector3 GetVelocity(float t)
		{
			int i = GetCurveStartIndex(ref t);
			return transform.TransformPoint(
				Bezier.GetFirstDerivative(_points[i], _points[i + 1], _points[i + 2], _points[i + 3], t)) - transform.position;
		}

		/// <summary>
		/// 指定した割合に対応する方向を取得する
		/// </summary>
		/// <returns>The direction.</returns>
		/// <param name="t">T.</param>
		public Vector3 GetDirection(float t)
		{
			return GetVelocity(t).normalized;
		}

		/// <summary>
		/// 指定した割合に対応する曲線の開始番号を取得する
		/// また割合を全体からその曲線内での割合に変更する
		/// </summary>
		/// <returns>The curve start index.</returns>
		/// <param name="t">T.</param>
		private int GetCurveStartIndex(ref float t)
		{
			int i;
			if (t >= 1f)
			{
				t = 1f;
				i = _points.Length - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * curveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}
			return i;
		}

		/// <summary>
		/// 曲線の追加
		/// </summary>
		public void AddCurve()
		{
			int len = _points.Length;
			Vector3 point = _points[len - 1];
			Array.Resize(ref _points, _points.Length + 3);
			len = _points.Length;
			point.x += 1f;
			_points[len - 3] = point;
			point.x += 1f;
			_points[len - 2] = point;
			point.x += 1f;
			_points[len - 1] = point;

			Array.Resize(ref _modes, _modes.Length + 1);
			_modes[_modes.Length - 1] = _modes[_modes.Length - 2];
			EnforceMode(_points.Length - 4);

			// ループの場合は追加した終点を始点と同期させる
			if (_loop)
			{
				_points[_points.Length - 1] = _points[0];
				_modes[_modes.Length - 1] = _modes[0];
				EnforceMode(0);
			}
		}
	}
}