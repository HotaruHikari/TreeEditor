using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    [NodeName("ToString")]
    [NodePath("Base/Value/Operate/ToString")]
    [NodeView("VariablePropertyNodeView")]
    public partial class ToStringNode : ValueNode
    {
        [SerializeReference, VariablePropertyPort(PortDirection.Input, "Value", "AcceptableTypes")]
        PropertyPort m_Value = new PropertyPort();
        [SerializeField, PropertyPort(PortDirection.Output, "String"), ReadOnly]
        StringPropertyPort m_String = new StringPropertyPort();

        protected override void OutputValue()
        {
            base.OutputValue();
            m_String.Value = m_Value.GetValue().ToString();
        }
    }
}