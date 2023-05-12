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
        _conditions = new Stack<ConditionValues>();


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
        buttons.AtIndex(2).RegisterCallback<ClickEvent>(AddCondition);
        buttons.AtIndex(3).RegisterCallback<ClickEvent>(SubCondition);

        //4-5 for adding and subbing from the mandatory animation list

        /*buttons.AtIndex(4).RegisterCallback<ClickEvent>();
        buttons.AtIndex(5).RegisterCallback<ClickEvent>();*/

        buttons.Last().RegisterCallback<ClickEvent>(DebugButton);
    }

    #region Character Information: Stats

    [Header("Stats")] private Stack<StatValues> _stats;
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
        _statNames.Clear();
        _statList.Clear();
        foreach (var stat in _stats)
        {
            _statNames.Add(stat.name);
            VisualElement prefab = statPrefab.CloneTree();

            var prefabName = prefab.Q<TextField>("Stat_name");
            var prefabValue = prefab.Q<TextField>("Stat_value");
            var prefabType = prefab.Q<DropdownField>();

            prefabValue.value = stat.value;
            prefabName.value = stat.name;
            prefabType.value = stat.GetValueType();

            prefabName.RegisterCallback<BlurEvent>(_ =>
            {
                _statNames.Remove(stat.name);
                stat.name = prefabName.value;
                _statNames.Add(stat.name);
            });
            prefabValue.RegisterCallback<BlurEvent>(_ => stat.value = prefabValue.value);
            prefabType.RegisterCallback<BlurEvent>(_ => stat.SetType(prefabType.value));
            _statList.Add(prefab);
            prefab.SendToBack();
        }
    }

    #endregion

    #region Character Information: Conditions

    [Header("Conditions")] [SerializeField]
    private VisualTreeAsset conditionPrefab;

    private VisualElement _conditionList;
    private Stack<ConditionValues> _conditions;

    private List<string> _statNames;

    private void AddCondition(ClickEvent evt)
    {
        ConditionValues newValue = CreateInstance<ConditionValues>();
        _conditions.Push(newValue);
        UpdateConditionUI();
    }

    private void SubCondition(ClickEvent evt)
    {
        if (_conditions.Count == 0) return;
        _conditions.Pop();
        UpdateConditionUI();
    }

    private void UpdateConditionUI()
    {
        _conditionList.Clear();
        foreach (var cond in _conditions)
        {
            VisualElement prefab = conditionPrefab.CloneTree();

            var prefabDdfStat = prefab.Q<DropdownField>("choosable stats");
            var prefabName = prefab.Q<TextField>("condition_name");
            var prefabType = prefab.Q<DropdownField>("type");
            var prefabPos = prefab.Q<Toggle>("is_positive");
            var prefabAmt = prefab.Q<TextField>("amount");
            var prefabRnd = prefab.Q<TextField>("rounds");

            //set all possible stats
            prefabDdfStat.choices = _statNames;

            //Assign ui with actual values
            prefabDdfStat.value = cond.statToChange;
            prefabName.value = cond.name;
            prefabType.value = cond.GetTypeOfCondition();
            prefabPos.value = cond.isPositive;
            prefabAmt.value = cond.change.ToString();
            prefabRnd.value = cond.roundsUntilFinish.ToString();

            prefabDdfStat.RegisterCallback<BlurEvent>(_ => cond.statToChange = prefabDdfStat.value);
            prefabName.RegisterCallback<BlurEvent>(_ => cond.name = prefabName.value);
            prefabType.RegisterCallback<BlurEvent>(_ => cond.isProcentual = prefabType.value == "Procentual");
            prefabPos.RegisterCallback<BlurEvent>(_ => cond.isPositive = prefabPos.value);
            prefabAmt.RegisterCallback<BlurEvent>(_ =>
            {
                try
                {
                    cond.change = Convert.ToInt32(prefabAmt.value);
                }
                catch (FormatException e)
                {
                    Debug.Log("Please put in an integer: " + e);
                    prefabAmt.value = "0";
                    cond.change = 0;
                }
            });
            prefabRnd.RegisterCallback<BlurEvent>(_ =>
            {
                try
                {
                    cond.roundsUntilFinish = Convert.ToInt32(prefabRnd.value);
                }
                catch (FormatException e)
                {
                    Debug.Log("Please enter an Integer: " + e);
                    prefabRnd.value = "0";
                    cond.roundsUntilFinish = 0;
                }
            });

            _conditionList.Add(prefab);
            prefab.SendToBack();
        }
    }

    #endregion

    #region Character Information: Animations

    [Header("Animations")] private List<string> animationNames;

    [SerializeField] private VisualTreeAsset animPrefab;

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
        FloatType
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
            case "float":
                type = StatValueType.FloatType;
                break;
        }
    }

    public string GetValueType()
    {
        switch (type)
        {
            case StatValueType.IntType:
                return "int";
            case StatValueType.FloatType:
                return "float";
        }

        return "idk something!";
    }
}


[Serializable]
public class ConditionValues : ScriptableObject
{
    public bool isProcentual;
    public bool isPositive;
    public new string name;
    public int roundsUntilFinish;
    public int change;
    public string statToChange;

    public override string ToString()
    {
        return string.Format("{0} is the condition for {1} with {2} amount in {3} rounds",
            name, statToChange, change, roundsUntilFinish);
    }

    public void SetStatToChange(string stat)
    {
        statToChange = stat;
    }

    public string GetTypeOfCondition()
    {
        return isProcentual ? "Procentual" : "fixed amount";
    }
}