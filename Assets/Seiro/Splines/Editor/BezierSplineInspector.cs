using UnityEngine;
using UnityEditor;

namespace Seiro.Splines
{

	/// <summary>
	/// ベジェスプラインのエディタスクリプト
	/// </summary>
	[CustomEditor(typeof(BezierSpline))]
	public class BezierSplineInspector : Editor
	{

		readonly int _stepPerCurve = 10;        // 曲線ごとの方向ベクトルの描画ステップ数
		readonly float _directionScale = 0.5f;  // 方向ベクトルのサイズ
		readonly float _handleSize = 0.04f;     // ボタンハンドルのサイズ
		readonly float _pickSize = 0.06f;       // ボタンハンドルの判定サイズ

		static readonly Color[] _modeColor = {
		Color.white,
		Color.yellow,
		Color.cyan
	};

		BezierSpline _spline;           // 対象のBezierSpline
		Transform _handleTransform;     // 対象のBezierSplineの姿勢
		Quaternion _handleRotation;     // 対象のBezierSplineの回転
		int _selectedIndex = -1;        // 選択されている頂点の番号

		public override void OnInspectorGUI()
		{
			_spline = target as BezierSpline;

			// Loopのトグル
			EditorGUI.BeginChangeCheck();
			bool loop = EditorGUILayout.Toggle("Loop", _spline.loop);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(_spline, "Toggle Loop");
				EditorUtility.SetDirty(_spline);
				_spline.loop = loop;
			}
			// 選択されている頂点についてインスペクターに表示する
			if (0 <= _selectedIndex && _selectedIndex < _spline.controlPointCount)
			{
				DrawSelectedPointInspector();
			}

			// 曲線追加ボタン
			if (GUILayout.Button("Add Curve"))
			{
				Undo.RecordObject(_spline, "Add Curve");
				EditorUtility.SetDirty(_spline);
				_spline.AddCurve();

			}
		}

		private void OnSceneGUI()
		{

			// 対象とその姿勢、回転の取得
			_spline = target as BezierSpline;
			_handleTransform = _spline.transform;
			_handleRotation = Tools.pivotRotation == PivotRotation.Local ?
								   _handleTransform.rotation : Quaternion.identity;

			Vector3 p0 = ShowPoint(0);
			for (int i = 1; i < _spline.controlPointCount; i += 3)
			{
				Vector3 p1 = ShowPoint(i);
				Vector3 p2 = ShowPoint(i + 1);
				Vector3 p3 = ShowPoint(i + 2);

				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);

				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;
			}
			ShowDirections();
		}

		/// <summary>
		/// 指定した番号の制御点の表示。座標を返す
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="index">Index.</param>
		private Vector3 ShowPoint(int index)
		{
			Vector3 point = _handleTransform.TransformPoint(_spline.GetControlPoint(index));

			// 大きさを奥行きに依存させないようにするための倍率を取得
			float size = HandleUtility.GetHandleSize(point);
			if (index == 0)
			{
				size += 2f;
			}
			Handles.color = _modeColor[(int)_spline.GetControlPointMode(index)];

			// 制御点の描画
			if (Handles.Button(point, _handleRotation, _handleSize * size, _pickSize * size, Handles.DotHandleCap))
			{
				_selectedIndex = index;
				// インスペクタは変更されないので再描画要求を行う
				Repaint();
			}

			// 選択している制御てんの場合はハンドルを描画
			if (_selectedIndex == index)
			{
				EditorGUI.BeginChangeCheck();
				point = Handles.DoPositionHandle(point, _handleRotation);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(_spline, "Move Point");
					EditorUtility.SetDirty(_spline);
					_spline.SetControlPoint(index, _handleTransform.InverseTransformPoint(point));
				}
			}
			return point;
		}

		/// <summary>
		/// 方向ベクトルの表示
		/// </summary>
		private void ShowDirections()
		{
			Vector3 point = _spline.GetPoint(0f);
			Handles.color = Color.green;
			Handles.DrawLine(point, point + _spline.GetDirection(0f) * _directionScale);
			int steps = _stepPerCurve * _spline.curveCount;
			for (int i = 1; i <= steps; ++i)
			{
				point = _spline.GetPoint(i / (float)steps);
				Handles.DrawLine(point, point + _spline.GetDirection(i / (float)steps) * _directionScale);
			}
		}

		/// <summary>
		/// 選択している制御点の情報をインスペクタに表示
		/// </summary>
		private void DrawSelectedPointInspector()
		{
			//選択している制御てんの情報を表示
			GUILayout.Label("Selected Point");
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", _spline.GetControlPoint(_selectedIndex));
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(_spline, "Move Point");
				EditorUtility.SetDirty(_spline);
				_spline.SetControlPoint(_selectedIndex, point);
			}

			// 選択している制御点モードの表示
			EditorGUI.BeginChangeCheck();
			BezierControlPointMode mode = (BezierControlPointMode)
				EditorGUILayout.EnumPopup("Mode", _spline.GetControlPointMode(_selectedIndex));
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(_spline, "Change Point Mode");
				_spline.SetControlPointMode(_selectedIndex, mode);
				EditorUtility.SetDirty(_spline);
			}
		}
	}
}