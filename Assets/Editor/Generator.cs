using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


/*** structure of the window as followed
        [quick intro to the generator]
        {
            a window about individuals:
            [stats]
            [possible conditions]
            [animations list]
        }
        {
            a window for actions an individual can do. this will be displayed as a list of visual elements:
            [name]
            a function: it is given a type(decrease, increase, toggle for a set turn)
                        for increase and decrease, a set value can be set or a variable
                        for toggle, one of the possible conditions will be toggled for a set of rounds given as a parameter
        }
        [a quick text about the battle system part]
        
 */
public class Generator : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTree;

    [SerializeField] private VisualTreeAsset statPrefab;

    [MenuItem("Window/TBCombat/Generator")]
    public static void ShowExample()
    {
        Generator wnd = GetWindow<Generator>();
        wnd.titleContent = new GUIContent("Generator");
    }
    
    public Stack<Values> Stats;
    private VisualElement _statList;
    private Generator _generator;

    public void CreateGUI()
    {
        _generator = this; 
        Stats = new Stack<Values>();

        var root = rootVisualElement;
        // Each editor window contains a root VisualElement object
        visualTree.CloneTree(root);
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        rootVisualElement.Bind(so);
        _statList = root.Q<VisualElement>("Stat_Container");
        
        Undo.undoRedoPerformed += UpdateStatUI;
        var buttons = root.Query<Button>();
        buttons.AtIndex(0).RegisterCallback<ClickEvent>(AddStat);
        buttons.AtIndex(1).RegisterCallback<ClickEvent>(SubStat);
        buttons.AtIndex(2).RegisterCallback<ClickEvent>(DebugButton);
        
    }

    private void AddStat(ClickEvent evt)
    {
        Values nextValue = CreateInstance<Values>();
        Undo.DestroyObjectImmediate(nextValue);
        Stats.Push(nextValue);
        Debug.Log("try me bitch!!");
        UpdateStatUI();
    }

    private void SubStat(ClickEvent evt)
    {
        if (Stats.Count == 0) return;
        Stats.Pop();
        Debug.Log("Reduce that bitch to atoms!!");
        UpdateStatUI();
    }

    private void UpdateStatUI()
    {
        _statList.Clear();
        foreach (var stat in Stats)
        {
            VisualElement prefab = statPrefab.CloneTree();
            var prefabName = prefab.Q<TextField>("Stat_name");
            var prefabValue = prefab.Q<TextField>("Stat_value");
            var prefabType = prefab.Q<DropdownField>();
            
            
            prefabValue.value = stat.value;
            prefabName.value = stat.name;
            prefabType.value = stat.type.ToString();
            
            prefabName.RegisterCallback<BlurEvent>(evt => stat.name = prefabName.value);
            prefabValue.RegisterCallback<BlurEvent>(evt => stat.value = prefabValue.value);
            prefabType.RegisterCallback<BlurEvent>(evt => stat.setType(prefabType.value));
            _statList.Add(prefab);
            
        }
        _statList.Sort(_=> _statList.IndexOf(_));
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= UpdateStatUI;
    }

    private void DebugButton(ClickEvent evt)
    {
        Debug.Log(Stats.Count);
        if(Stats.Count != 0) Debug.Log(Stats.Peek());
    }
}

public enum ValueType
{
    IntType,
    FloatType,
    StringType
}

[Serializable]
public class Values : ScriptableObject
{
    public ValueType type = ValueType.IntType;
    public new string name = "";
    public string value = "";

    public override string ToString()
    {
        return name + ": " + value + " is from type: " + type;
    }

    public void setType(string type)
    {
        switch (type)
        {
            case "int":
                this.type = ValueType.IntType;
                break;
            case "string":
                this.type = ValueType.StringType;
                break;
            case "float":
                this.type = ValueType.FloatType;
                break;
        }
    }
}