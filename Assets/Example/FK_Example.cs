using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Panty.Test
{
    public interface IMyModule : IModule
    {
        void Say();
        void Say(string msg);
        string Msg { get; }
        string Get(int id);
    }
    public interface IMyUtility : IUtility
    {
        void Use();
    }
    public class MyUtility : IMyUtility
    {
        void IMyUtility.Use() => "Use".Log();
    }
    public interface IBaseModule : IModule
    {

    }
    public class BaseModule : IBaseModule
    {
        public void TryInit()
        {
            // ���Գ�ʼ��ģ�飬����Ѿ���ʼ�������ظ�����
        }
    }
    public class MyModule : AbsModule, IMyModule
    {
        protected override void OnInit()
        {
            // ��ʼ���׶�
        }
        protected override void OnDeInit()
        {
            // ���ٽ׶�
        }
        void IMyModule.Say() => "Say".Log();
        void IMyModule.Say(string msg) => msg.Log();
        string IMyModule.Get(int id) => id.ToString();
        string IMyModule.Msg => "˵��ѽ��";
    }
    public struct ExampleQuery : IQuery<string>
    {
        public string Do(IModuleHub hub)
        {
            return hub.Module<IMyModule>().Msg;
        }
    }
    public struct ExampleSelfQuery : IQuery<ExampleSelfQuery>
    {
        public ExampleSelfQuery Do(IModuleHub hub)
        {
            return this;
        }
    }
    public struct ExampleDataQuery : IQuery<int, string>
    {
        public string Do(IModuleHub hub, int num)
        {
            return hub.Module<IMyModule>().Get(num);
        }
    }
    public struct ExampleSelfDataQuery : IQuery<int, ExampleSelfDataQuery>
    {
        public ExampleSelfDataQuery Do(IModuleHub hub, int num)
        {
            return this;
        }
    }
    public struct ExampleDataCmd : ICmd<string>
    {
        public void Do(IModuleHub hub, string msg)
        {
            // ִ�������߼� hub �����������е��üܹ�Ȩ�� ����
            hub.Module<IMyModule>().Say(msg);
            hub.Utility<IMyUtility>();
        }
    }
    public struct ExampleCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            // ִ�������߼� hub �����������е��üܹ�Ȩ�� ����
            hub.Module<IMyModule>().Say();
        }
    }
    public struct MyEvent
    {
        public string Message;
    }
    //public class FK_Example : MonoBehaviour, IPermissionProvider
    //{
    //    IModuleHub IPermissionProvider.Hub => ExampleHub.GetIns();
    //}
    public class FK_Example : ExampleGame
    {
        private void Start()
        {
            this.SendCmd(new ExampleCmd());
            this.SendCmd<ExampleDataCmd, string>("��Ϣ");
            var q = this.Query<ExampleSelfDataQuery, int>(10);
        }
    }
}