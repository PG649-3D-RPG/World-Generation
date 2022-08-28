using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class SPTree : Tree<int[]>{
    protected int[] point;
    protected Random rand;
    protected int d;
    



    public SPTree(IEnumerable<int> size, Random rand = null) : base(size.ToArray()){
        this.d = label.Length;
        this.point = new int[d];
        this.rand = rand ?? new Random();
    }

    public SPTree(IEnumerable<int> size, Action<SPTree> fSplit, Func<SPTree, bool> fStop = null) : this(size){
        fStop = fStop ?? (x => false);
        Split(fSplit, fStop, recursive : true);
    }

    public void Split(Action<SPTree> fSplit, Func<SPTree, bool> fStop, bool recursive = false){
        if(!IsLeaf()) throw new Exception();
        else if(!fStop(this)){
            fSplit(this);
            if(recursive) foreach(SPTree child in children) child.Split(fSplit,fStop, recursive : recursive);
        }
    }




    //probably only works for 2d right now
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
                left.Point = x.Point;
                int[] rightSize = (int[])x.label.Clone();
                rightSize[splitDim] = x.label[splitDim]-splitPoint;
                SPTree right = new SPTree(rightSize, rand : x.rand);
                right.Point = (int[])x.Point.Clone();
                right.Point[splitDim] += splitPoint;
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
                SPTree c1 = new SPTree(new int[] {x,y}, rand : z.rand);
                c1.Point = z.Point;
                SPTree c2 = new SPTree(new int[] {x_,y}, rand : z.rand);
                c2.Point = (int[])z.Point.Clone();
                c2.Point[0] += x_;
                SPTree c3 = new SPTree(new int[] {x,y_}, rand : z.rand);
                c3.Point = (int[])z.Point.Clone();
                c3.Point[1] += y;
                SPTree c4 = new SPTree(new int[] {x_,y_}, rand : z.rand);
                c4.Point = (int[])z.Point.Clone();
                c4.Point[0] += x_;
                c4.Point[1] += y_;
                z.AddChild(c1);
                z.AddChild(c2);
                z.AddChild(c3);
                z.AddChild(c4);
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




    public void Extrude3D(Action<SPTree> f){
        if(d < 3){
            f(this);
            foreach(SPTree child in children) child.Extrude3D(f);
        }
    }
    //max excluding
    public void Extrude3DMinMax(int min, int max){
        if(d == 2){
            Array.Resize(ref label, 3);
            label[2] = rand.Next(min, max);
            d = 3;
        }
    }
    //???
    public void Extrude3DDepth(int[] values){
        if(values.Length == Root.Height + 1 && d == 2){
            Array.Resize(ref label, 3);
            label[2] = values[depth];
        }
    }





    public int[] Point{
        get{return point;}
        set{point = value; }
    }
    public Random Rand{
        get{return rand;}
        set{rand = value; }
    }

    public int D{
        get{return d;}
    }
}

