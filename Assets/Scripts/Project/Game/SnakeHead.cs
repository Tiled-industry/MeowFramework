using UnityEngine;

namespace Panty.Project
{
    public struct SnakeMoveEvent
    {

    }
    public struct SnakeBiggerEvent
    {

    }
    public class SnakeHead : ProjectGame
    {
        private PArray<SnakeBody> mBodys;

        private void Start()
        {
            mBodys = new PArray<SnakeBody>();

            this.AddEvent<SnakeBiggerEvent>(OnBigger).RmvOnDestroy(this);
            this.AddEvent<SnakeMoveEvent>(OnMove).RmvOnDestroy(this);
            this.Module<ITaskScheduler>().AddDelayTask(0.3f, this.SendCmd<SnakeMotionCmd>, true);
        }
        private void OnMove(SnakeMoveEvent e)
        {
            // �޸�λ�� �޸���ת
        }
        private void OnBigger(SnakeBiggerEvent e)
        {

        }
    }
}