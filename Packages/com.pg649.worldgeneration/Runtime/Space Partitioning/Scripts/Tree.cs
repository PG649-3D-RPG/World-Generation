using System.Collections.Generic;
using System;

public class Tree<T>{
    protected Tree<T> root, parent;
    protected List<Tree<T>> children;
    protected T node;
    protected int depth, height;


    public Tree(){
    this.node = default(T);
    this.children = new List<Tree<T>>();
    this.parent = null;
    this.root = this;
    this.depth = 0;
    this.height = 0; //only root has height 
    }
    public Tree(T node) : this(){
        this.node = node;
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
    public List<T> LeafNodes(){
        List<T> l = new List<T>();
        if(IsLeaf()) l.Add(node);
        else foreach(Tree<T> t in children) l.AddRange(t.LeafNodes());
        return l;
    }


    public Tree<V> MapNew<V>(Func<T,V> f){
        Tree<V> t = new Tree<V>(f(node));
        foreach(Tree<T> c in children){
            t.AddChild(c.MapNew<V>(f));
        }
        return t;
    }
    public void AddChild(Tree<T> t){
        t.Depth = depth + 1;
        t.Parent = this;
        t.Root = root;
        root.Height = Math.Max(root.Height, t.Depth);
        children.Add(t);
    }
    public void AddChild(T cn){
        AddChild(new Tree<T>(cn));
    }


    public T Node{
        get{return node;}
        set{node = value;}
    }
    public List<Tree<T>> Children{
        get{return children;}
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