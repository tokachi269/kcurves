using System;
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

	[Flags]
	public enum EasingMode : byte
	{
		None= 0x04,           //1  0x04 00000001 指定可
		Auto = 0x10,          //4  0x10 00000100 指定可
		EaseInOut = 0x20,     //16 0x20 00010000 指定可
		EaseInOutBack = 0x40, //64 0x40 01000000 指定可

		//EaseIn,			  //
		//EaseOut,			  //
		//EaseInOut,		  //3  0x04 0100
		//EaseInBack,		  //
		//EaseOutBack,		  //
		//EaseInOutBack,	  //4  0x08 1000
		//None, Auto100100
//UnityEngine.Debug:Log (object)
//Assets.CameraDirector:Update () (at Assets/CameraDirector.cs:140)
//
//EaseInOut, None101000
//UnityEngine.Debug:Log (object)
//Assets.CameraDirector:Update () (at Assets/CameraDirector.cs:141)
//
//EaseInOutBack, None1001000
//UnityEngine.Debug:Log (object)
//Assets.CameraDirector:Update () (at Assets/CameraDirector.cs:142)


	}
}