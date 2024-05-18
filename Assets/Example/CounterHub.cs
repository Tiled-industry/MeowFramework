using UnityEngine;

namespace Panty.Test
{
    public class CounterHub : ModuleHub<CounterHub>
    {
        protected override void BuildModule()
        {
            // ����ǵ�ע��ģ���ȥ
            AddModule<ICounterModel>(new CounterModel());
        }
    }
    public class CounterGame : MonoBehaviour, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => CounterHub.GetIns();
    }
    public class CounterUI : UIPanel, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => CounterHub.GetIns();
    }
}