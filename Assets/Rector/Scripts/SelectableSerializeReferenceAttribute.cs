using System;
using UnityEngine;

namespace Rector
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SelectableSerializeReferenceAttribute : PropertyAttribute
    {
    }
}
