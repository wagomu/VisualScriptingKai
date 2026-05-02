using UnityEngine;
using Unity.VisualScripting;
using UnityEditor;

namespace CHM.VisualScriptingKai.Editor
{
    /// <summary>
    /// Data structure that can be used to locate a string value in a node.
    /// Shows the matched string prominently in the info display.
    /// </summary>
    public struct LiteralStringTrace : IGraphElementTrace
    {
        public readonly IGraphElement GraphElement => unit;
        public IUnit unit;
        public string literalString;
        public GraphReference Reference { get; set; }
        public GraphSource Source { get; set; }
        public long Score { get; set; }
        public readonly Vector2 GraphPosition => unit.position;
        public readonly int CompareTo(IGraphElementTrace other)
        => this.DefaultCompareTo(other);
        public readonly string GetInfo()
        {
            return $"<b><size=14>{literalString}</size></b>"
            + $"\n{unit.Name()} | {Source.Info}";
        }
        public readonly Texture2D GetIcon(int resolution)
        {
            return unit.Icon()[resolution];
        }
    }
}
