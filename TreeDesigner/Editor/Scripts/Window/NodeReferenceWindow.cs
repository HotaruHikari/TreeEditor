using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace TreeDesigner.Editor
{
    public class NodeReferenceWindow : EditorWindow
    {
        bool m_Started;
        int m_CurrentIndex;
        int m_WaitFrame;
        MonoScript m_MonoScript;
        IMGUIContainer m_TreeContainer;
        List<BaseTree> m_TargetTrees = new List<BaseTree>();

        public virtual void CreateGUI()
        {
            IMGUIContainer imguiContainer = new IMGUIContainer(() =>
            {
                m_MonoScript = EditorGUILayout.ObjectField("NodeType", m_MonoScript, typeof(MonoScript), false) as MonoScript;
                if (!m_Started && GUILayout.Button("Find"))
                {
                    if (m_TreeContainer != null)
                    {
                        rootVisualElement.Remove(m_TreeContainer);
                        m_TreeContainer = null;
                        m_TargetTrees.ForEach(i => Resources.UnloadAsset(i));
                        m_TargetTrees.Clear();
                    }

                    if (!m_MonoScript)
                    {
                        Debug.Log("NodeType Can't be null");
                        return;
                    }
                    if (m_MonoScript.GetClass().IsSubclassOf(typeof(BaseNode)) && !m_MonoScript.GetClass().IsAbstract)
                    {
                        m_Started = true;
                        m_CurrentIndex = 0;
                        m_WaitFrame = 1;
                    }
                    else
                        Debug.Log("This Class Isn't Subclass Of BaseNode");
                }
                if(m_TreeContainer != null && GUILayout.Button("Clear"))
                {
                    rootVisualElement.Remove(m_TreeContainer);
                    m_TreeContainer = null;
                    m_TargetTrees.ForEach(i => Resources.UnloadAsset(i));
                    m_TargetTrees.Clear();
                }
            });
            rootVisualElement.Add(imguiContainer);
        }

        private void Update()
        {
            if (m_WaitFrame > 0)
            {
                m_WaitFrame--;
                return;
            }

            if (m_Started)
            {
                if (m_CurrentIndex < TreeModificationProcessor.TreeLocations.TreeInfos.Count)
                {
                    TreeLocations.TreeLocationInfo treeLocationInfo = TreeModificationProcessor.TreeLocations.TreeInfos[m_CurrentIndex];
                    EditorUtility.DisplayProgressBar("FindReference", treeLocationInfo.name, (float)m_CurrentIndex / TreeModificationProcessor.TreeLocations.TreeInfos.Count);

                    BaseTree tree = AssetDatabase.LoadAssetAtPath<BaseTree>(treeLocationInfo.path);
                    if (tree.Nodes.Find(i => i.GetType() == m_MonoScript.GetClass()) != null)
                        m_TargetTrees.Add(tree);

                    m_CurrentIndex++;
                    m_WaitFrame = 1;
                }
                else
                {
                    m_Started = false;
                    EditorUtility.ClearProgressBar();
                    m_TreeContainer = new IMGUIContainer(() =>
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("TargetTrees");
                        GUI.enabled = false;
                        m_TargetTrees.ForEach(i => EditorGUILayout.ObjectField(i, typeof(BaseTree), false));
                        GUI.enabled = true;
                    });
                    rootVisualElement.Add(m_TreeContainer);
                }
            }
        }
        private void OnDisable()
        {
            m_TargetTrees.ForEach(i => Resources.UnloadAsset(i));
            m_TargetTrees.Clear();
        }

        [MenuItem("Tools/TreeDesigner/NodeReferenceWindow", false, 0)]
        public static void OpenNodeReferenceWindow()
        {
            GetWindow<NodeReferenceWindow>();
        }
    }
}