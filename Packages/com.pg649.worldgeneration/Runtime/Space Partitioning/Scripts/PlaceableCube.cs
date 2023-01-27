using Unity.AI.Navigation;
using UnityEngine;

public class PlaceableCube : Placeable {

    private int size;

    public PlaceableCube(int size = 1) {
        this.size = size;
    }

    public override GameObject ToGameObject() {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = new Vector3(size, size, size);
        go.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/instancing");
        // set cube to non walkable area
        var nmm = go.AddComponent<NavMeshModifier>();
        nmm.overrideArea = true;
        nmm.area = 1;
        return go;
    }

    public override int Width {
        get { return size; }
    }
    public override int Height {
        get { return size; }
    }

}
