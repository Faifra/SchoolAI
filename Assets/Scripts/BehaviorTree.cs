using System.Collections.Generic;
using UnityEngine;

public enum NodeState
{
    Success,
    Failure,
    Running
}

public abstract class Node
{
    public abstract NodeState Execute();
}

public class ActionNode : Node
{
    private System.Func<NodeState> action;

    public ActionNode(System.Func<NodeState> action)
    {
        this.action = action;
    }

    public override NodeState Execute()
    {
        return action.Invoke();
    }
}

public class ConditionNode : Node
{
    private System.Func<bool> condition;

    public ConditionNode(System.Func<bool> condition)
    {
        this.condition = condition;
    }

    public override NodeState Execute()
    {
        return condition.Invoke() ? NodeState.Success : NodeState.Failure;
    }
}

public class SequenceNode : Node
{
    private List<Node> nodes = new List<Node>();
    private int currentIndex = 0;

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }

    public override NodeState Execute()
    {
        if (nodes.Count == 0) return NodeState.Success;

        var state = nodes[currentIndex].Execute();

        switch (state)
        {
            case NodeState.Success:
                currentIndex++;
                if (currentIndex >= nodes.Count)
                {
                    currentIndex = 0; // reset for next run
                    return NodeState.Success;
                }
                return NodeState.Running;

            case NodeState.Failure:
                currentIndex = 0; // reset for next run
                return NodeState.Failure;

            case NodeState.Running:
                return NodeState.Running;
        }

        return NodeState.Failure;
    }
}

public class SelectorNode : Node
{
    private List<Node> nodes = new List<Node>();
    private int currentIndex = 0;

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }

    public override NodeState Execute()
    {
        if (nodes.Count == 0) return NodeState.Failure;

        var state = nodes[currentIndex].Execute();

        switch (state)
        {
            case NodeState.Success:
                currentIndex = 0; // reset for next run
                return NodeState.Success;

            case NodeState.Failure:
                currentIndex++;
                if (currentIndex >= nodes.Count)
                {
                    currentIndex = 0; // reset for next run
                    return NodeState.Failure;
                }
                return NodeState.Running;

            case NodeState.Running:
                return NodeState.Running;
        }

        return NodeState.Failure;
    }
}
