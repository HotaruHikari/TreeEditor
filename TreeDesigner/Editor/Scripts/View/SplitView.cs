using UnityEngine.UIElements;

namespace TreeDesigner.Editor
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
        public SplitView() { }
    }
}