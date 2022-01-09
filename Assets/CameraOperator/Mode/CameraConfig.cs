
using UnityEngine;
using System.ComponentModel;

namespace CameraOperator.Tool
{
	public class CameraConfig : MonoBehaviour
	{
		public Vector3 position;

		public Quaternion rotation;

		public float Length;

		public float size;

		public float height;

		[DefaultValue(2f)]
		public float duration = 2f;

		[DefaultValue(0f)]
		public float delay;

		[DefaultValue(45f)]
		public float fov;
		public float time = 2f;
		public ApplyItems applyItems = new ApplyItems(true, true, true);

		[DefaultValue(EasingMode.Auto)]
		public EasingMode easingMode;
		public bool isLookAt;

		public CameraConfig(Vector3 position, Quaternion rotation, float fov ,bool? isLookAt = null)
		{
			this.position = position;
			this.rotation = rotation;
			this.fov = fov;
			//this.CaptureCamera();
			this.easingMode = EasingMode.Auto;
			delay = 0f;
			this.isLookAt = isLookAt != null && (bool)isLookAt;
		}

		//public void CaptureCamera()
		//{
		//	this.position = ToolController.cameraController.m_currentPosition;
		//	this.size = ToolController.cameraController.m_currentSize;
		//	this.height = ToolController.cameraController.m_currentHeight;
		//	this.fov = ToolController.camera.fieldOfView;
		//	float num = this.size * (1f - this.height / ToolController.cameraController.m_maxDistance) / Mathf.Tan(0.017453292f * this.fov);
		//	Vector2 currentAngle = ToolController.cameraController.m_currentAngle;
		//	this.rotation = Quaternion.AngleAxis(currentAngle.x, Vector3.up) * Quaternion.AngleAxis(currentAngle.y, Vector3.right);
		//	Vector3 worldPos = this.position + this.rotation * new Vector3(0f, 0f, -num);
		//	this.position.y = this.position.y + Knot.CalculateCameraHeightOffset(worldPos, num);
		//}

	}
}
