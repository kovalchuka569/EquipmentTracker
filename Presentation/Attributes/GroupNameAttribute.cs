using System;

namespace Presentation.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class GroupNameAttribute(string groupName) : Attribute
{
    public string GroupName { get; } = groupName;
}