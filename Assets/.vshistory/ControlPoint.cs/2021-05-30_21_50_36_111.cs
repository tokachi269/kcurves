using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.ComponentModel;

namespace Assets
{
	public class ControlPoint : MonoBehaviour
	{
		public Vector3 position;

		public Quaternion rotation;

		public float t;

		public float Length;

		public float size;

		public float height;

		[DefaultValue(2f)]
		public float duration = 2f;

		[DefaultValue(0f)]
		public float delay;

		[DefaultValue(45f)]
		public float fov;

		public ApplyItems applyItems = new ApplyItems(true, true, true);

		public bool isLookAt { get; set; }

		[DefaultValue(EasingMode.Auto)]
		public EasingMode easingMode;

		public ControlPoint(Vector3 position, Quaternion rotation, float fov, bool isLookAt)
		{
			this.position = position;
			this.rotation = rotation;
			this.fov = fov;
			//this.CaptureCamera();
			this.isLookAt = isLookAt;
			this.easingMode = EasingMode.Auto;
		}




		//public void CaptureCamera()
		//{
		//	this.position = CameraDirector.cameraController.m_currentPosition;
		//	this.size = CameraDirector.cameraController.m_currentSize;
		//	this.height = CameraDirector.cameraController.m_currentHeight;
		//	this.fov = CameraDirector.camera.fieldOfView;
		//	float num = this.size * (1f - this.height / CameraDirector.cameraController.m_maxDistance) / Mathf.Tan(0.017453292f * this.fov);
		//	Vector2 currentAngle = CameraDirector.cameraController.m_currentAngle;
		//	this.rotation = Quaternion.AngleAxis(currentAngle.x, Vector3.up) * Quaternion.AngleAxis(currentAngle.y, Vector3.right);
		//	Vector3 worldPos = this.position + this.rotation * new Vector3(0f, 0f, -num);
		//	this.position.y = this.position.y + Knot.CalculateCameraHeightOffset(worldPos, num);
		//}

	}
}
