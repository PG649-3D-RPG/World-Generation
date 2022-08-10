using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class SPTree : Tree<int[]>{

    public static readonly Action<SPTree>[] splitMethod = new Action<SPTree>[] {KDTreeRandom()};

    protected Random rand;

    protected int d;
    
    public SPTree(IEnumerable<int> size, Random rand = null) : base(size.ToArray()){
        this.d = label.Length;
        this.rand = rand ?? new Random();
    }

    public void Split(Action<SPTree> fSplit, Func<SPTree, bool> fStop, bool recursive = false){
        if(!IsLeaf()) throw new Exception();
        else if(!fStop(this)){
            fSplit(this);
            if(recursive) foreach(SPTree child in children) child.Split(fSplit,fStop, recursive : recursive);
        }
    }

    public void Extrude3D(Action<SPTree> f){
        if(d < 3){
            f(this);
            foreach(SPTree child in children) child.Extrude3D(f);
        }
    }

    public static SPTree GenerateSPTree(IEnumerable<int> size, Action<SPTree> fSplit, Func<SPTree, bool> fStop = null){
        SPTree t = new SPTree(size);
        fStop = fStop ?? (x => false);
        t.Split(fSplit, fStop, recursive : true);
        return t;   
    }

    public static Action<SPTree> KDTreeRandom(IEnumerable<int> pMinSize = null){
        return x => {
            int [] minSize = pMinSize != null ? pMinSize.ToArray() : Enumerable.Repeat(0,x.d).ToArray();
            int[] splitDims = x.label.Select((z,i) => (z,i)).Where(z => 2*minSize[z.Item2] <= z.Item1).Select(z => z.Item2).ToArray();
            if(splitDims.Length > 0){
                int splitDim = splitDims[x.rand.Next(0,splitDims.Length)];
                int splitPoint = x.rand.Next(minSize[splitDim],x.label[splitDim] - minSize[splitDim]);
                int[] leftSize = (int[])x.label.Clone();
                leftSize[splitDim] = splitPoint;
                SPTree left = new SPTree(leftSize, rand : x.rand);
                int[] rightSize = (int[])x.label.Clone();
                rightSize[splitDim] = x.label[splitDim]-splitPoint;
                SPTree right = new SPTree(rightSize, rand : x.rand);
                x.AddChild(left);
                x.AddChild(right);
            }
        };
    }

    public static Action<SPTree> QuadTreeUniform(){
        return z => {
            if(z.d == 2 && z.label.All(x => x > 0)){
                int x = z.label[0]/2;
                int x_ = z.label[0] - x;
                int y = z.label[1]/2;
                int y_ = z.label[1] -y;
                z.AddChild(new SPTree(new int[] {x,y}, rand : z.rand));
                z.AddChild(new SPTree(new int[] {x_,y}, rand : z.rand));
                z.AddChild(new SPTree(new int[] {x,y_}, rand : z.rand));
                z.AddChild(new SPTree(new int[] {x_,y_}, rand : z.rand));
            }
        };
    }

    public static Func<SPTree, bool> StopMinSize(IEnumerable<int> pMinSize = null){
        return x => {
            if(x.label.Length == pMinSize.Count()){
                bool b = false;
                int i = 0;
                foreach(int ms in pMinSize){
                    if(x.label[i] < 2*ms) b = true;
                    i++;
                }
                return b;
            }
            else return true;
        };  
    }

    //max excluding
    public static Action<SPTree> Extrude3DMinMax(int min, int max){
        return z => {
            Array.Resize(ref z.label, 3);
            z.label[2] = z.rand.Next(min, max);
        };
    }

    public static Action<SPTree> Extrude3DDepth(int[] values){
        return z => {
            if(values.Length == z.Root.Height + 1){
                Array.Resize(ref z.label, 3);
                z.label[2] = values[z.Depth];
            }
        };
    }

}

