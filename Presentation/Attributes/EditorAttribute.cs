using System;
using Presentation.Enums;

namespace Presentation.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EditorAttribute(EditorType editorKey) : Attribute
{
    public EditorType EditorType { get; } = editorKey;
}