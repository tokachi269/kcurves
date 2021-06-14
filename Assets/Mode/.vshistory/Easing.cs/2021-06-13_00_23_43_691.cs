using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    [Flags]
    public enum EasingMode : byte
    {
        None = 0x01,          //1  0x01 00000001 指定可
        Auto = 0x04,          //4  0x04 00000100 指定可
        EaseInOut = 0x10,     //16 0x10 00010000 指定可
        EaseInOutBack = 0x40, //64 0x40 01000000 指定可

        //EaseIn,			  //
        //EaseOut,			  //
        //EaseInOut,		  //3  0x04 0100
        //EaseInBack,		  //
        //EaseOutBack,		  //
        //EaseInOutBack,	  //4  0x08 1000


        NoneNone = 0b00000011,
        NoneAuto = 0b00001001,
        NoneEaseInOut = 0b00100001,
        NoneEaseInOutBack = 0b10000001,
        AutoNone = 0b00000110,
        AutoAuto = 0b00001100,
        AutoEaseInOut = 0b00100100,
        AutoEaseInOutBack = 0b10000100,
        EaseInOutNone = 0b00010010,
        EaseInOutAuto = 0b00011000,
        EaseInOutEaseInOut = 0b00110000,
        EaseInOutEaseInOutBack = 0b10010000,
        EaseInOutBackNone = 0b01000010,
        EaseInOutBackAuto = 0b01001000,
        EaseInOutBackEaseInOut = 0b01100000,
        EaseInOutBackEaseInOutBack = 0b11000000
    }


    class Easing
    {
        //Backが始値と終値の差分の10％となる値
        private const float EasingBack = 1.70158f;

        public static float GetEasing(EasingMode mode, float rate)
        {
            float t;
            float rate = num / denom;
            switch (mode)
            {
                case EasingMode.NoneNone:
                    t = rate;
                    break;
                case EasingMode.NoneAuto:
                    t = EaseOutSine(rate);
                    break;
                case EasingMode.NoneEaseInOut:
                    t = EaseOutSine(rate);
                    break;
                case EasingMode.NoneEaseInOutBack:
                    t = EaseOutBackSin(rate);
                    break;
                case EasingMode.AutoNone:
                    t = EaseInSine(rate);
                    break;
                case EasingMode.AutoAuto:
                    t = rate;
                    //t = EaseInOutSine(rate);
                    break;
                case EasingMode.AutoEaseInOut:
                    t = EaseInOutSine(rate);
                    break;
                case EasingMode.AutoEaseInOutBack:
                    t = EaseInOutBackSin(rate);
                    break;
                case EasingMode.EaseInOutNone:
                    t = EaseInSine(rate);
                    break;
                case EasingMode.EaseInOutAuto:
                    t = EaseInOutSine(rate);
                    break;
                case EasingMode.EaseInOutEaseInOut:
                    t = EaseInOutSine(rate);
                    break;
                case EasingMode.EaseInOutEaseInOutBack:
                    t = EaseInOutSine(rate);
                    break;
                case EasingMode.EaseInOutBackNone:
                    t = EaseInBackSin(rate);
                    break;
                case EasingMode.EaseInOutBackAuto:
                    t = EaseInOutBackSin(rate);
                    break;
                case EasingMode.EaseInOutBackEaseInOut:
                    t = EaseInOutSine(rate);
                    break;
                case EasingMode.EaseInOutBackEaseInOutBack:
                    t = EaseInOutBackSin(rate);
                    break;
                default:
                    t = rate;
                    break;
            }
            return t * denom;
        }

        public static float EaseInOutSine(float t)
        {
            return (float)(-(Math.Cos(Math.PI * t) - 1) / 2);
        }

        public static float EaseInSine(float t)
        {
            return (float)(1 - Math.Cos((Math.PI * t) / 2));
        }

        public static float EaseOutSine(float t)
        {
            return (float)Math.Sin((Math.PI * t) / 2);
        }

        private static float EaseInOutBackSin(float t)
        {
            float num = EasingBack * 1.525f;

            return (float)(t < 0.5
              ? (Math.Pow(2 * t, 2) * ((num + 1) * 2 * t - num)) / 2
              : (Math.Pow(2 * t - 2, 2) * ((num + 1) * (t * 2 - 2) + num) + 2) / 2);
        }

        private static float EaseOutBackSin(float t)
        {
            float num = EasingBack + 1;

            return (float)(1 + num * Math.Pow(t - 1, 3) + EasingBack * Math.Pow(t - 1, 2));
        }

        private static float EaseInBackSin(float t)
        {
            float num = EasingBack + 1;

            return num * t * t * t - EasingBack * t * t;
        }

        private static void EasingModeFlagGenerater(){
            for (byte n = 0; n <= 3; n++)
            {
                for (byte m = 0; m <= 3; m++)
                {
                    Debug.Log(Convert.ToString((byte)Math.Pow(4, n) | ((byte)Math.Pow(4, m) << 1), 2).PadLeft(8, '0') + " : " + (EasingMode)(Math.Pow(4, n)) + "," + (EasingMode)(Math.Pow(4, m)));
                }
            }
        }      
    }
}
