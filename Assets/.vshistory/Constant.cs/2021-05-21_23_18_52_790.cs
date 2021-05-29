﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public ControlPoint ControlPoint
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
	public enum EasingMode
	{
		None,           //1  0x01 0001
		Auto,           //2  0x02 0010
		EaseIn,
		EaseOut,
		EaseInOut,      //3  0x04 0100
		EaseInBack,
		EaseOutBack,
		EaseInOutBack,  //4  0x04 1000
	}
}