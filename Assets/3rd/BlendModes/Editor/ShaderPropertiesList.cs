// Copyright 2014-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BlendModes
{
    public class ShaderPropertiesList
    {
        private const float headerSpace = 2;
        private const float paddingWidth = 5;
        private const float paddingHeight = 4;

        private static readonly GUIContent nameConent = new GUIContent("Name", "Name of the shader property.");
        private static readonly GUIContent typeConent = new GUIContent("Type", "Type of the shader property.");
        private static readonly GUIContent valueConent = new GUIContent("Value", "Value of the shader property.");

        private ReorderableList propertiesList;
        private SerializedObject serializedObject;
        private SerializedProperty serializedProperty;
        private ComponentExtension componentExtension;

        public ShaderPropertiesList (SerializedObject serializedObject, SerializedProperty serializedProperty, ComponentExtension componentExtension)
        {
            this.serializedObject = serializedObject;
            this.serializedProperty = serializedProperty;
            this.componentExtension = componentExtension;
        }

        public void DrawList ()
        {
            if (componentExtension == null || componentExtension.DefaultShaderProperties.Length == 0) return;

            if (propertiesList == null) InitilizePropertiesList();

            if (ShouldDrawAnyElement())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Extension Shader Properties", EditorStyles.boldLabel);
                propertiesList.DoLayoutList();
            }
        }

        private void InitilizePropertiesList ()
        {
            propertiesList = new ReorderableList(serializedObject, serializedProperty, false, true, false, false);
            propertiesList.drawHeaderCallback = DrawBindListHeader;
            propertiesList.drawElementCallback = DrawBindListElement;
            propertiesList.elementHeightCallback = GetElementHeight;
            propertiesList.footerHeight = 0;
        }

        private void DrawBindListHeader (Rect rect)
        {
            var propertyRect = new Rect(rect.x, rect.y, (rect.width / 3f) - paddingWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(propertyRect, nameConent);
            propertyRect.x += propertyRect.width + paddingWidth;
            EditorGUI.LabelField(propertyRect, typeConent);
            propertyRect.x += propertyRect.width + paddingWidth;
            EditorGUI.LabelField(propertyRect, valueConent);
        }

        private void DrawBindListElement (Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!ShouldDrawElement(index)) return;

            var element = propertiesList.serializedProperty.GetArrayElementAtIndex(index);
            var nameProperty = element.FindPropertyRelative("name");
            var typeProperty = element.FindPropertyRelative("type");

            var propertyRect = new Rect(rect.x, rect.y + headerSpace, (rect.width / 3f) - paddingWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(propertyRect, nameProperty, GUIContent.none);
            propertyRect.x += propertyRect.width + paddingWidth;
            EditorGUI.PropertyField(propertyRect, typeProperty, GUIContent.none);
            propertyRect.x += propertyRect.width + paddingWidth;
            EditorGUI.EndDisabledGroup();
            var type = (ShaderPropertyType)typeProperty.enumValueIndex;
            switch (type)
            {
                case ShaderPropertyType.Color:
                    EditorGUI.PropertyField(propertyRect, element.FindPropertyRelative("colorValue"), GUIContent.none);
                    break;
                case ShaderPropertyType.Vector:
                    var property = element.FindPropertyRelative("vectorValue");
                    property.vector4Value = EditorGUI.Vector4Field(propertyRect, GUIContent.none, property.vector4Value);
                    break;
                case ShaderPropertyType.Float:
                    EditorGUI.PropertyField(propertyRect, element.FindPropertyRelative("floatValue"), GUIContent.none);
                    break;
                case ShaderPropertyType.Texture:
                    EditorGUI.PropertyField(propertyRect, element.FindPropertyRelative("textureValue"), GUIContent.none);
                    break;
            }
        }

        private float GetElementHeight (int index)
        {
            return ShouldDrawElement(index) ? EditorGUIUtility.singleLineHeight + paddingHeight : 0f;
        }

        private bool ShouldDrawElement (int index)
        {
            // Don't draw shader properties that doesn't belong to the current render material.
            // This allows us to define properties for multiple shader families, but draw only the relevant ones.
            var element = propertiesList.serializedProperty.GetArrayElementAtIndex(index);
            var propertyName = element.FindPropertyRelative("name").stringValue;
            return componentExtension.MaterialHasProperty(propertyName);
        }

        private bool ShouldDrawAnyElement ()
        {
            if (propertiesList == null || propertiesList.serializedProperty == null) return false;
            for (int i = 0; i < propertiesList.serializedProperty.arraySize; i++)
                if (ShouldDrawElement(i)) return true;
            return false;
        }
    }
}
