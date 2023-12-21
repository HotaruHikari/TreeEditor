using System;
using UnityEngine;

namespace TreeDesigner
{
    [Serializable]
    [NodeName("Curve")]
    [NodePath("Base/Value/Curve")]
    public class CurveNode : ValueNode
    {
        [SerializeField, ShowInPanel("Curve")]
        AnimationCurve m_Curve;
        [SerializeField, PropertyPort(PortDirection.Input, "Input")]
        FloatPropertyPort m_Input = new FloatPropertyPort();
        [SerializeField, PropertyPort(PortDirection.Output, "Output"), ReadOnly]
        FloatPropertyPort m_Output = new FloatPropertyPort();

        protected override void OutputValue()
        {
            base.OutputValue();
            m_Output.Value = m_Curve.Evaluate(m_Input.Value);
        }
    }
}