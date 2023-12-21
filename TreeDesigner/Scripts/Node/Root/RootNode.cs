using System;
using UnityEngine;

namespace TreeDesigner 
{
    [Serializable]
    [NodeName("Root")]
    [NodeColor(217, 187, 249)]
    [Output("Output", PortCapacity.Single)]
    public partial class RootNode : BaseNode
    {
        [SerializeField]
        protected string m_OutputEdgeGUID;
        public string OutputGUID => m_OutputEdgeGUID;

        [NonSerialized]
        protected BaseNode m_Child;
        public BaseNode Child => m_Child;

        public override void Init(BaseTree tree)
        {
            base.Init(tree);

            if (!string.IsNullOrEmpty(m_OutputEdgeGUID))
                m_Child = m_Owner.GUIDEdgeMap[m_OutputEdgeGUID].EndNode;
        }
        public override void Dispose()
        {
            base.Dispose();

            m_Child = null;
        }
        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            m_OutputEdgeGUID = string.Empty;
            m_Child = null;
        }
        public override void ResetNode()
        {
            base.ResetNode();
            m_Child?.ResetNode();
        }

        protected override State OnUpdate()
        {
            if (m_Owner.Running && m_Child)
                return m_Child.UpdateNode();
            else
                return State.None;
        }
    }
}