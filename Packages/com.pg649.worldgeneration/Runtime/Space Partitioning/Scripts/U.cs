using UnityEngine;


public interface IGameObjectable{
    public GameObject ToGameObject();
}

public abstract class Placeable : IGameObjectable{

    public GameObject CenteredGameObject(){
        GameObject go = ToGameObject();

        return go;
    }
    public abstract GameObject ToGameObject();

    public int Width{get;}
    public int Height{get;}
}

public static class TreeExtensionUnity{
    public static GameObject ToGameObject<T>(this Tree<T> t) where T : IGameObjectable{
        GameObject go = t.Node.ToGameObject();
        foreach(Tree<T> child in t.Children){
            GameObject g = child.Node.ToGameObject();
            g.transform.parent = go.transform;
        }
    return go;
    }
}