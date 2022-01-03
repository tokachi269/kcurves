using System.Collections.Generic;
using UnityEngine;

namespace CameraOperator.Tool
{
    internal class PlayAnimation
    {
        private ExtendBezierControls Positions;
        private int currentKnotIndex = 0;
        private int currentBezierIndex = 0;
        public float PositionBetweenRange = 0f;
        private float MaxSpeed = 0;
        private EasingMode mode;

        public PlayAnimation(ExtendBezierControls positions, float maxSpeed)
        {
            this.Positions = positions;
            this.MaxSpeed = maxSpeed;
        }
        public void Play(ref List<CameraConfig> tempKnots, ref EasingMode[] easingMode, ref float progressLength,ref int targetKnotIndex,ref int targetBezierIndex, bool isReturn)
        {
            Play(ref tempKnots, ref easingMode, ref progressLength, targetKnotIndex, targetBezierIndex);
            if(isReturn)
            {
                targetKnotIndex = currentKnotIndex;
                targetBezierIndex = currentBezierIndex;
            }
        }

        private void Play(ref List<CameraConfig> tempKnots,ref EasingMode[] easingMode, ref float progressLength,int targetKnotIndex,int targetBezierIndex)
        {  
            //次のセグメントに移動したか判定する
            bool isSegChanged = false;
            if (targetBezierIndex == 0 || targetBezierIndex == Positions.SegmentCount)
            {
                if (progressLength >= Positions.Length(targetBezierIndex))
                {
                    progressLength -= Positions.Length(targetBezierIndex);
                    isSegChanged = true;
                }
            }
            else if (targetBezierIndex % 2 == 1)
            {
                if (progressLength >= Positions.Length(targetBezierIndex))
                {
                    isSegChanged = true;
                }
            }
            else
            {
                if (progressLength >= Positions.Length(targetBezierIndex - 1) + Positions.Length(targetBezierIndex))
                {
                    progressLength -= Positions.Length(targetBezierIndex - 1) + Positions.Length(targetBezierIndex);
                    isSegChanged = true;
                }
            }

            //セグメントが移動した場合、計算に必要なパラメーターを設定する
            if (isSegChanged)
            {
                targetBezierIndex++;

                targetKnotIndex = targetBezierIndex % 2 == 0 ? (targetBezierIndex / 2) : (targetBezierIndex + 1) / 2;
                //mode = EasingMode.None;
                mode = (EasingMode)((byte)easingMode[targetKnotIndex + 1] | ((byte)easingMode[targetKnotIndex] << 1));
                PositionBetweenRange = 0f;

                if (targetKnotIndex == tempKnots.Count - 1)
                {
                    PositionBetweenRange = Positions.Length(targetBezierIndex);
                }
                else
                {
                    for (ushort j = (ushort)(2 * targetKnotIndex - 1); j < Positions.SegmentCount && j <= 2 * targetKnotIndex; j++)
                    {
                        PositionBetweenRange += Positions.Length(j);
                    }
                }
            }

            currentKnotIndex = targetKnotIndex;
            currentBezierIndex = targetBezierIndex ;

        }
    }
}
