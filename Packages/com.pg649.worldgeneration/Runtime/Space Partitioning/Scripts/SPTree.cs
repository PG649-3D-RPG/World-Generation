using System.Collections.Generic;
using System;
using System.Linq;

public class SPTreeNode {
    protected int[] point, size;
    protected int dim,degree;
    protected Random rand;


    public SPTreeNode(int[] size, Random rand = null){
        this.size = size;
        this.dim = size.Length;
        this.point = new int[dim];
        this.rand = rand ?? new Random();
    }
    public SPTreeNode(int[] size, int[] point, Random rand = null) : this(size, rand : rand){
        this.point = point;
    }

    public int[] Point{
        get{return point;}
        set{point = value;}
    }
    public int[] Size{
        get{return size;}
        set{size = value;}
    }
    public int Dim{
        get{return dim;}
        set{dim  = value;}
    }
    public int Degree{
        get{return dim;}
        set{dim = value;}
    }
    public Random Rand{
        get{return rand;}
        set{rand = value;}
    }
}




public class SPTreeT : Tree<SPTreeNode>{
    public enum PartitionMode {
        KDTreeRandom, QuadTreeUniform
    }


    public SPTreeT(IEnumerable<int> size, Random rand = null) : base(new SPTreeNode(size.ToArray(), rand : rand)){}
    public SPTreeT(IEnumerable<int> size, Action<SPTreeT> fSplit, Func<SPTreeT, bool> fStop = null, Random rand = null) : this(size, rand : rand){
        fStop = fStop ?? (x => false);
        Split(fSplit, fStop, recursive : true);
    }


    public void Split(Action<SPTreeT> fSplit, Func<SPTreeT, bool> fStop, bool recursive = false){
        if(!IsLeaf()) throw new Exception();
        else if(!fStop(this)){
            fSplit(this);
            if(recursive) foreach(SPTreeT child in children) child.Split(fSplit,fStop, recursive : recursive);
        }
    }


    //probably only works for 2d right now
    public static Action<SPTreeT> KDTreeRandom(IEnumerable<int> pMinSize = null){
        return x => {
            int [] minSize = pMinSize != null ? pMinSize.ToArray() : Enumerable.Repeat(0,x.node.Dim).ToArray();
            int[] splitDims = x.Node.Size.Select((z,i) => (z,i)).Where(z => 2*minSize[z.Item2] <= z.Item1).Select(z => z.Item2).ToArray();
            if(splitDims.Length > 0){
                int splitDim = splitDims[x.node.Rand.Next(0,splitDims.Length)];
                int splitPoint = x.Node.Rand.Next(minSize[splitDim],x.node.Size[splitDim] - minSize[splitDim]);
                int[] leftSize = (int[])x.node.Size.Clone();
                leftSize[splitDim] = splitPoint;
                SPTreeT left = new SPTreeT(leftSize, rand : x.node.Rand);
                left.node.Point = x.node.Point;
                int[] rightSize = (int[])x.node.Size.Clone();
                rightSize[splitDim] = x.node.Size[splitDim]-splitPoint;
                SPTreeT right = new SPTreeT(rightSize, rand : x.node.Rand);
                right.node.Point = (int[])x.node.Point.Clone();
                right.node.Point[splitDim] += splitPoint;
                x.AddChild(left);
                x.AddChild(right);
            }
        };
    }
    public static Action<SPTreeT> QuadTreeUniform(){
        return z => {
            if(z.node.Dim == 2 && z.node.Size.All(x => x > 0)){
                int x = z.node.Size[0]/2;
                int x_ = z.node.Size[0] - x;
                int y = z.node.Size[1]/2;
                int y_ = z.node.Size[1] -y;
                SPTreeT c1 = new SPTreeT(new int[] {x,y}, rand : z.node.Rand);
                c1.node.Point = z.node.Point;
                SPTreeT c2 = new SPTreeT(new int[] {x_,y}, rand : z.node.Rand);
                c2.node.Point = (int[])z.node.Point.Clone();
                c2.node.Point[0] += x_;
                SPTreeT c3 = new SPTreeT(new int[] {x,y_}, rand : z.node.Rand);
                c3.node.Point = (int[])z.node.Point.Clone();
                c3.node.Point[1] += y;
                SPTreeT c4 = new SPTreeT(new int[] {x_,y_}, rand : z.node.Rand);
                c4.node.Point = (int[])z.node.Point.Clone();
                c4.node.Point[0] += x_;
                c4.node.Point[1] += y_;
                z.AddChild(c1);
                z.AddChild(c2);
                z.AddChild(c3);
                z.AddChild(c4);
            }
        };
    }


    public static Func<SPTreeT, bool> StopMinSize(IEnumerable<int> pMinSize = null){
        return x => {
            if(x.node.Size.Length == pMinSize.Count()){
                bool b = false;
                int i = 0;
                foreach(int ms in pMinSize){
                    if(x.node.Size[i] < 2*ms) b = true;
                    i++;
                }
                return b;
            }
            else return true;
        };
    }
}