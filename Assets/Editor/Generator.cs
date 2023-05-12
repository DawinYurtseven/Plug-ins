using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            
            []check for functionality
            [x]check for ui
        }
        {
            a window for actions an individual can do. this will be displayed as a list of visual elements:
            [name]
            a function: it is given a newType(decrease, increase, toggle for a set turn)
                        for increase and decrease, a set value can be set or a variable
                        for toggle, one of the possible conditions will be toggled for a set of rounds given as a parameter
                        
            []check for functionality
            []check for ui
        }
        [a quick text about the battle system part]
        {
            window about the Game-flow of the battle
            [enemy ai]
            [different turns]
            
            []check for functionality
            []check for ui
        }
 */
public class Generator : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTree;


    [MenuItem("Window/TBCombat/Generator")]
    public static void ShowExample()
    {
        Generator wnd = GetWindow<Generator>();
        wnd.titleContent = new GUIContent("Generator");
    }


    public void CreateGUI()
    {
        //this part is for initializations of the stacks
        _stats = new Stack<StatValues>();


        var root = rootVisualElement;
        // Each editor window contains a root VisualElement object
        visualTree.CloneTree(root);
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        rootVisualElement.Bind(so);
        
        //this part is for the assignment of the VisualElements from the uxml data to this file
        _statList = root.Q<VisualElement>("Stat_Container");
        _conditionList = root.Q<VisualElement>("Conditions_Container");

        var buttons = root.Query<Button>();
        //0-1 for adding and subbing from the stat list
        buttons.AtIndex(0).RegisterCallback<ClickEvent>(AddStat);
        buttons.AtIndex(1).RegisterCallback<ClickEvent>(SubStat);
        //2-3 for adding and subbing from the conditions list

        /*buttons.AtIndex(2).RegisterCallback<ClickEvent>();
        buttons.AtIndex(3).RegisterCallback<ClickEvent>();*/

        //4-5 for adding and subbing from the mandatory animation list

        /*buttons.AtIndex(4).RegisterCallback<ClickEvent>();
        buttons.AtIndex(5).RegisterCallback<ClickEvent>();*/

        buttons.Last().RegisterCallback<ClickEvent>(DebugButton);
    }

    #region Character Information: Stats

    private Stack<StatValues> _stats;
    private VisualElement _statList;
    [SerializeField] private VisualTreeAsset statPrefab;

    private void AddStat(ClickEvent evt)
    {
        StatValues nextStatValue = CreateInstance<StatValues>();
        _stats.Push(nextStatValue);
        UpdateStatUI();
    }

    private void SubStat(ClickEvent evt)
    {
        if (_stats.Count == 0) return;
        _stats.Pop();
        UpdateStatUI();
    }

    private void UpdateStatUI()
    {
        _statList.Clear();
        foreach (var stat in _stats)
        {
            VisualElement prefab = statPrefab.CloneTree();

            var prefabName = prefab.Q<TextField>("Stat_name");
            var prefabValue = prefab.Q<TextField>("Stat_value");
            var prefabType = prefab.Q<DropdownField>();

            prefabValue.value = stat.value;
            prefabName.value = stat.name;
            prefabType.value = stat.GetValueType();

            prefabName.RegisterCallback<BlurEvent>(_ => stat.name = prefabName.value);
            prefabValue.RegisterCallback<BlurEvent>(_ => stat.value = prefabValue.value);
            prefabType.RegisterCallback<BlurEvent>(_ => stat.SetType(prefabType.value));
            _statList.Add(prefab);
            prefab.SendToBack();
        }

        Debug.Log(_stats.Count);
    }

    #endregion

    #region Character Information: Conditions

    [SerializeField] private VisualTreeAsset _conditionPrefab;

    private VisualElement _conditionList;
    //private Stack<Condition>

    private void AddCondition(ClickEvent evt)
    {
        
    }

    #endregion
    
    /*
     * this is for when I'm able to understand the "Undo" class better
     */
    /*private void OnEnable()
    {
        Undo.undoRedoPerformed += UpdateStatUI;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= UpdateStatUI;
    }*/

    private void DebugButton(ClickEvent evt)
    {
        //TODO: try to understand file writing and reading in c#
    }
}

 


[Serializable]
public class StatValues : ScriptableObject
{
    public enum StatValueType
    {
        IntType,
        FloatType,
        StringType
    }
    public StatValueType type = StatValueType.IntType;
    public new string name = "";
    public string value = "";

    public override string ToString()
    {
        return name + ": " + value + " is from newType: " + type;
    }

    public void SetType(string newType)
    {
        switch (newType)
        {
            case "int":
                type = StatValueType.IntType;
                break;
            case "string":
                type = StatValueType.StringType;
                break;
            case "float":
                type = StatValueType.FloatType;
                break;
        }
    }

    public string GetValueType()
    {
        switch (type)
        {
            case StatValueType.StringType:
                return "string";
            case StatValueType.IntType:
                return "int";
            case StatValueType.FloatType:
                return "float";
        }

        return "idk something!";
    }
}


public class ConditionValues : ScriptableObject
{
    public enum ConditionType
    {
        StatAfflictive,
        HpAfflictive,
        MpAfflictive
    }

    public bool isPositive;
    public new string name;
    
}