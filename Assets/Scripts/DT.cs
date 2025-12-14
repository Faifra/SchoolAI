using System;

public abstract class DTNodeBase
{
    public abstract void Execute();
}

public class DTDecisionNode : DTNodeBase
{
    private Func<bool> condition;
    private DTNodeBase trueNode;
    private DTNodeBase falseNode;

    public DTDecisionNode(Func<bool> condition, DTNodeBase trueNode, DTNodeBase falseNode)
    {
        this.condition = condition;
        this.trueNode = trueNode;
        this.falseNode = falseNode;
    }

    public override void Execute()
    {
        if (condition())
            trueNode.Execute();
        else
            falseNode.Execute();
    }
}

public class DTActionNode : DTNodeBase
{
    private Action action;

    public DTActionNode(Action action)
    {
        this.action = action;
    }

    public override void Execute()
    {
        action();
    }
}