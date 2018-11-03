﻿using OmiyaGames.Builds;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace OmiyaGames.UI.Builds
{
    ///-----------------------------------------------------------------------
    /// <copyright file="ChildBuildSettingReorderableList.cs" company="Omiya Games">
    /// The MIT License (MIT)
    /// 
    /// Copyright (c) 2014-2018 Omiya Games
    /// 
    /// Permission is hereby granted, free of charge, to any person obtaining a copy
    /// of this software and associated documentation files (the "Software"), to deal
    /// in the Software without restriction, including without limitation the rights
    /// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    /// copies of the Software, and to permit persons to whom the Software is
    /// furnished to do so, subject to the following conditions:
    /// 
    /// The above copyright notice and this permission notice shall be included in
    /// all copies or substantial portions of the Software.
    /// 
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    /// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    /// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    /// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    /// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    /// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    /// THE SOFTWARE.
    /// </copyright>
    /// <author>Taro Omiya</author>
    /// <date>11/03/2018</date>
    ///-----------------------------------------------------------------------
    /// <summary>
    /// Property drawer for <code>CustomFileName</code>.
    /// </summary>
    public class ChildBuildSettingReorderableList
    {
        public class BuildSettingCreator
        {
            public BuildSettingCreator(string name, GenericMenu.MenuFunction function)
            {
                DisplayName = name;
                Function = function;
            }

            public string DisplayName
            {
                get;
            }

            public GenericMenu.MenuFunction Function
            {
                get;
            }
        }

        public readonly BuildSettingCreator[] AllMethods;

        public ChildBuildSettingReorderableList(SerializedProperty property, GUIContent label)
        {
            // Member Variable
            Property = property;
            Label = label;

            // Setup List
            List = new ReorderableList(Property.serializedObject, Property, true, true, true, true);
            List.drawHeaderCallback = DrawBuildSettingListHeader;
            List.drawElementCallback = DrawBuildSettingListElement;
            List.onAddDropdownCallback = DrawBuildSettingListDropdown;
            List.elementHeight = EditorUiUtility.GetHeight(2);

            // Setup all Methods
            AllMethods = new BuildSettingCreator[]
            {
                new BuildSettingCreator("Group of Platforms", () => { AddAndModify<GroupBuildSetting>("Group"); }),
                null,
                new BuildSettingCreator("Windows 64-bit", () => { CreateDesktopPlatformSettings<WindowsBuildSetting>("Windows 64-bit", IPlatformBuildSetting.Architecture.Build64Bit); }),
                new BuildSettingCreator("Windows 32-bit", () => { CreateDesktopPlatformSettings<WindowsBuildSetting>("Windows 32-bit", IPlatformBuildSetting.Architecture.Build32Bit); }),
                null,
                new BuildSettingCreator("Mac", () => { AddAndModify<MacBuildSetting>("Mac"); }),
                null,
                new BuildSettingCreator("Linux Universal", () => { CreateDesktopPlatformSettings<LinuxBuildSetting>("Linux", IPlatformBuildSetting.Architecture.BuildUniversal); }),
                new BuildSettingCreator("Linux 64-bit", () => { CreateDesktopPlatformSettings<LinuxBuildSetting>("Linux 64-bit", IPlatformBuildSetting.Architecture.Build64Bit); }),
                new BuildSettingCreator("Linux 32-bit", () => { CreateDesktopPlatformSettings<LinuxBuildSetting>("Linux 32-bit", IPlatformBuildSetting.Architecture.Build32Bit); }),
                null,
                new BuildSettingCreator("WebGL", () => { CreateWebGLSettings(); }),
                null,
                new BuildSettingCreator("iOS", () => { AddAndModify<IosBuildSetting>("iOS"); }),
                new BuildSettingCreator("Android", () => { AddAndModify<AndroidBuildSetting>("Android"); }),
                new BuildSettingCreator("UWP", () => { AddAndModify<UwpBuildSetting>("UWP"); }),
            };
        }

        private void CreateWebGLSettings()
        {
            SerializedProperty element = Add<WebGlBuildSetting>("WebGL");
            element.FindPropertyRelative("fileName").FindPropertyRelative("asSlug").boolValue = true;

            // Apply the property
            ApplyModification();
        }

        private void CreateDesktopPlatformSettings<T>(string name, IPlatformBuildSetting.Architecture architecture) where T : IPlatformBuildSetting
        {
            SerializedProperty element = Add<T>(name);
            element.FindPropertyRelative("architecture").enumValueIndex = (int)architecture;

            // Apply the property
            ApplyModification();
        }

        #region Properties
        public SerializedProperty Property
        {
            get;
        }

        public ReorderableList List
        {
            get;
        }

        public GUIContent Label
        {
            get;
        }
        #endregion

        #region Helper Methods
        private void DrawBuildSettingListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, Label);
        }

        private void DrawBuildSettingListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Get all the properties
            SerializedProperty element = Property.GetArrayElementAtIndex(index);

            // Calculate position
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += EditorUiUtility.VerticalMargin;

            // Draw the object field
            bool originalEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.ObjectField(rect, "", element.objectReferenceValue, typeof(IChildBuildSetting), false);
            GUI.enabled = originalEnabled;

            // Calculate position
            rect.y += rect.height;
            rect.y += EditorUiUtility.VerticalMargin;

            // Draw Edit buttons
            DrawButtons(rect);
        }

        private void DrawBuildSettingListDropdown(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();
            foreach (BuildSettingCreator value in AllMethods)
            {
                if (value != null)
                {
                    menu.AddItem(new GUIContent(value.DisplayName), false, value.Function);
                }
                else
                {
                    menu.AddSeparator("");
                }
            }
            menu.ShowAsContext();
        }

        private SerializedProperty Add<T>(string name) where T : IChildBuildSetting
        {
            SerializedProperty element = CreateNewElement();

            // Setup data field
            T instance = ScriptableObject.CreateInstance<T>();
            Debug.Log(instance);
            instance.name = name;
            element.objectReferenceValue = instance;
            Debug.Log(element.objectReferenceValue);
            return element;
        }

        private void AddAndModify<T>(string name) where T : IChildBuildSetting
        {
            Add<T>(name);
            ApplyModification();
        }

        private void Duplicate<T>(string name, T original) where T : IChildBuildSetting
        {
            SerializedProperty element = CreateNewElement();

            // Setup data field
            T instance = ScriptableObject.Instantiate<T>(original);
            instance.name = name;
            element.objectReferenceValue = instance;

            // Apply the property
            ApplyModification();
        }

        private void ApplyModification()
        {
            List.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty CreateNewElement()
        {
            int index = List.serializedProperty.arraySize;
            List.serializedProperty.arraySize++;
            List.index = index;
            return List.serializedProperty.GetArrayElementAtIndex(index);
        }

        private static void DrawButtons(Rect rect)
        {
            rect.width -= (EditorUiUtility.VerticalMargin * 2);
            rect.width /= 3f;
            if (GUI.Button(rect, "Edit") == true)
            {
                Debug.Log("Hit Edit button");
            }
            rect.x += EditorUiUtility.VerticalMargin;
            rect.x += rect.width;
            if (GUI.Button(rect, "Duplicate") == true)
            {
                Debug.Log("Hit Duplicate button");
            }
            rect.x += EditorUiUtility.VerticalMargin;
            rect.x += rect.width;
            if (GUI.Button(rect, "Build") == true)
            {
                Debug.Log("Build");
            }
        }
        #endregion
    }
}
