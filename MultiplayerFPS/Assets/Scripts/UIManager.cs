using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour{

    public static UIManager instance;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI gunNameText;

    private void Awake() {
        instance = this;
    }

}
