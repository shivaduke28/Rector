using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Rector.Editor
{
    //         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
    //                    Version 2, December 2004
    //
    // Copyright (C) 2004 Sam Hocevar <sam@hocevar.net>
    //
    // Everyone is permitted to copy and distribute verbatim or modified
    // copies of this license document, and changing it is allowed as long
    // as the name is changed.
    //
    //            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
    //   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
    //
    //  0. You just DO WHAT THE FUCK YOU WANT TO.
    // from: https://light11.hatenadiary.com/entry/2021/11/30/190034
    [CustomPropertyDrawer(typeof(SelectableSerializeReferenceAttribute))]
    public sealed class SelectableSerializeReferenceAttributeDrawer : PropertyDrawer
    {
        readonly Dictionary<string, PropertyData> dataPerPath = new();

        PropertyData data;

        int selectedIndex;

        void Init(SerializedProperty property)
        {
            if (dataPerPath.TryGetValue(property.propertyPath, out data))
            {
                return;
            }

            data = new PropertyData(property);
            dataPerPath.Add(property.propertyPath, data);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Assert.IsTrue(property.propertyType == SerializedPropertyType.ManagedReference);

            Init(property);

            var fullTypeName = property.managedReferenceFullTypename.Split(' ').Last();
            selectedIndex = Array.IndexOf(data.DerivedFullTypeNames, fullTypeName);

            using (var ccs = new EditorGUI.ChangeCheckScope())
            {
                var selectorPosition = position;

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                selectorPosition.width -= EditorGUIUtility.labelWidth;
                selectorPosition.x += EditorGUIUtility.labelWidth;
                selectorPosition.height = EditorGUIUtility.singleLineHeight;
                var selectedTypeIndex = EditorGUI.Popup(selectorPosition, selectedIndex, data.DerivedTypeNames);
                if (ccs.changed)
                {
                    selectedIndex = selectedTypeIndex;
                    var selectedType = data.DerivedTypes[selectedTypeIndex];
                    property.managedReferenceValue =
                        selectedType == null ? null : Activator.CreateInstance(selectedType);
                }

                EditorGUI.indentLevel = indent;
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);

            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            return EditorGUI.GetPropertyHeight(property, true);
        }

        sealed class PropertyData
        {
            internal PropertyData(SerializedProperty property)
            {
                var managedReferenceFieldTypenameSplit = property.managedReferenceFieldTypename.Split(' ').ToArray();
                var assemblyName = managedReferenceFieldTypenameSplit[0];
                var fieldTypeName = managedReferenceFieldTypenameSplit[1];
                var fieldType = GetAssembly(assemblyName).GetType(fieldTypeName);
                DerivedTypes = TypeCache.GetTypesDerivedFrom(fieldType).Where(x => !x.IsAbstract && !x.IsInterface && x.GetConstructor(Type.EmptyTypes) != null).ToArray();
                DerivedTypeNames = new string[DerivedTypes.Length];
                DerivedFullTypeNames = new string[DerivedTypes.Length];
                for (var i = 0; i < DerivedTypes.Length; i++)
                {
                    var type = DerivedTypes[i];
                    DerivedTypeNames[i] = ObjectNames.NicifyVariableName(type.Name);
                    DerivedFullTypeNames[i] = type.FullName?.Replace("+", "/"); // for nested class (modified from the original code)
                }
            }

            public Type[] DerivedTypes { get; }

            public string[] DerivedTypeNames { get; }

            public string[] DerivedFullTypeNames { get; }

            static Assembly GetAssembly(string name)
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .SingleOrDefault(assembly => assembly.GetName().Name == name);
            }
        }
    }
}
