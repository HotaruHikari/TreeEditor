using System;

namespace TreeDesigner
{
    /// <summary>
    /// 节点的端口数量
    /// </summary>
    public enum PortCapacity
    {
        Single,
        Multi
    }

    /// <summary>
    /// 节点的端口类型
    /// </summary>
    public enum PortDirection
    {
        Input,
        Output
    }

    [Flags]
    public enum NodeCapabilities
    {
        Selectable = 0x1,
        Collapsible = 0x2,
        Resizable = 0x4,
        Movable = 0x8,
        Deletable = 0x10,
        Droppable = 0x20,
        Ascendable = 0x40,
        Renamable = 0x80,
        Copiable = 0x100,
        Snappable = 0x200,
        Groupable = 0x400,
        Stackable = 0x800
    }

    public enum State
    {
        None,
        Running,
        Success,
        Failure,
    }
}