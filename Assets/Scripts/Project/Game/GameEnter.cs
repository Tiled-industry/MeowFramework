using UnityEngine;

namespace Panty.Project
{
    //public static class A
    //{
    //    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //    private static void Run()
    //    {

    //    }
    //}
    public class GameEnter : ProjectGame
    {
        private void Start()
        {
            this.Module<ILevelModel>().SelectLevel(1);
            // ������ص����� ��ò�Ҫ����mono����
            // ��Ҫ���������� ��ϵͳ��ģ����� GridSystem
            this.SendCmd<InitMapCmd>();
        }
    }
}