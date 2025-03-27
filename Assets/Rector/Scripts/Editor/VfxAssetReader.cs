using System;
using System.Collections.Generic;
using Rector.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using VYaml.Annotations;
using VYaml.Serialization;

namespace Rector.Editor
{
    public static class VfxAssetReader
    {
        public static List<IInput> GetPropertyInputs(VisualEffect visualEffect)
        {
            var asset = visualEffect.visualEffectAsset;

            // 公式のAPIで取得できるもの
            var exposedProperties = new List<VFXExposedProperty>();
            asset.GetExposedProperties(exposedProperties);

            // .vfxにシリアライズされた値
            var serializableVfxParameterInfos = ReadParameterInfo(asset);

            var result = new List<IInput>(exposedProperties.Count);

            foreach (var exposedProperty in exposedProperties)
            {
                var serializableVfxParameterInfo = Array.Find(serializableVfxParameterInfos,
                    info => info.Name == exposedProperty.name);

                var type = exposedProperty.type;
                switch (type)
                {
                    case not null when type == typeof(float):
                        result.Add(new FloatInput(
                            exposedProperty.name,
                            visualEffect.GetFloat(exposedProperty.name),
                            serializableVfxParameterInfo?.Min ?? float.NegativeInfinity,
                            serializableVfxParameterInfo?.Max ?? float.PositiveInfinity
                        ));
                        break;
                    case not null when type == typeof(int):
                        result.Add(new IntInput(
                            exposedProperty.name,
                            visualEffect.GetInt(exposedProperty.name),
                            (int)(serializableVfxParameterInfo?.Min ?? float.NegativeInfinity),
                            (int)(serializableVfxParameterInfo?.Max ?? float.PositiveInfinity)
                        ));
                        break;
                    case not null when type == typeof(Vector3):
                        result.Add(new Vector3Input(
                            exposedProperty.name,
                            visualEffect.GetVector3(exposedProperty.name)
                        ));
                        break;
                    case not null when type == typeof(bool):
                        result.Add(new BoolInput(
                            exposedProperty.name,
                            visualEffect.GetBool(exposedProperty.name)
                        ));
                        break;
                    default:
                        Debug.LogWarning($"not supported type: {type}");
                        break;
                }
            }

            return result;
        }

        static SerializableVFXParameterInfo[] ReadParameterInfo(VisualEffectAsset asset)
        {
            var text = System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(asset));
            var docs = YamlSerializer.DeserializeMultipleDocuments<SerializableDocument>(text);
            foreach (var doc in docs)
            {
                if (doc is { MonoBehaviour: { ParameterInfo: { } parameterInfo } })
                {
                    return parameterInfo;
                }
            }

            return Array.Empty<SerializableVFXParameterInfo>();
        }

        public static List<string> GetEventNames(VisualEffect visualEffect)
        {
            return ReadEventNames(visualEffect.visualEffectAsset);
        }

        static List<string> ReadEventNames(VisualEffectAsset asset)
        {
            var eventNames = new List<string>();
            var text = System.IO.File.ReadAllBytes(AssetDatabase.GetAssetPath(asset));
            var docs = YamlSerializer.DeserializeMultipleDocuments<SerializableDocument>(text);
            foreach (var doc in docs)
            {
                if (doc is { MonoBehaviour: { EventName: { } eventName } } && !string.IsNullOrEmpty(eventName))
                {
                    eventNames.Add(eventName);
                }
            }

            return eventNames;
        }
    }

    [YamlObject]
    public partial class SerializableDocument
    {
        [YamlMember("MonoBehaviour")] public SerializableMonoBehaviour MonoBehaviour;
    }

    // https://github.com/needle-mirror/com.unity.visualeffectgraph/blob/787767efd455eb421d58ec1109b055a5ad8b0777/Editor/Models/VFXGraph.cs
    [YamlObject]
    public partial class SerializableMonoBehaviour
    {
        // VfxGraph
        [YamlMember("m_ParameterInfo")] public SerializableVFXParameterInfo[] ParameterInfo;

        // Event
        [YamlMember("eventName")] public string EventName;
    }

    // https://github.com/needle-mirror/com.unity.visualeffectgraph/blob/13.0.0/Editor/Models/VFXParameterInfo.cs
    [YamlObject]
    public partial class SerializableVFXParameterInfo
    {
        [YamlMember("name")] public string Name;
        [YamlMember("path")] public string Path;
        [YamlMember("tooltip")] public string Tooltip;
        [YamlMember("sheetType")] public string SheetType;
        [YamlMember("realType")] public string RealType;
        [YamlMember("defaultValue")] public VfxSerializableObject DefaultValue;
        [YamlMember("min")] public float Min;
        [YamlMember("max")] public float Max;
        [YamlMember("enumValues")] public List<string> EnumValues;
        [YamlMember("descendantCount")] public int DescendantCount;

        public override string ToString() =>
            $"Name: {Name}, Path: {Path}, Tooltip: {Tooltip}, SheetType: {SheetType}, RealType: {RealType}, DefaultValue: {DefaultValue}, Min: {Min}, Max: {Max}, EnumValues: {EnumValues}, DescendantCount: {DescendantCount}";
    }

    // https://github.com/needle-mirror/com.unity.visualeffectgraph/blob/787767efd455eb421d58ec1109b055a5ad8b0777/Editor/Core/VFXSerializer.cs
    [YamlObject]
    public partial class VfxSerializableObject
    {
        [YamlMember("m_Type")] public object Type;

        // JSON
        [YamlMember("m_SerializableObject")] public string SerializableObject;

        // NOTE: VisualEffectAsset(=.vfx)にシリアライズされたDefaultValueを読みたい場合は、
        // このクラスに ToFloat などの関数を生やす。
        // SerializableObjectはJSON形式の文字列なのでパースする必要がある
        // Rectorでは VisualEffectAssetではなくVisualEffectオブジェクト(=.prefab)がシリアライズする値を使う
        public override string ToString() => $"Type: {Type}, SerializableObject: {SerializableObject}";
    }
}
