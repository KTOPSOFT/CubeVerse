using MalbersAnimations.Events;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
namespace MalbersAnimations.Utilities
{
    #region Material Changer
    /// <summary>Is used to change Materials on any Mesh Renderer using a list of Materials Items </summary>
    [AddComponentMenu("Malbers/Utilities/Mesh/Material Changer")]
    public class MaterialChanger : MonoBehaviour
    {
        public List<MaterialItem> materialList = new();
        public bool showMeshesList = true;
        public bool changeHidden = false;

        public bool random;

        /// <summary>All Material Changer Index Stored on a string separated by a space ' '</summary>
        public string AllIndex
        {
            set
            {
                string[] getIndex = value.Split(' ');

                for (int i = 0; i < materialList.Count; i++)
                {
                    if (getIndex.Length > i)
                    {

                        if (int.TryParse(getIndex[i], out int index))
                        {
                            if (index == -1) continue;

                            materialList[i].ChangeMaterial(index);
                        }
                    }
                }
            }

            get
            {
                string AllIndex = "";

                for (int i = 0; i < materialList.Count; i++)
                {
                    var materialItem = materialList[i];

                    if (materialItem != null)
                    { AllIndex += materialItem.current.ToString() + " "; }
                }

                AllIndex.Remove(AllIndex.Length - 1);   //Remove the last space }
                return AllIndex;
            }
        }

        public MaterialItem this[int index]
        {
            get => materialList[index];
            set => materialList[index] = value;
        }

        public int Count => materialList.Count;

        private void OnEnable()
        {
            foreach (var mat in materialList)
            {
                if (mat.Linked && mat.Master >= 0 && mat.Master < Count)   //If the master material item is in range
                {
                    materialList[mat.Master].OnMaterialChanged.AddListener(mat.ChangeMaterial);         //Used for linked materials
                }
            }

            if (random) Randomize();
        }

        private void OnDisable()
        {
            foreach (var mat in materialList)
            {
                if (mat.Linked && mat.Master >= 0 && mat.Master < Count)   //If the master material item is in range
                {
                    materialList[mat.Master].OnMaterialChanged.RemoveListener(mat.ChangeMaterial);         //Used for linked materials
                }
            }
        }

        public virtual void Randomize()
        {
            foreach (var mat in materialList)
            {
                if (!mat.Linked) mat.ChangeMaterial(UnityEngine.Random.Range(0, mat.materials.Length));
            }
        }


        /// <summary>Swap to the next or previous material on each Material Item </summary>
        public virtual void SetAllMaterials(bool Next = true)
        {
            foreach (var materialItem in materialList)
            {
                materialItem.ChangeMaterial(Next);
            }
        }

        /// <summary> Set all the MaterialItems on the List a specific Material using an Index  </summary>
        /// <param name="index">the index on the Materials[], for each Material Item</param>
        public virtual void SetAllMaterials(int index)
        {
            foreach (var mat in materialList)
            {
                mat.ChangeMaterial(index);
            }
        }

        /// <summary> Set a Material from the List of material inside the materialList...   </summary>
        /// <param name="indexList">index of the Material List</param>
        /// <param name="nextIndex">index a material on the MaterialList</param>
        public virtual void SetMaterial(int indexList, int nextIndex)
        {
            if (indexList < 0) indexList = 0;
            indexList %= Count;

            if (materialList[indexList] != null)
                materialList[indexList].ChangeMaterial(nextIndex);
        }

        /// <summary>  Set a Material from the List of material inside the materialList...   </summary>
        /// <param name="index"></param>
        /// <param name="next"></param>
        public virtual void SetMaterial(int index, bool next = true)
        {
            if (index < 0) index = 0;
            index %= Count;

            if (materialList[index] != null)
            {
                materialList[index].ChangeMaterial(next);
            }
        }

        public virtual void SetMaterial(string name, int Index)
        {
            MaterialItem materialItem = materialList.Find(item => item.Name == name);

            if (materialItem != null)
            {
                materialItem.ChangeMaterial(Index);
            }
            else
            {
                Debug.LogWarning("No material Item Found with the name: " + name);
            }
        }

        public virtual void SetMaterial(string name, bool next = true)
        {
            MaterialItem materialItem = materialList.Find(item => item.Name == name);

            if (materialItem != null)
            {
                materialItem.ChangeMaterial(next);
            }
            else
            {
                Debug.LogWarning("No material Item Found with the name: " + name);
            }
        }

        /// <summary>Set all the MaterialItems on the List an External Material</summary>
        /// <param name="mat"></param>
        public virtual void SetAllMaterials(Material mat)
        {
            foreach (var MaterialItem in materialList)
            {
                MaterialItem.ChangeMaterial(mat);
            }
        }


