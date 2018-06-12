using UnityEditor;
using UnityEngine;

namespace Seiro.Splines
{

	/// <summary>
	/// 線の描画
	/// </summary>
	[CustomEditor(typeof(Line))]
	public class LineInspector : Editor
	{

		private void OnSceneGUI()
		{
			Line line = target as Line;
			Transform handlesTransform = line.transform;

			//操作モードに応じて変更する
			Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ?
											 handlesTransform.rotation : Quaternion.identity;

			//点をtransformのモデル行列で変換
			Vector3 p0 = handlesTransform.TransformPoint(line.p0);
			Vector3 p1 = handlesTransform.TransformPoint(line.p1);

			//線の描画
			Handles.color = Color.white;
			Handles.DrawLine(p0, p1);

			//ハンドルの描画と変更の検出
			EditorGUI.BeginChangeCheck();
			p0 = Handles.DoPositionHandle(p0, handleRotation);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(line, "Move Point");
				line.p0 = handlesTransform.InverseTransformPoint(p0);
			}
			EditorGUI.BeginChangeCheck();
			p1 = Handles.DoPositionHandle(p1, handleRotation);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(line, "Move Point");
				line.p1 = handlesTransform.InverseTransformPoint(p1);
			}
		}
	}
}