using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShellManager : MonoBehaviour {
    #region Variables for interaction with sell
    const float kickForce = 500f;

    bool grabShell;

    private Rigidbody2D rbShell;
    private GameObject currentShell;

    public Transform hand;
    #endregion

    void Start() {
        rbShell = gameObject.GetComponent<Rigidbody2D>();
    }

    public void KickShell() {
        
    }

    public void GrabShell(GameObject shellObject) {
        
    }
}