        /// <summary> Swap to the Next material on a specific Material Item on the List using index </summary>
        /// <param name="index">index on the Material Item on the material list</param>
        public virtual void NextMaterialItem(int index)
        {
            if (index < 0) index = 0;
            index %= Count;

            materialList[index].NextMaterial();
        }

        /// <summary> Swap to the Next material on a specific Material Item on the List using the Name  </summary>
        /// <param name="name">the Name used for the MaterialItem</param>
        public virtual void NextMaterialItem(string name)
        {
            MaterialItem mat = materialList.Find(item => item.Name.ToUpper() == name.ToUpper());

            if (mat != null) mat.NextMaterial();
        }

        /// <summary>  Returns the Current Index on the material list using the index of the slot  </summary>
        public virtual int CurrentMaterialIndex(int index)
        {
            return materialList[index].current;
        }

        /// <summary>Returns the Current index of the material list slot using the index of the slot /// </summary>
        public virtual int CurrentMaterialIndex(string name)
        {
            int index = materialList.FindIndex(item => item.Name == name);
            return materialList[index].current;
        }

#if UNITY_EDITOR
        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {
            MEventListener listener =
                 (this.FindComponent<MEventListener>()
                 ?? transform.root.GetComponentInChildren<MEventListener>())
                 ?? transform.root.gameObject.AddComponent<MEventListener>();
            listener.Events ??= new List<MEventItemListener>();

            MEvent BlendS = MTools.GetInstance<MEvent>("Material Changer");

            if (listener.Events.Find(item => item.Event == BlendS) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = BlendS,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, NextMaterialItem);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, NextMaterialItem);
                listener.Events.Add(item);
                UnityEditor.EditorUtility.SetDirty(listener);
                Debug.Log("<B>Material Changer</B> Added to the Event Listeners");
            }

            MTools.SetDirty(listener);
        }
