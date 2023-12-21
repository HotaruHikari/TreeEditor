using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TreeDesigner.Editor
{
    public static partial class TreeDesignerUtility
    {
        static Dictionary<Type, MonoScript> s_NodeScriptMap = new Dictionary<Type, MonoScript>();
        static Dictionary<Type, MonoScript> s_NodeViewScriptMap = new Dictionary<Type, MonoScript>();
        static Dictionary<BaseTree, SerializedObject> s_SerializedTreeMap = new Dictionary<BaseTree, SerializedObject>();

        static Dictionary<Type, string> s_NodePathMap = new Dictionary<Type, string>();
        static Dictionary<string, List<(Type, string)>> s_StartPathMap = new Dictionary<string, List<(Type, string)>>();

        public const string DefaultFolderGUID = "320778c47f0f2104fa68e3102f51659e";

        static TreeDesignerUtility()
        {
            BuildScriptCache(); 
        }

        #region Script

        /// <summary>
        /// ???????§ß?????
        /// </summary>
        static void BuildScriptCache()
        {
            foreach (var nodeType in TypeCache.GetTypesDerivedFrom<BaseNode>())
            {
                AddNodeScriptAsset(nodeType);
                AddNodePath(nodeType);
            }
            AddNodeViewScriptAsset(typeof(BaseNodeView));
            foreach (var nodeViewType in TypeCache.GetTypesDerivedFrom<BaseNodeView>())
            {
                AddNodeViewScriptAsset(nodeViewType);
            }
        }
        public static MonoScript GetNodeScript(Type type)
        {
            if (s_NodeScriptMap.TryGetValue(type, out MonoScript monoScript))
                return monoScript;
            return null;
        }
        public static MonoScript GetNodeViewScript(Type type)
        {
            if (s_NodeViewScriptMap.TryGetValue(type, out MonoScript monoScript))
                return monoScript;
            return null;
        }
        public static Type GetNodeViewType(string nodeViewTypeName)
        {
            foreach (var nodeViewScriptPair in s_NodeViewScriptMap)
            {
                if (nodeViewScriptPair.Key.Name == nodeViewTypeName)
                    return nodeViewScriptPair.Key;
            }
            return null;
        }
        public static string GetNodePath(Type type)
        {
            if (s_NodePathMap.TryGetValue(type, out string path))
                return path;
            return string.Empty;
        }
        public static List<string> GetNodePaths(string startPath)
        {
            List<string> nodePaths = new List<string>();
            if (s_StartPathMap.TryGetValue(startPath, out List<(Type, string)> pathPairs))
                pathPairs.ForEach(i => nodePaths.Add(i.Item2));
            return nodePaths;
        }
        public static List<(Type,string)> GetNodePathPairs(string startPath)
        {
            List<(Type, string)> nodePathPairs = new List<(Type, string)>();
            if (s_StartPathMap.TryGetValue(startPath, out List<(Type, string)> pathPairs))
                nodePathPairs.AddRange(pathPairs);
            return nodePathPairs;
        }
        static void AddNodeScriptAsset(Type type)
        {
            var nodeScriptAsset = FindScriptFromClassName(type.Name);
            if (nodeScriptAsset != null)
                s_NodeScriptMap[type] = nodeScriptAsset;
        }
        static void AddNodeViewScriptAsset(Type type)
        {
            var nodeScriptAsset = FindScriptFromClassName(type.Name);
            if (nodeScriptAsset != null)
                s_NodeViewScriptMap[type] = nodeScriptAsset;
        }
        static void AddNodePath(Type type)
        {
            if (type.IsAbstract) return;

            NodePathAttribute nodePathAttribute = type.GetAttribute<NodePathAttribute>();
            if (nodePathAttribute == null) return;

            if (!s_NodePathMap.ContainsKey(type))
                s_NodePathMap.Add(type, nodePathAttribute.Path);

            var pathSplitStrs = nodePathAttribute.Path.Split(new char[] { '/' });
            if (pathSplitStrs.Length == 1) return;

            string startPath = pathSplitStrs[0];
            if (!s_StartPathMap.ContainsKey(startPath))
                s_StartPathMap.Add(startPath, new List<(Type, string)>());
            s_StartPathMap[startPath].Add((type, nodePathAttribute.Path));
        }
        static MonoScript FindScriptFromClassName(string className)
        {
            var scriptGUIDs = AssetDatabase.FindAssets($"t:script {className}");

            if (scriptGUIDs.Length == 0)
                return null;

            foreach (var scriptGUID in scriptGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(scriptGUID);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                if (script != null && string.Equals(className, Path.GetFileNameWithoutExtension(assetPath), StringComparison.OrdinalIgnoreCase))
                    return script;
            }

            return null;
        }
        #endregion

        #region SerializedProperty
        public static void ApplyModify(this BaseTree tree, string name, Action action)
        {
            Undo.RegisterCompleteObjectUndo(tree, $"Tree ({name})");
            tree.GetSerializedTree().Update();
            action?.Invoke();
            EditorUtility.SetDirty(tree);
        }
        public static void ApplyModify(this BaseNode node, string name, Action action)
        {
            ApplyModify(node.Owner, name, action);
        }

        public static SerializedObject GetNewSerializedTree(this BaseTree tree)
        {
            if (!s_SerializedTreeMap.ContainsKey(tree))
                s_SerializedTreeMap.Add(tree, new SerializedObject(tree));
            else
                s_SerializedTreeMap[tree] = new SerializedObject(tree);

            return s_SerializedTreeMap[tree];
        }
        public static SerializedObject GetNewSerializedTree(this BaseNode node)
        {
            return GetNewSerializedTree(node.Owner);
        }
        public static SerializedObject GetSerializedTree(this BaseTree tree)
        {
            if (!s_SerializedTreeMap.ContainsKey(tree))
                s_SerializedTreeMap.Add(tree, new SerializedObject(tree));

            //s_SerializedTreeMap[tree].Update();
            return s_SerializedTreeMap[tree];
        }
        public static SerializedObject GetSerializedTree(this BaseNode node)
        {
            return node.Owner.GetSerializedTree();
        }
        public static SerializedObject GetSerializedTree(this BaseExposedProperty exposedProperty)
        {
            return exposedProperty.Owner.GetSerializedTree();
        }
        public static SerializedProperty GetSerializedNode(this BaseNode node)
        {
            return node.GetSerializedTree().FindProperty("m_Nodes").GetArrayElementAtIndex(node.Owner.Nodes.IndexOf(node));
        }
        public static SerializedProperty GetSerializedExposedProperty(this BaseExposedProperty exposedProperty)
        {
            return exposedProperty.GetSerializedTree().FindProperty("m_ExposedProperties").GetArrayElementAtIndex(exposedProperty.Owner.ExposedProperties.IndexOf(exposedProperty));
        }
        public static SerializedProperty GetNodeSerializedProperty(this BaseNode node, string propertyName)
        {
            return node.GetSerializedNode().FindPropertyRelative(propertyName);
        }
        public static SerializedProperty GetExposedPropertySerializedProperty(this BaseExposedProperty exposedProperty, string propertyName)
        {
            return exposedProperty.GetSerializedExposedProperty().FindPropertyRelative(propertyName);
        }
        #endregion

        [MenuItem("Assets/Create/TreeDesigner/BaseTree")]
        public static void CreateBaseTree()
        {
            BaseTree tree = ScriptableObject.CreateInstance<BaseTree>();
            tree.RootGUID = tree.CreateNode(typeof(RootNode)).GUID;

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Tree.asset");
            AssetDatabase.CreateAsset(tree, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
           
            Selection.activeObject = tree;
        }

        [MenuItem("Assets/Create/TreeDesigner/SubTree")]
        public static void CreateSubTree()
        {
            SubTree tree = ScriptableObject.CreateInstance<SubTree>();
            tree.RootGUID = tree.CreateNode(typeof(RootNode)).GUID;

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New SubTree.asset");
            AssetDatabase.CreateAsset(tree, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = tree;
        }
    }
}