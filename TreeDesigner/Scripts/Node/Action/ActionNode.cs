using System;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    [NodeColor(239, 71, 111)]
    [Input("Input")]
    public abstract partial class ActionNode : BaseNode
    {
        [SerializeField]
        string m_InputEdgeGUID;
        public string InputEdgeGUID => m_InputEdgeGUID;

        [NonSerialized]
        protected BaseNode m_Parent;
        public BaseNode Parent => m_Parent;

        public virtual State ReturnState => State.Success;

        public override void Init(BaseTree tree)
        {
            base.Init(tree);

            if (!string.IsNullOrEmpty(m_InputEdgeGUID))
                m_Parent = m_Owner.GUIDEdgeMap[m_InputEdgeGUID].StartNode;
        }
        public override void Dispose()
        {
            base.Dispose();

            m_Parent = null;
        }
        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
        }

        protected override void OnStart()
        {
            base.OnStart();

            //if (m_Parent.State == State.Running)

            DoAction();
            //try
            //{
            //    DoAction();
            //}
            //catch (Exception e)
            //{
            //    this.Log(e);
            //}
        }
        protected override State OnUpdate()
        {
            return ReturnState;
        }

        protected abstract void DoAction();
    }
}