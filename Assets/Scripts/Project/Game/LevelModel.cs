namespace Panty.Project
{
    public class LevelInfo
    {
        public int Num; // ����ĸ���
    }
    public interface ILevelModel : IModule
    {
        void SelectLevel(int level);
        LevelInfo Cur { get; } // ����һ����ȡ��ǰ�ؿ�������
    }
    //public class LevelModel : AbsModule, ILevelModel
    //{
    //    protected override void OnInit()
    //    {

    //    }
    //}
}