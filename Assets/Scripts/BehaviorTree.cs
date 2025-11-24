using System.Collections.Generic;
using UnityEngine;

public abstract class Node
{
    public abstract bool Execute();
}

public class ActionNode : Node
{
    private System.Action action;

    public ActionNode(System.Action action)
    {
        this.action = action;
    }

    public override bool Execute()
    {
        action.Invoke();
        return true; // Assuming action always succeeds
    }
}

public class ConditionNode : Node
{
    private System.Func<bool> condition;

    public ConditionNode(System.Func<bool> condition)
    {
        this.condition = condition;
    }

    public override bool Execute()
    {
        return condition.Invoke();
    }
}

public class SequenceNode : Node
{
    private List<Node> nodes = new List<Node>();

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }

    public override bool Execute()
    {
        foreach (var node in nodes)
        {
            if (!node.Execute())
            {
                return false; // Sequence fails if any node fails
            }
        }
        return true; // All nodes succeeded
    }
}

public class SelectorNode : Node
{
    private List<Node> nodes = new List<Node>();

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }

    public override bool Execute()
    {
        foreach (var node in nodes)
        {
            if (node.Execute())
            {
                return true; // Selector succeeds if any node succeeds
            }
        }
        return false; // All nodes failed
    }
}