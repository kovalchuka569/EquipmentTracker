using System;

namespace Presentation.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class OrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}