using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TreeDesigner.Editor 
{
    public class DragLineManipulator : PointerManipulator
    {
        bool m_Active;
        Vector3 m_Start;
        Action<float> m_OnDrag;
        IMGUIContainer m_IMGUIContainer;

        public DragLineManipulator(Action<float> onDrag)
        {
            m_OnDrag = onDrag;
            m_Active = false;
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse
            });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);

            m_IMGUIContainer = new IMGUIContainer(() =>
            {
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, target.worldBound.width, target.worldBound.height), MouseCursor.ResizeHorizontal);
            });
            target.Add(m_IMGUIContainer);
        }
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);

            target.Remove(m_IMGUIContainer);
            m_IMGUIContainer = null;
        }

        public void ApplyDelta(float delta)
        {
            m_OnDrag.Invoke(delta);
        }
        protected void OnPointerDown(PointerDownEvent e)
        {
            if (m_Active)
            {
                e.StopImmediatePropagation();
            }
            else if (CanStartManipulation(e))
            {
                m_Start = e.localPosition;
                m_Active = true;
                base.target.CapturePointer(e.pointerId);
                e.StopPropagation();
            }
        }
        protected void OnPointerMove(PointerMoveEvent e)
        {
            if (m_Active && base.target.HasPointerCapture(e.pointerId))
            {
                Vector2 vector = e.localPosition - m_Start;
                float num = vector.x;
                ApplyDelta(num);
                e.StopPropagation();
            }
        }
        protected void OnPointerUp(PointerUpEvent e)
        {
            if (m_Active && base.target.HasPointerCapture(e.pointerId) && CanStopManipulation(e))
            {
                m_Active = false;
                base.target.ReleasePointer(e.pointerId);
                e.StopPropagation();
            }
        }
    }
}