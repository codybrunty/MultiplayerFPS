using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour{

    public static UIManager instance;
    public TextMeshProUGUI ammoText;

    private void Awake() {
        instance = this;
    }

}
