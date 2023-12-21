using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace TreeDesigner.Editor
{
    public class DropdownMenuManipulator : Clickable
    {
        DropdownMenuHandler m_DropdownMenuHandler;

        public DropdownMenuManipulator(Action<DropdownMenu> menuBuilder, MouseButton mouseButton) : base(() => { })
        {
            m_DropdownMenuHandler = new DropdownMenuHandler(menuBuilder);
            activators.Clear();
            activators.Add(new ManipulatorActivationFilter
            {
                button = mouseButton
            });
            clicked += () =>
            {
                m_DropdownMenuHandler.ShowMenu(target);
            };
        }
    }
}
