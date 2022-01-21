using UnityEngine;
using System.ComponentModel;

namespace CamOpr.Tool
{
	public class CameraConfig
	{
		public int ListIndex = -1;

		//カメラの座標
		public Vector3 Position;

		//向き
		public Quaternion Rotation;

		//ユーザー制御点間の長さ
		public float Length;

		public float Size;

		public float Height;


		[DefaultValue(2f)]
		public float Duration= 2f;

		//一時停止時間
		[DefaultValue(0f)]
		public float Delay;

		//画角
		[DefaultValue(45f)]
		public float Fov;

		[DefaultValue(2f)]
		public float Time = 2f;

		//座標、向き、画角の適用設定
		public ApplyItems ApplyItems = new ApplyItems(true, true, true);

		//Easing設定
		[DefaultValue(EasingMode.Auto)]
		public EasingMode EasingMode;

		//カメラの注視設定
		public bool IsLookAt;

		public CameraConfig(Vector3 position, Quaternion rotation, float fov, bool? isLookAt = null)
		{
			this.Position = position;
			this.Rotation = rotation;
			this.Fov = fov;
			//this.CaptureCamera();
			this.EasingMode = EasingMode.Auto;
			Delay = 0f;
			this.IsLookAt = isLookAt != null && (bool)isLookAt;
		}


	}
}
