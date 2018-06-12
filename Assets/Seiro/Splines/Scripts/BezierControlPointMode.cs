namespace Seiro.Splines
{
	/// <summary>
	/// ベジェ曲線の制御点のモード
	/// </summary>
	public enum BezierControlPointMode
	{
		Free,       //今までの
		Aligned,    //軸合わせ
		Mirrored    //反対の軸と対称
	}
}