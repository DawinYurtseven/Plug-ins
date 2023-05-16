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

    private static VisualElement _root;

    public void CreateGUI()
    {


        _root = rootVisualElement;
        // Each editor window contains a root VisualElement object
        visualTree.CloneTree(_root);
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        rootVisualElement.Bind(so);

        
        //prep for the list
        _statList = _root.Q<VisualElement>("Stat_Container");
        _conditionList  = _root.Q<VisualElement>("Conditions_Container");
        _animationList  = _root.Q<VisualElement>("Anim_container");


        var buttons = _root.Query<Button>();

        //0-1 for adding and subbing from the stat list
        buttons.AtIndex(0).RegisterCallback<ClickEvent>(AddStat);
        buttons.AtIndex(1).RegisterCallback<ClickEvent>(SubStat);

        //2-3 for adding and subbing from the conditions list
        buttons.AtIndex(2).RegisterCallback<ClickEvent>(AddCondition);
        buttons.AtIndex(3).RegisterCallback<ClickEvent>(SubCondition);

        //4-5 for adding and subbing from the mandatory animation list

        buttons.AtIndex(4).RegisterCallback<ClickEvent>(AddAnimation);
        buttons.AtIndex(5).RegisterCallback<ClickEvent>(SubAnimation);

        buttons.Last().RegisterCallback<ClickEvent>(DebugButton);
    }

    #region Character Information: Stats

    [Header("Stats")] private readonly Stack<StatValues> _stats = new Stack<StatValues>();
    private  VisualElement _statList;
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
    private readonly Stack<ConditionValues> _conditions = new Stack<ConditionValues>();

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

    [Header("Animations")] private readonly Stack<AnimationNames> _animationNames = new();

    [SerializeField] private VisualTreeAsset animPrefab;

    private VisualElement _animationList;

    private void AddAnimation(ClickEvent evt)
    {
        var animationNames = CreateInstance<AnimationNames>();
        _animationNames.Push(animationNames);
        UpdateAnimationList();
    }

    private void SubAnimation(ClickEvent evt)
    {
        if (_animationNames.Count == 0) return;
        _animationNames.Pop();
        UpdateAnimationList();
    }

    private void UpdateAnimationList()
    {
        _animationList.Clear();
        int index = _animationNames.Count -1;
        foreach (var anim in _animationNames)
        {
            VisualElement prefab = animPrefab.CloneTree();

            var prefabName = prefab.Q<TextField>("AnimName");
            prefabName.label = $"Animation {index}";
            prefabName.value = anim.name;

            prefabName.RegisterCallback<BlurEvent>(_ =>
            {
                anim.name = prefabName.value;
            });
            
            _animationList.Add(prefab);
            prefab.SendToBack();
            index--;
        }
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
        foreach (var cond in _conditions)
        {
            Debug.Log(cond);
        }
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
        return $"{name}: {value} is from type: {type}";
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
        return $"{name} is the condition for {statToChange} with {change} {GetTypeOfCondition()} in " +
               $"{roundsUntilFinish} rounds";
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

[Serializable]
public class AnimationNames : ScriptableObject
{
    public new string name;
    public int animationOrder;

    public override string ToString()
    {
        return $"{name} is in order {animationOrder}";
    }
}