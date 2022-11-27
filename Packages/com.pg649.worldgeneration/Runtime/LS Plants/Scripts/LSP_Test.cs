using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSP_Test : MonoBehaviour
{
    public LSP_Settings settings;

    void Start(){
        LSP lsp = new LSP(settings);
        GameObject g = lsp.ToGameObject();
        Debug.Log(g);
    }
}
