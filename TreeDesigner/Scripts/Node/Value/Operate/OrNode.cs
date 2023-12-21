using System;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    [NodeName("Or")]
    [NodePath("Base/Value/Operate/Or")]
    public class OrNode : ValueNode
    {
        [SerializeField, PropertyPort(PortDirection.Input, "Input1")]
        BoolPropertyPort m_Input1 = new BoolPropertyPort();
        [SerializeField, PropertyPort(PortDirection.Input, "Input2")]
        BoolPropertyPort m_Input2 = new BoolPropertyPort();
        [SerializeField, PropertyPort(PortDirection.Output, "Output"), ReadOnly]
        BoolPropertyPort m_Output = new BoolPropertyPort();

        protected override void OutputValue()
        {
            base.OutputValue();
            m_Output.Value = m_Input1.Value || m_Input2.Value;
        }
    }
}