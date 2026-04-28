using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CHM.VisualScriptingKai.Editor
{
    public static class UnitUtility
    {
        public static IEnumerable<string> SearchNames(this IUnit unit)
        {
            var names = new HashSet<string>();
            AddSearchName(names, unit.Name());

            try
            {
                if(unit is Literal literal)
                    AddEnumValueNames(names, literal.value);

                foreach(var defaultValue in unit.defaultValues.Values)
                    AddEnumValueNames(names, defaultValue);

                if(unit is SwitchOnEnum switchOnEnum)
                    AddEnumBranchNames(names, switchOnEnum.enumType);
                else if(unit is SelectOnEnum selectOnEnum)
                    AddEnumBranchNames(names, selectOnEnum.enumType);
            }
            catch(Exception)
            {
                // Keep Graph Lens search best-effort; the normal node name is already included.
            }

            return names;
        }

        public static string Name(this IUnit unit)
        {
            // var option = unit.Option<IUnitOption>();
            // if(option != null)
            //     return option.haystack;
            var description = unit.Description<UnitDescription>();
            bool hasSurtitle = description.surtitle != null && description.surtitle.Length > 0;
            bool hasTitle = description.title != null && description.title.Length > 0;
            if(hasSurtitle && hasTitle)
                return description.surtitle + ": " + description.title;
            else if(hasSurtitle && !hasTitle)
                return description.surtitle;
            else if(!hasSurtitle && hasTitle)
                return description.title;
            else return unit.GetType().HumanName();
        }
        public static EditorTexture Icon(this IUnit unit)
        {
            var description = unit.Description<UnitDescription>();
            return description.icon;
        }
        public static string Name(this IState state)
        {
            var description = state.Description<StateDescription>();
            bool hasTitle = description.title != null && description.title.Length > 0;
            if(hasTitle)
                return description.title;
            return state.GetType().HumanName();
        }
        public static EditorTexture Icon(this IState state)
        {
            var description = state.Description<StateDescription>();
            return description.icon;
        }
        public static string Name(this IStateTransition stateTransition)
        {
            var description = stateTransition.Description<StateTransitionDescription>();
            bool hasTitle = description.title != null && description.title.Length > 0
            && description.title != "(No Event)";
            if(hasTitle)
                return description.title;
            return stateTransition.GetType().HumanName();
        }
        public static EditorTexture Icon(this IStateTransition stateTransition)
        {
            var description = stateTransition.Description<StateTransitionDescription>();
            return description.icon;
        }

        private static void AddEnumBranchNames(HashSet<string> names, Type enumType)
        {
            if(enumType == null || !enumType.IsEnum)
                return;

            var addedValues = new HashSet<Enum>();
            foreach(var valueByName in EnumUtility.ValuesByNames(enumType))
            {
                if(addedValues.Add(valueByName.Value))
                    AddSearchName(names, valueByName.Key);
            }
        }

        private static void AddEnumValueNames(HashSet<string> names, object value)
        {
            if(value is not Enum enumValue)
                return;

            var enumName = enumValue.ToString();
            if(long.TryParse(enumName, out _))
                return;

            AddSearchName(names, enumName);

            foreach(var part in enumName.Split(','))
                AddSearchName(names, part.Trim());
        }

        private static void AddSearchName(HashSet<string> names, string name)
        {
            if(!string.IsNullOrWhiteSpace(name))
                names.Add(name);
        }
    }
}
