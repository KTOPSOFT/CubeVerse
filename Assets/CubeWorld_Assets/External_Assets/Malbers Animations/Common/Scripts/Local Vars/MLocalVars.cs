using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using MalbersAnimations.Scriptables;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/scriptable-architecture/mlocal-variables")]
    [AddComponentMenu("Malbers/Runtime Vars/Local Variables")]
    public class MLocalVars : MonoBehaviour
    {
        public (string name, object value) list;
        public List<LocalVar> variables = new();
        public Dictionary<string, object> vars;

        private (string name, int index) PinVar;

        public void Start()
        {
            vars = new Dictionary<string, object>();

            foreach (var item in variables)
            {
                vars.Add(item.name, item.GetValue());
            }

            //Reset Pin Var
            PinVar.name = string.Empty;
            PinVar.index = -1;
        }

        public void SetVar(LocalVar newvar)
        {
            var newValue = newvar.GetValue();

            if (vars.ContainsKey(newvar.name))
            {
                vars[name] = newValue;

#if UNITY_EDITOR
                //Update the variables list with the new value. The list is not needed at runtime
                variables.Find(v => v.name == newvar.name && v.type == newvar.type).SetValue(newValue); //Update the list
#endif
            }
            else
            {
                //Add to the dictionary
                vars.Add(newvar.name, newValue);
#if UNITY_EDITOR
                //Add the variables list with the new value. The list is not needed at runtime
                variables.Add(newvar);
#endif

                Debug.Log($"Variable {newvar.name} Added to the Local Vars");
            }
        }

        public void SetVar<T>(string name, T value)
        {
            if (vars.ContainsKey(name))
            {
                vars[name] = value;
#if UNITY_EDITOR
                //Update the variables list with the new value. The list is not needed at runtime
                variables.Find(v => v.name == name).SetValue(value);
#endif
            }
            else
            {
                vars.Add(name, value); //Update the dictionary

#if UNITY_EDITOR
                //Add the variables list with the new value. The list is not needed at runtime
                var newVar = new LocalVar() { name = name };
                newVar.SetValue<T>(value);
                variables.Add(newVar);
#endif
            }
        }

        public T GetVar<T>(string name)
        {
            Pin_Var(name);
            if (PinVar.index == -1) return default;

            return vars.Get<T>(name);
        }


        public virtual bool HasVar(string name) => vars.ContainsKey(name);
        public virtual bool HasVar(LocalVar var) => vars.ContainsKey(var.name);

        public void Pin_Var(string name)
        {
            if (vars.ContainsKey(name))
            {
                PinVar.name = name;
                PinVar.index = variables.FindIndex(x => x.name == name);
            }
            else
            {
                Debug.LogWarning($"[{transform.name}] - [Local Variables]  does not contain the var <{name}>", this);
                PinVar.name = string.Empty;
                PinVar.index = -1;
            }
        }

        public void SetTrue(string name) => Var_Set_True(name);
        public void Var_Set_True(string name)
        {
            Pin_Var(name);
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                SetVar(name, true);
            }
        }

        public void SetFalse(string name) => Var_Set_True(name);
        public void Var_Set_False(string name)
        {
            Pin_Var(name);
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                SetVar(name, false);
            }
        }


        public virtual void Pin_SetValue(int value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].intValue = value; //update variable list
            }
        }


        public virtual void Pin_SetValue(float value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].floatValue = value; //update variable list
            }
        }

        public virtual void Pin_SetValue(bool value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].boolValue = value; //update variable list
            }
        }

        public virtual void Pin_SetValue(string value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].stringValue = value; //update variable list
            }
        }

        public virtual void Pin_SetValue(Vector2 value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].vector2Value = value; //update variable list
            }
        }

        public virtual void Pin_SetValue(Vector3 value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].vector3Value = value; //update variable list
            }
        }

        public virtual void Pin_SetValue(GameObject value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].gameObjectValue.Value = value; //update variable list
            }
        }

        public virtual void Pin_SetValue(Transform value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].transformValue.Value = value; //update variable list
            }
        }

        public virtual void Pin_SetValue(Material value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].materialValue = value; //update variable list
            }
        }

        public virtual void Pin_SetValue(Object value)
        {
            if (!string.IsNullOrEmpty(PinVar.name))
            {
                vars[PinVar.name] = value; //Update the dictionary
                if (PinVar.index != -1) variables[PinVar.index].objectValue = value; //update variable list
            }
        }


        public bool Compare(LocalVar value, ComparerInt compare = ComparerInt.Equal)
        {
            switch (value.type)
            {
                case LocalVar.VarType.Int:
                    var INT = GetVar<int>(value.name);
                    return value.intValue.CompareInt(INT, compare);
                case LocalVar.VarType.Float:
                    var FLOAT = GetVar<float>(value.name);
                    return value.floatValue.CompareFloat(FLOAT, compare);
                case LocalVar.VarType.Bool:
                    var BOOL = GetVar<bool>(value.name);
                    return value.boolValue == BOOL;
                case LocalVar.VarType.String:
                    var STRING = GetVar<string>(value.name);
                    return value.stringValue == STRING;
                case LocalVar.VarType.Vector3:
                    var VECTOR3 = GetVar<Vector3>(value.name);
                    return value.vector3Value == VECTOR3;
                case LocalVar.VarType.Vector2:
                    var VECTOR2 = GetVar<Vector2>(value.name);
                    return value.vector2Value == VECTOR2;
                case LocalVar.VarType.GameObject:
                    var GO = GetVar<GameObject>(value.name);
                    return value.gameObjectValue.Value == GO;
                case LocalVar.VarType.Transform:
                    var trans = GetVar<Transform>(value.name);
                    return trans == value.transformValue.Value;
                case LocalVar.VarType.Material:
                    var MAT = GetVar<Material>(value.name);
                    return value.materialValue == MAT;
                case LocalVar.VarType.UnityObject:
                    var UOBJ = GetVar<Object>(value.name);
                    return value.objectValue == UOBJ;
                default:
                    return false;
            }
        }
    }


    [System.Serializable]
    public class LocalVar
    {
        public string name;
        public VarType type;

        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue;
        public Vector3 vector3Value;
        public Vector2 vector2Value;
        public GameObjectReference gameObjectValue;
        public TransformReference transformValue;
        public Material materialValue;
        public Object objectValue;

        public object GetValue()
        {
            return type switch
            {
                VarType.Int => intValue,
                VarType.Float => floatValue,
                VarType.Bool => boolValue,
                VarType.String => stringValue,
                VarType.Vector3 => vector3Value,
                VarType.Vector2 => vector2Value,
                VarType.GameObject => gameObjectValue,
                VarType.Transform => transformValue,
                VarType.Material => materialValue,
                VarType.UnityObject => objectValue,
                _ => null,
            };
        }

        public void SetValue(object value)
        {
            switch (type)
            {
                case VarType.Int:
                    intValue = (int)value;
                    break;
                case VarType.Float:
                    floatValue = (float)value;
                    break;
                case VarType.Bool:
                    boolValue = (bool)value;
                    break;
                case VarType.String:
                    stringValue = (string)value;
                    break;
                case VarType.Vector3:
                    vector3Value = (Vector3)value;
                    break;
                case VarType.Vector2:
                    vector2Value = (Vector2)value;
                    break;
                case VarType.GameObject:
                    gameObjectValue.Value = (GameObject)value;
                    break;
                case VarType.Transform:
                    transformValue.Value = (Transform)value;
                    break;
                case VarType.Material:
                    materialValue = (Material)value;
                    break;
                case VarType.UnityObject:
                    objectValue = (Object)value;
                    break;
                default:
                    break;
            }
        }

        public void SetValue(VarType type, object value)
        {
            this.type = type;
            SetValue(value);
        }

        public void SetValue<T>(object value)
        {
            var type = typeof(T);

            if (type == typeof(int)) { SetValue(VarType.Int, (int)value); }
            else if (type == typeof(float)) { SetValue(VarType.Float, (float)value); }
            else if (type == typeof(bool)) { SetValue(VarType.Bool, (bool)value); }
            else if (type == typeof(string)) { SetValue(VarType.String, (string)value); }
            else if (type == typeof(Vector3)) { SetValue(VarType.Vector3, (Vector3)value); }
            else if (type == typeof(Vector2)) { SetValue(VarType.Vector2, (Vector2)value); }
            else if (type == typeof(GameObject)) { SetValue(VarType.GameObject, (GameObject)value); }
            else if (type == typeof(Transform)) { SetValue(VarType.Transform, (Transform)value); }
            else if (type == typeof(Material)) { SetValue(VarType.Material, (Material)value); }
            else if (type == typeof(Object)) { SetValue(VarType.UnityObject, (Object)value); }
        }

        public enum VarType { Int, Float, Bool, String, Vector3, Vector2, GameObject, Transform, Material, UnityObject }
    }


    #region EDITOR STUFF

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LocalVar))]
    public class LocalVarAttribute : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var name = property.FindPropertyRelative("name");
            var type = property.FindPropertyRelative("type");
            var value = property.FindPropertyRelative("value");

            var indent = EditorGUI.indentLevel;

            EditorGUI.indentLevel = 0;

            var height = EditorGUIUtility.singleLineHeight;

            var NameRect = new Rect(position) { height = height };
            NameRect.width /= 3;

            var TypeRect = new Rect(position) { height = height };
            TypeRect.x += NameRect.width + 5;
            TypeRect.width /= 3;

            var ValueRect = new Rect(position) { height = height };
            ValueRect.x += NameRect.width + TypeRect.width + 10 + 15;
            ValueRect.width = ValueRect.width / 3 - 12 - 15;

            EditorGUIUtility.labelWidth = 25;

            EditorGUI.PropertyField(NameRect, name, new GUIContent("Var", "Variable Name-Key used for the Dictionary"));
            EditorGUIUtility.labelWidth = 0;
            EditorGUI.PropertyField(TypeRect, type, GUIContent.none);

            switch ((LocalVar.VarType)type.intValue)
            {
                case LocalVar.VarType.Int:
                    var intValue = property.FindPropertyRelative("intValue");
                    EditorGUI.PropertyField(ValueRect, intValue, GUIContent.none);
                    break;
                case LocalVar.VarType.Float:
                    var floatValue = property.FindPropertyRelative("floatValue");
                    EditorGUI.PropertyField(ValueRect, floatValue, GUIContent.none);
                    break;
                case LocalVar.VarType.Bool:
                    var boolValue = property.FindPropertyRelative("boolValue");
                    EditorGUI.PropertyField(ValueRect, boolValue, GUIContent.none);
                    break;
                case LocalVar.VarType.String:
                    var stringValue = property.FindPropertyRelative("stringValue");
                    EditorGUI.PropertyField(ValueRect, stringValue, GUIContent.none);
                    break;
                case LocalVar.VarType.Vector3:
                    var vector3Value = property.FindPropertyRelative("vector3Value");
                    EditorGUI.PropertyField(ValueRect, vector3Value, GUIContent.none);
                    break;
                case LocalVar.VarType.Vector2:
                    var vector2Value = property.FindPropertyRelative("vector2Value");
                    EditorGUI.PropertyField(ValueRect, vector2Value, GUIContent.none);
                    break;
                case LocalVar.VarType.GameObject:
                    var gameObjectValue = property.FindPropertyRelative("gameObjectValue");
                    EditorGUI.PropertyField(ValueRect, gameObjectValue, GUIContent.none);

                    break;
                case LocalVar.VarType.Transform:
                    var transformValue = property.FindPropertyRelative("transformValue");
                    EditorGUI.PropertyField(ValueRect, transformValue, GUIContent.none);
                    break;
                case LocalVar.VarType.Material:
                    var materialValue = property.FindPropertyRelative("materialValue");
                    EditorGUI.PropertyField(ValueRect, materialValue, GUIContent.none);
                    break;
                case LocalVar.VarType.UnityObject:
                    var objectValue = property.FindPropertyRelative("objectValue");
                    EditorGUI.PropertyField(ValueRect, objectValue, GUIContent.none);
                    break;
                default:
                    break;
            }


            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }



    [CustomEditor(typeof(MLocalVars))]
    public class MLocalVarsEditor : Editor
    {
        SerializedProperty variables;
        MLocalVars M;

        private void OnEnable()
        {
            variables = serializedObject.FindProperty("variables");

            M = target as MLocalVars;
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(variables);

                if (cc.changed && Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties(); //update the value then pass it to the dictionary  (EDITOR ONLY)
                    foreach (var item in M.variables)
                    {
                        M.vars[item.name] = item.GetValue();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}