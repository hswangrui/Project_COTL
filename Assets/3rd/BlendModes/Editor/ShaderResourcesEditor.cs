// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BlendModes
{
    [CustomEditor(typeof(ShaderResources))]
    public class ShaderResourcesEditor : Editor
    {
        private ShaderResources Target => target as ShaderResources;

        private const string extensionsInfo = "Each shader extension contains multiple blend effect shaders. Each shader compiles into multiple variants per blend mode (25 in total). When building, each shader variant takes time to compile for the target platform and graphics tier. Consider removing extension packages you're not using to reduce the compile time and build size.";
        private bool showManager;
        private IEnumerable<string> installedShaderExtensions;
        private IEnumerable<string> availableShaderExtensions;

        private void OnEnable ()
        {
            CacheExtensions();
        }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI();

            showManager = EditorGUILayout.Foldout(showManager, "Extensions", true);
            if (showManager) ManagerGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Update shader resources"))
                ExtensionManager.UpdateShaderResources();
        }

        private void ManagerGUI ()
        {
            if (availableShaderExtensions == null || installedShaderExtensions == null) return;
            EditorGUILayout.HelpBox(extensionsInfo, MessageType.Info);
            foreach (var packageName in availableShaderExtensions)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(packageName);
                if (installedShaderExtensions.Contains(packageName))
                    RemovePackageButtonGUI(packageName);
                else InstallPackageButtonGUI(packageName);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void InstallPackageButtonGUI (string packageName)
        {
            if (GUILayout.Button("Install", GUILayout.Width(100)))
            {
                ExtensionManager.InstallShaderExtension(packageName);
                ExtensionManager.UpdateShaderResources();
                CacheExtensions();
            }
        }

        private void RemovePackageButtonGUI (string packageName)
        {
            if (GUILayout.Button("Remove", GUILayout.Width(100)))
            {
                Target.RemoveAllShaders();
                ExtensionManager.RemoveShaderExtension(packageName);
                ExtensionManager.UpdateShaderResources();
                CacheExtensions();
            }
        }

        private void CacheExtensions ()
        {
            installedShaderExtensions = ExtensionManager.GetInstalledShaderExtensions();
            availableShaderExtensions = ExtensionManager.ExtensionSources.GetAvailableShaderExtensions();
        }
    }
}
