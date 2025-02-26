using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class OffsetFieldAttribute : PropertyAttribute
{
    public int Value;
    public OffsetFieldAttribute(int value = 1) 
    { 
        Value = value;
    }
}