#endif

    }

    /// <summary>Slot on the List of Materials Items</summary>
    [System.Serializable]
    public class MaterialItem
    {
        /// <summary>The name for the Material Item</summary>
        public string Name;

        /// <summary>The mesh renderer to use for the materials</summary>
        public Renderer mesh;

        /// <summary>The list of the Materials on the material Item</summary>
        public Material[] materials;

        /// <summary>Is the Material Item linked to another Material Item</summary>
        public bool Linked = false;

        /// <summary> If the material is Linked this is the Master ID </summary>
        public int Master = 0;

        /// <summary> Current Index Material Item (Which material is being used on the material List</summary>
        public int current = 0;

        /// <summary> Does this material Item has another meshes that uses the same Material?</summary>
        public bool HasLODs;
        /// <summary> List of other meshes that same Material?</summary>
        public Renderer[] LODs;

        /// <summary> Material ID (Used when a mesh have multiple materials)</summary>
        [Tooltip("Material ID (Used when a mesh have multiple materials) Default 0")]
        public int indexM = 0;

        public IntEvent OnMaterialChanged = new IntEvent();

        #region Constructors
        public MaterialItem()
        {
            Name = "NameHere";
            mesh = null;
            materials = new Material[0];
        }

        public MaterialItem(MeshRenderer MR)
        {
            Name = "NameHere";
            mesh = MR;
            materials = new Material[0];
        }

        public MaterialItem(string name, MeshRenderer MR, Material[] mats)
        {
            Name = name;
            mesh = MR;
            materials = mats;
        }

        public MaterialItem(string name, MeshRenderer MR)
        {
            Name = name;
            mesh = MR;
            materials = new Material[0];
        }
        #endregion

        /// <summary> Changes to the next material on the list..(Same as NextMaterial) </summary>
        public virtual void ChangeMaterial()
        {
            current++;                                                  //Move to the Next Material
            if (current < 0) current = 0;                               //If we get to lower than 0 clamp to zero
            current %= materials.Length;

            Material[] currentMaterial = mesh.sharedMaterials;         //Create a copy of the Current materials 

            if (materials[current] != null)                 //Check on the list that the next material is NOT Empty
            {
                currentMaterial[indexM] = materials[current];       //Change only the MaterialID on the Item
                mesh.sharedMaterials = currentMaterial;
                ChangeLOD(current);
                OnMaterialChanged.Invoke(current);
            }
            else
            {
                Debug.LogWarning("The Material on the Slot: " + current + " is empty");
            }
        }

        public virtual void Set_by_BinaryIndex(int binaryCurrent)
        {
            int current = 0;

            for (int i = 0; i < materials.Length; i++)
            {
                if (MTools.IsBitActive(binaryCurrent, i))
                {
                    current = i;        //find the first active bit and get the current
                    break;
                }
            }
            ChangeMaterial(current);
        }


        internal void ChangeLOD(int index)
        {
            if (!HasLODs) return;

            foreach (var mesh in LODs)
            {
                if (mesh == null) break;

                Material[] currentMaterial = mesh.sharedMaterials;

                var clampIndex = Mathf.Clamp(indexM, 0, currentMaterial.Length - 1);

                currentMaterial[clampIndex] = materials[current];
                if (materials[current] != null)
                    mesh.sharedMaterials = currentMaterial;
            }
        }

        internal void ChangeLOD(Material mat)
        {
            if (!HasLODs) return;

            Material[] currentMaterial = mesh.sharedMaterials;
            currentMaterial[indexM] = mat;

            foreach (var mesh in LODs)
            {
                mesh.sharedMaterials = currentMaterial;
            }
        }

        /// <summary>Changes to the Next material on the list.(Same as ChangeMaterial)</summary>
        public virtual void NextMaterial() { ChangeMaterial(); }


        /// <summary>Used for Change a specific material on the list using and Index. </summary>
        /// <param name="index">Index for the Material Array</param>
        public virtual void ChangeMaterial(int index)
        {
            if (materials.Length == 0) return;

            index = Mathf.Clamp(index, 0, materials.Length);

            var mat = materials[index];

            if (mat != null && mesh != null)
            {
                Material[] currentMaterial = mesh.sharedMaterials;

                if (currentMaterial.Length - 1 < indexM)
                {
                    Debug.LogWarning("The Meshes on the " + Name + " Material Item, does not have " + (indexM + 1) + " Materials, please change the ID parameter to value lower than " + currentMaterial.Length);
                    return;
                }

                currentMaterial[indexM] = mat;

                mesh.sharedMaterials = currentMaterial;
                current = index;
                ChangeLOD(index);
                OnMaterialChanged.Invoke(current);
            }
            else
            {
                Debug.LogWarning("The material on the Slot: " + index + "  is empty");
            }
        }

        /// <summary>Changes to the previous material on the list.</summary>
        public virtual void PreviousMaterial()
        {
            current--;
            if (current < 0) current = materials.Length - 1;

            if (materials[current] != null)
            {
                Material[] currentMaterial = mesh.sharedMaterials;
                currentMaterial[indexM] = materials[current];

                mesh.sharedMaterials = currentMaterial;
                ChangeLOD(current);
                OnMaterialChanged.Invoke(current);
            }
            else
            {
                Debug.LogWarning("The Material on the Slot: " + current + " is empty");
            }
        }

        /// <summary>Changes to a specific External material</summary>
        public virtual void ChangeMaterial(Material mat)
        {
            Material[] currentMaterial = mesh.sharedMaterials;
            currentMaterial[indexM] = mat;

            mesh.sharedMaterials = currentMaterial;
            ChangeLOD(mat);
        }

        /// <summary>Changes to the Next or Previous material on the list</summary>
        /// <param name="Next">true: Next, false: Previous</param>
        public virtual void ChangeMaterial(bool Next)
        {
            if (Next)
                ChangeMaterial();
            else
                PreviousMaterial();
        }
    }
    #endregion

    #region Material Changer Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(MaterialChanger))]
    public class MaterialChangerEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty materialList, showMeshesList, random, changeHidden;
        private MaterialChanger M;

        private void OnEnable()
        {
            M = ((MaterialChanger)target);

            materialList = serializedObject.FindProperty("materialList");
            showMeshesList = serializedObject.FindProperty("showMeshesList");
            changeHidden = serializedObject.FindProperty("changeHidden");
            random = serializedObject.FindProperty("random");

            list = new ReorderableList(serializedObject, materialList, true, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = HeaderCallbackDelegate,
                onAddCallback = OnAddCallBack
            };
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Swap Materials");

            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                list.DoLayoutList();
                EditorGUI.indentLevel++;

                if (showMeshesList.boolValue)
                {
                    if (list.index != -1 && materialList.arraySize > list.index)
                    {
                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            var Element = materialList.GetArrayElementAtIndex(list.index);

                            if (Element != null)
                            {
                                EditorGUILayout.PropertyField(Element, new GUIContent(Element.FindPropertyRelative("Name").stringValue), false);


                                if (Element.isExpanded)
                                {
                                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                    {
                                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("mesh"), new GUIContent("Mesh", "Mesh object to apply the Materials"));
                                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("indexM"), new GUIContent("ID", "Material ID"));
                                    }


                                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                    {
                                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("materials"), new GUIContent("Materials"), true);
                                    }

                                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                    {
                                        SerializedProperty hasLODS = Element.FindPropertyRelative("HasLODs");
                                        EditorGUILayout.PropertyField(hasLODS, new GUIContent("LODs", "Has Level of Detail Meshes"));
                                        if (hasLODS.boolValue)
                                        {
                                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("LODs"), new GUIContent("Meshes", "Has Level of Detail Meshes"), true);
                                        }
                                    }


                                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                                    {
                                        EditorGUIUtility.labelWidth = 65;
                                        var linked = Element.FindPropertyRelative("Linked");

                                        EditorGUILayout.PropertyField(linked, new GUIContent("Linked", "This Material Item will be driven by another Material Item"));
                                        if (linked.boolValue)
                                        {
                                            var Master = Element.FindPropertyRelative("Master");
                                            EditorGUILayout.PropertyField(Master, new GUIContent("Master", "Which MaterialItem Index is the Master"));

                                            if (Master.intValue >= materialList.arraySize)
                                            {
                                                Master.intValue = materialList.arraySize - 1;
                                            }
                                        }
                                        EditorGUIUtility.labelWidth = 0;
                                    }


                                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                    {
                                        EditorGUILayout.PropertyField(Element.FindPropertyRelative("OnMaterialChanged"), new GUIContent("On Material Changed", "Invoked when a material item index changes"));
                                    }
                                }
                            }
                        }
                    }
                }
                EditorGUI.indentLevel--;

                if (cc.changed)
                { Undo.RecordObject(target, "Move Handles"); }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_0 = new(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_01 = new(rect.x + 14, rect.y, 35, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new(rect.x + 14 + 25, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new(rect.x + 35 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 25, EditorGUIUtility.singleLineHeight);
            showMeshesList.boolValue = EditorGUI.ToggleLeft(R_0, new GUIContent("", "Show the Material Items when Selected"), showMeshesList.boolValue);

            EditorGUI.LabelField(R_01, new GUIContent(" #", "Index"), EditorStyles.miniLabel);
            EditorGUI.LabelField(R_1, "Material Items", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "Current", EditorStyles.centeredGreyMiniLabel);
            Rect R_3 = new(rect.width + 5, rect.y + 1, 20, EditorGUIUtility.singleLineHeight - 2);

            Rect R_4 = new(rect.width - 25, rect.y + 1, 30, EditorGUIUtility.singleLineHeight - 2);
            random.boolValue = GUI.Toggle(R_3, random.boolValue, new GUIContent("R", "Random Material on Start"), EditorStyles.miniButton);
            changeHidden.boolValue = GUI.Toggle(R_4, changeHidden.boolValue, new GUIContent("CH", "Change Material on Hidden Objects"), EditorStyles.miniButton);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = materialList.GetArrayElementAtIndex(index);
            rect.y += 2;

            Rect R_0 = new(rect.x, rect.y, (rect.width - 65) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new(rect.x + 25, rect.y, (rect.width - 65) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new(rect.x + 25 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2) - 8, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_0, "(" + index.ToString() + ")", EditorStyles.label);

            var nam = element.FindPropertyRelative("Name");

            nam.stringValue = EditorGUI.TextField(R_1, nam.stringValue, EditorStyles.label);
            string buttonCap = "None";

            var e = M.materialList[index];

            if (e.mesh != null)
            {

                using (new EditorGUI.DisabledGroupScope(!changeHidden.boolValue && !e.mesh.gameObject.activeSelf || e.materials.Length == 0 || e.Linked))
                {
                    if (e.materials.Length > e.current)
                    {
                        buttonCap = /*e.mesh.gameObject.activeSelf ? */
                            (e.materials[e.current] == null ? "None" : e.materials[e.current].name) + " (" + (e.Linked ? "L" : e.current.ToString()) + ")";//: "Is Hidden";
                    }

                    if (GUI.Button(R_2, buttonCap, EditorStyles.miniButton))
                    {
                        ToggleButton(index);
                    }
                }

            }
        }

        void ToggleButton(int index)
        {
            if (M.materialList[index].mesh != null)
            {
                Undo.RecordObject(target, "Change Material");
                Undo.RecordObject(M.materialList[index].mesh, "Change Material");

                M.materialList[index].ChangeMaterial();

                //Check for linked Mateeriials

                foreach (var mat in M.materialList)
                {
                    if (mat.Linked && mat.Master >= 0 && mat.Master < M.materialList.Count)
                    {
                        Undo.RecordObject(mat.mesh, "Change Material");
                        mat.ChangeMaterial(M.materialList[mat.Master].current);
                    }
                }
                serializedObject.ApplyModifiedProperties();
                //UnityEditor.EditorUtility.SetDirty(M.materialList[index].mesh);
            }
        }

        void OnAddCallBack(ReorderableList list)
        {
            if (M.materialList == null)
            {
                M.materialList = new System.Collections.Generic.List<MaterialItem>();
            }
            M.materialList.Add(new MaterialItem());
        }
    }
#endif
    #endregion
}

