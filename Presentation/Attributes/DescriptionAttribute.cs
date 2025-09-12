using System;

namespace Presentation.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}