using System;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public interface IParameterField
    {
        Type Type { get; }
        int NumRows { get; }
        object Field(string valName, Type valType, object val, Rect? rect);
    }
}