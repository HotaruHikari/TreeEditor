using System;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    [NodeColor(255, 209, 102)]
    [Input("Input"), Output("Output", PortCapacity.Single)]
    public abstract partial class DecoratorNode : BaseNode
    {
        [SerializeField]
        string m_InputEdgeGUID;
        public string InputEdgeGUID => m_InputEdgeGUID;
        
        [SerializeField]
        protected string m_OutputEdgeGUID;
        public string OutputGUID => m_OutputEdgeGUID;

        [NonSerialized]
        protected BaseNode m_Parent;
        public BaseNode Parent => m_Parent;

        [NonSerialized]
        protected BaseNode m_Child;
        public BaseNode Child => m_Child;

        public override void Init(BaseTree tree)
        {
            base.Init(tree);

            if (!string.IsNullOrEmpty(m_InputEdgeGUID))
                m_Parent = m_Owner.GUIDEdgeMap[m_InputEdgeGUID].StartNode;
            if (!string.IsNullOrEmpty(m_OutputEdgeGUID))
                m_Child = m_Owner.GUIDEdgeMap[m_OutputEdgeGUID].EndNode;
        }
        public override void Dispose()
        {
            base.Dispose();

            m_Parent = null;
            m_Child = null;
        }
        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
            m_OutputEdgeGUID = string.Empty;
            m_Child = null;
        }
        public override void ResetNode()
        {
            base.ResetNode();
            m_Child?.ResetNode();
        }
    }
}