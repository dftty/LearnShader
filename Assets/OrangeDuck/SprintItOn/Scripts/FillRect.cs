using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace SprintItOn
{
    public class FillRect : MonoBehaviour, IPointerClickHandler, IDragHandler
    {

        public float minValue;
        public float maxValue;

        public string leftTextString;

        public Image image;

        public Text leftText;

        public Text rightText;

        public float currentVal = 0.5f;

        public Action<float> OnValueChanged;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void Init(float minValue, float maxValue, string leftTextString, float currentVal, Action<float> OnValueChanged)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.leftTextString = leftTextString;
            this.currentVal = currentVal;
            this.OnValueChanged = OnValueChanged;

            leftText.text = leftTextString;
            rightText.text = currentVal.ToString();
            image.fillAmount = currentVal / (maxValue - minValue); 
        }

        public void OnDrag(PointerEventData eventData)
        {
            SetImage(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SetImage(eventData);
        }

        private void SetImage(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == image.gameObject)
            {
                currentVal = (eventData.position.x - image.rectTransform.position.x + image.rectTransform.sizeDelta.x / 2) / image.rectTransform.rect.width;
                currentVal = Mathf.Clamp(currentVal, 0, 1);
                currentVal = minValue + (maxValue - minValue) * currentVal;
                rightText.text = currentVal.ToString();

                image.fillAmount = currentVal / (maxValue - minValue);
                OnValueChanged?.Invoke(currentVal);
            }
        }
    }

}