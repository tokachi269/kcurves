namespace CameraOperator.Tool
{
    internal class PlayAnimation
    {
        private ExtendBezierControls positions;
        int knotIndex = 0;
        int bezierIndex = 0;
        float PositionBetweenRange = 0f;
        EasingMode mode;
        public PlayAnimation(ExtendBezierControls positions)
        {
            this.positions = positions;
        }
    }
}