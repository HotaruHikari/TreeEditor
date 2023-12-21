using System;
using System.Collections.Generic;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    [NodeColor(6, 214, 160)]
    [Input("Input"), Output("Output", PortCapacity.Multi)]
    public abstract partial class CompositeNode : BaseNode
    {
        [SerializeField]
        string m_InputEdgeGUID;
        public string InputEdgeGUID => m_InputEdgeGUID;

        [SerializeField]
        protected List<string> m_OutputEdgeGUIDs = new List<string>();
        public List<string> OutputGUIDs => m_OutputEdgeGUIDs;

        [NonSerialized]
        protected BaseNode m_Parent;
        public BaseNode Parent => m_Parent;

        [NonSerialized]
        protected List<BaseNode> m_Children = new List<BaseNode>();
        public List<BaseNode> Children => m_Children;

        public override void Init(BaseTree tree)
        {
            base.Init(tree);

            if (!string.IsNullOrEmpty(m_InputEdgeGUID))
                m_Parent = m_Owner.GUIDEdgeMap[m_InputEdgeGUID].StartNode;

            m_Children.Clear();
            m_OutputEdgeGUIDs.ForEach(i => m_Children.Add(m_Owner.GUIDEdgeMap[i].EndNode));
        }
        public override void Dispose()
        {
            base.Dispose();

            m_Parent = null;
            m_Children.Clear();
        }
        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
            m_OutputEdgeGUIDs.Clear();
            m_Children.Clear();
        }
        public override void ResetNode()
        {
            base.ResetNode();
            m_Children.ForEach(i => i.ResetNode());
        }
    }
}