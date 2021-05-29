using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets
{
    [Flags]
    public enum EasingMode : byte
    {
        None = 0x01,           //1  0x01 00000001 指定可
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
        public static float GetEasing(EasingMode mode)
        {
            float num = 0;
            switch (mode)
            {
                case EasingMode.NoneNone:
                    break;
                case EasingMode.NoneAuto:
                    num = EaseOutSine();
                    break;
                case EasingMode.NoneEaseInOut:
                    num = EaseOutSine();
                    break;
                case EasingMode.NoneEaseInOutBack:
                    num = EaseOutBackSin();
                    break;
                case EasingMode.AutoNone:
                    num = EaseInSine();
                    break;
                case EasingMode.AutoAuto:
                    num = EaseInOutSine();
                    break;
                case EasingMode.AutoEaseInOut:
                    num = EaseInOutSine();
                    break;
                case EasingMode.AutoEaseInOutBack:
                    num = EaseInOutBackSin();
                    break;
                case EasingMode.EaseInOutNone:
                    num = EaseInSine();
                    break;
                case EasingMode.EaseInOutAuto:
                    num = EaseInOutSine();
                    break;
                case EasingMode.EaseInOutEaseInOut:
                    num = EaseInOutSine();
                    break;
                case EasingMode.EaseInOutEaseInOutBack:
                    num = EaseInOutSine();
                    break;
                case EasingMode.EaseInOutBackNone:
                    num = EaseInBackSin();
                    break;
                case EasingMode.EaseInOutBackAuto:
                    num = EaseInOutBackSin();
                    break;
                case EasingMode.EaseInOutBackEaseInOut:
                    num = EaseInOutSine();
                    break;
                case EasingMode.EaseInOutBackEaseInOutBack:
                    num = EaseInOutBackSin();
                    break;

            }
            return num;
        }
    }
}
