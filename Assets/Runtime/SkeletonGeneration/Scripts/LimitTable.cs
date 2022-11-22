using System;
using System.Collections.Generic;
public class LimitTable {
    private Dictionary<(BoneCategory, BoneCategory), JointLimits> table;

    public LimitTable(Dictionary<(BoneCategory, BoneCategory), JointLimits> table) {
        this.table = table;
    }

    public bool HasLimits((BoneCategory, BoneCategory) t) {
        return table.ContainsKey(t);
    }
    public JointLimits this[(BoneCategory, BoneCategory) t]
    {
        get { return table[t]; }
        set { table[t] = value; }
    }
}