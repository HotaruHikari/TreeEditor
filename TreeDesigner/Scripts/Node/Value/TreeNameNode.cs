using System;
using UnityEngine;

namespace TreeDesigner 
{
    [Serializable]
    [NodeName("TreeName")]
    [NodePath("Base/Value/TreeName")]
    public class TreeNameNode : ValueNode
    {
        [SerializeField, PropertyPort(PortDirection.Input, "Tree")]
        TreePropertyPort m_Tree = new TreePropertyPort();
        [SerializeField, PropertyPort(PortDirection.Output, "Name"), ReadOnly]
        StringPropertyPort m_Name = new StringPropertyPort();

        protected override void OutputValue()
        {
            base.OutputValue();
            m_Name.Value = m_Tree.Value.name;
        }
    }
}