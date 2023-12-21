using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TreeDesigner.Editor
{
    public interface IDragableVisualElement
    {
        public void StartDrag();
        public void StopDrag();
        public void UpdateDrag(DragUpdatedEvent e, VisualElement dragArea);
        public void PerformDrag(DragPerformEvent e, VisualElement dragArea);
    }
}
