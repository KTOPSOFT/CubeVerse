using System;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>  Attribute to specify the type of the field serialized by the SerializeReference attribute in the inspector.  </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SubclassSelectorAttribute : PropertyAttribute { }


    /// <summary> An attribute that overrides the name of the type displayed in the SubclassSelector popup.  </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface,
        AllowMultiple = false, Inherited = false)]
    public sealed class AddTypeMenuAttribute : Attribute
    {
        public string MenuName { get; }

        public int Order { get; }

        public AddTypeMenuAttribute(string menuName, int order = 0)
        {
            MenuName = menuName;
            Order = order;
        }

        static readonly char[] k_Separeters = new char[] { '/' };

        /// <summary>  Returns the menu name split by the '/' separator.  </summary>
        public string[] GetSplittedMenuName()
        {
            return !string.IsNullOrWhiteSpace(MenuName) ? MenuName.Split(k_Separeters, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();
        }

        /// <summary>  Returns the display name without the path.  </summary>
        public string GetTypeNameWithoutPath()
        {
            return MenuName;
        }

        public string GetTypeNameWithoutPathLast()
        {
            string[] splittedDisplayName = GetSplittedMenuName();
            return (splittedDisplayName.Length != 0) ? splittedDisplayName[splittedDisplayName.Length - 1] : MenuName;
        }
    }
}