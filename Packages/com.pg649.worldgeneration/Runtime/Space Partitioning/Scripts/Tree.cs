using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Tree<T>
{
    protected Tree<T> root, parent;
    protected List<Tree<T>> children;
    protected T label;
    protected int depth, height;

    public Tree(T label){
        this.label = label;
        this.children = new List<Tree<T>>();
        this.parent = null;
        this.root = this;
        this.depth = 0;
        this.height = 0; //only root has height 
    }

    public Tree(T label, List<Tree<T>> children, Tree<T> parent = null){
        this.label = label;
        this.children = children;
    }

    public bool IsLeaf(){
        return (children.Count == 0);
    }

    public List<Tree<T>> Leaves(){
        List<Tree<T>> l = new List<Tree<T>>();
        if(IsLeaf()) l.Add(this);
        else foreach(Tree<T> t in children) l.AddRange(t.Leaves());
        return l;
    }

    public Tree<V> Map<V>(Func<T,V> f){
        return new Tree<V>(f(label), children.Select( x => x.Map<V>(f)).ToList());
    }

    public void AddChild(Tree<T> t){
        t.Depth = depth + 1;
        t.Parent = this;
        t.Root = root;
        root.Height = Math.Max(root.Height, t.Depth);
        children.Add(t);
    }

    public Tree<T> Parent{
        get{return parent;}
        set{parent = value;}
    }
    public Tree<T> Root{
        get{return root;}
        set{root = value;}
    }
    public int Depth{
        get{ return depth;}
        set{ depth = value;}
    }

    public int Height{
        get{ return height;}
        set{ height = value;}
    }

}
