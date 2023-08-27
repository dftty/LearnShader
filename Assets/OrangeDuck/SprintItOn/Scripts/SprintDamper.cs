using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SprintItOn
{
    public class SprintDamper : MonoBehaviour, IPointerClickHandler, IDragHandler
    {
        public const int maxHistroy = 128;

        public const float fillRectYOffset = 36f;

        public RectTransform rectTransform;

        public Image redDot;

        public Image dotPrefab;

        public Image linePrefab;


        public FillRect fillRectPrefab;

        public RectTransform fillRectParent;

        FillRect frequencyFillRect;
        FillRect dampingRatioFillRect;
        FillRect dampingFillRect;
        FillRect halfLifeFillRect;
        FillRect dtFillRect;

        Image[] dots = new Image[maxHistroy];

        Image[] lines = new Image[maxHistroy - 1];

        float[] prevY = new float[maxHistroy];

        float[] prevX = new float[maxHistroy];

        float[] prevV = new float[maxHistroy];

        float y;
        float g;
        float x;
        float v;

        float frequency;
        float dampingRatio;
        float damping;
        float halfLife;
        float dt;

        // Start is called before the first frame update
        void Start()
        {
            CreateFillRect();

            g = y = rectTransform.sizeDelta.y / 2;
            x = 0;

            for (int i = 0; i < maxHistroy; i++)
            {
                prevY[i] = y;
                prevX[i] = x;

                Image dot = Instantiate(dotPrefab, rectTransform);
                dot.rectTransform.anchoredPosition = new Vector2(redDot.rectTransform.anchoredPosition.x , y);
                dots[i] = dot;

                if (i > 0)
                {
                    Image line = Instantiate(linePrefab, rectTransform);
                    line.rectTransform.anchoredPosition = new Vector2(redDot.rectTransform.anchoredPosition.x, y);
                    lines[i - 1] = line;
                }
            }
        }

        void FixedUpdate()
        {
            for (int i = maxHistroy - 1; i > 0; i--)
            {
                prevX[i] = prevX[i - 1];
                prevY[i] = prevY[i - 1];
                prevV[i] = prevV[i - 1];
            }

            float goal = redDot.rectTransform.anchoredPosition.y;

            x += dt;
            // Common.SprintDamperExact(ref y, ref v, goal, 0, frequency, halfLife, dt);

            // dampingRatio where a value of 1 means a critically damped spring, a value < 1 means 
            // an under-damped spring, and a value >1 means a over-damped spring
            Common.SprintDamperExactRatio(ref y, ref v, goal, 0, dampingRatio, halfLife, dt);

            // Common.CriticalSprintDamperExact(ref y, ref v, goal, 0, halfLife, dt);
            // Common.SimpleSprintDamperExact(ref y, ref v, goal, halfLife, dt);

            prevX[0] = x;
            prevY[0] = y;
            prevV[0] = v;

            for (int i = 0; i < maxHistroy; i++)
            {
                float posX = redDot.rectTransform.anchoredPosition.x - (x - prevX[i]) * 450;
                dots[i].rectTransform.anchoredPosition = new Vector2(posX, prevY[i]);
            }

            for (int i = 0; i < maxHistroy - 1; i++)
            {
                lines[i].rectTransform.anchoredPosition = dots[i].rectTransform.anchoredPosition;
                float width = Vector3.Distance(dots[i].rectTransform.anchoredPosition, dots[i + 1].rectTransform.anchoredPosition);
                lines[i].rectTransform.sizeDelta = new Vector2(width, 2f);
                lines[i].rectTransform.localRotation = Quaternion.Euler(0, 0, AngleFromVector(dots[i + 1].rectTransform.anchoredPosition - dots[i].rectTransform.anchoredPosition));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SetRedDot(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            SetRedDot(eventData);
        }

        private void SetRedDot(PointerEventData eventData)
        {
            float posX = redDot.rectTransform.anchoredPosition.x;
            float posY = eventData.position.y - rectTransform.rect.height / 2;
            redDot.rectTransform.anchoredPosition = new Vector2(posX, posY);
        }

        void CreateFillRect()
        {
            int index = 1;

            frequencyFillRect = Instantiate(fillRectPrefab, fillRectParent);
            frequencyFillRect.Init(0, 3, "Frequency", 0.5f, (float val) => { frequency = val; });
            frequency = frequencyFillRect.currentVal;

            frequencyFillRect.transform.SetParent(fillRectParent);
            frequencyFillRect.GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, -index * fillRectYOffset);

            index++;

            dampingRatioFillRect = Instantiate(fillRectPrefab, fillRectParent);
            dampingRatioFillRect.Init(0, 2, "Damping Ratio", 1f, (float val) => { dampingRatio = val; });
            dampingRatio = dampingRatioFillRect.currentVal;

            dampingRatioFillRect.transform.SetParent(fillRectParent);
            dampingRatioFillRect.GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, -index * fillRectYOffset);

            index++;

            dampingFillRect = Instantiate(fillRectPrefab, fillRectParent);
            dampingFillRect.Init(0, 30f, "Damping", 15f, (float val) => { damping = val; });
            damping = dampingFillRect.currentVal;

            dampingFillRect.transform.SetParent(fillRectParent);
            dampingFillRect.GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, -index * fillRectYOffset);

            index++;

            halfLifeFillRect = Instantiate(fillRectPrefab, fillRectParent);
            halfLifeFillRect.Init(0, 1, "Half Life", 0.5f, (float val) => { halfLife = val; });
            halfLife = halfLifeFillRect.currentVal;

            halfLifeFillRect.transform.SetParent(fillRectParent);
            halfLifeFillRect.GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, -index * fillRectYOffset);

            index++;

            dtFillRect = Instantiate(fillRectPrefab, fillRectParent);
            dtFillRect.Init(0.02f, 0.1f, "dt", 0.02f, (float val) => { 
                dt = val; 
                Time.fixedDeltaTime = dt;
            });
            dt = dtFillRect.currentVal;

            dtFillRect.transform.SetParent(fillRectParent);
            dtFillRect.GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, -index * fillRectYOffset);
        }

        float AngleFromVector(Vector2 vec)
        {
            vec.Normalize();
            float angle = Mathf.Acos(vec.x) * Mathf.Rad2Deg;
            if (vec.y < 0)
            {
                angle = 360 - angle;
            }

            return angle;
        }
    }
}