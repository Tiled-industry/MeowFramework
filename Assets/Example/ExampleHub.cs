using UnityEngine;

namespace Panty.Test
{
    public class ExampleHub : ModuleHub<ExampleHub>
    {
        protected override void BuildModule()
        {
            // �Ƽ�ʹ�� MonoKit �� OnDeInit�¼� ����������
            MonoKit.GetIns().OnDeInit += Deinit;
        }
    }
    public class ExampleGame : MonoBehaviour, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => ExampleHub.GetIns();
    }
    public class ExampleUI : UIPanel, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => ExampleHub.GetIns();
    }
}