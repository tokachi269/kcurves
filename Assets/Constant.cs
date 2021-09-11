using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace Assets
{
	public class ApplyItems
	{
		[DefaultValue(true)]
		public bool position { get; set; }

		[DefaultValue(true)]
		public bool rotation { get; set; }

		[DefaultValue(true)]
		public bool fov { get; set; }

		[DefaultValue(true)]
		public bool LookAt { get; set; }

		public CameraConfig ControlPoint
		{
			get => default;
			set
			{
			}
		}

		public ApplyItems(bool position, bool rotation, bool fov)
		{
			this.position = position;
			this.rotation = rotation;
			this.fov = fov;
		}
	}
}