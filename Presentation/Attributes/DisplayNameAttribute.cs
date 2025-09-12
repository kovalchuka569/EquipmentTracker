using System;

namespace Presentation.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DisplayNameAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;
}