using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    [NodeColor(255, 209, 102)]
    public partial class TimelineNode : BaseNode
    {
        [SerializeField]
        string m_InputEdgeGUID;
        public string InputEdgeGUID => m_InputEdgeGUID;

        [SerializeField]
        List<string> m_OutputEdgeGUIDs = new List<string>();
        public List<string> OutputEdgeGUIDs => m_OutputEdgeGUIDs;

        [SerializeField]
        List<float> m_Times = new List<float>();
        public List<float> Times => m_Times;

        [NonSerialized]
        protected BaseNode m_Parent;
        public BaseNode Parent => m_Parent;

        [NonSerialized]
        protected List<BaseNode> m_Children;
        public List<BaseNode> Children => m_Children;

        public override void Init(BaseTree tree)
        {
            base.Init(tree);

            if (!string.IsNullOrEmpty(m_InputEdgeGUID))
                m_Parent = m_Owner.GUIDEdgeMap[m_InputEdgeGUID].StartNode;

            for (int i = 0; i < m_OutputEdgeGUIDs.Count; i++)
            {
                if (m_Owner.GUIDEdgeMap.TryGetValue(m_OutputEdgeGUIDs[i], out BaseEdge outputEdge))
                    m_Children[i] = outputEdge.EndNode;
            }
        }
        public override void Dispose()
        {
            base.Dispose();

            m_Parent = null;
            m_Children.ForEach(i => i = null);
        }
        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            m_InputEdgeGUID = string.Empty;
            m_Parent = null;
            m_OutputEdgeGUIDs.ForEach(i => i = string.Empty);
            m_Children.ForEach(i => i = null);
        }
        public override void ResetNode()
        {
            base.ResetNode();
            m_Children.ForEach(i => i?.ResetNode());
        }
    }
}