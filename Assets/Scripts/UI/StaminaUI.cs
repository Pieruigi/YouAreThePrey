using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YATP.UI
{
    public class StaminaUI : MonoBehaviour
    {
        [SerializeField]
        Image bar;

        // Start is called before the first frame update
        void Start()
        {
            PlayerController.Instance.OnStaminaChanged += HandleOnStaminaChanged;
        }

     

        void HandleOnStaminaChanged(float currentValue, float maxValue)
        {
            bar.fillAmount = currentValue / maxValue;
        }
     
    }

}
