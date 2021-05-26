/************************************************/
/*                                              */
/*     Copyright (c) 2018 - 2021 monitor1394    */
/*     https://github.com/monitor1394           */
/*                                              */
/************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace XCharts
{
    public class ChartLabel : ChartObject
    {
        private bool m_LabelAutoSize = true;
        private float m_LabelPaddingLeftRight = 3f;
        private float m_LabelPaddingTopBottom = 3f;
        private ChartText m_LabelText;
        private RectTransform m_LabelRect;
        private RectTransform m_IconRect;
        private RectTransform m_ObjectRect;
        private Vector3 m_IconOffest;

        private Image m_IconImage;

        public GameObject gameObject
        {
            get { return m_GameObject; }
            set
            {
                m_GameObject = value;
                m_ObjectRect = value.GetComponent<RectTransform>();
            }
        }
        public Image icon
        {
            get { return m_IconImage; }
            set { SetIcon(value); }
        }
        public ChartText label
        {
            get { return m_LabelText; }
            set
            {
                m_LabelText = value;
                if (value != null) m_LabelRect = m_LabelText.gameObject.GetComponent<RectTransform>();
            }
        }

        public ChartLabel()
        {
        }

        public void SetLabel(GameObject labelObj, bool autoSize, float paddingLeftRight, float paddingTopBottom)
        {
            m_GameObject = labelObj;
            m_LabelAutoSize = autoSize;
            m_LabelPaddingLeftRight = paddingLeftRight;
            m_LabelPaddingTopBottom = paddingTopBottom;
            m_LabelText = new ChartText(labelObj);
            m_LabelRect = m_LabelText.gameObject.GetComponent<RectTransform>();
            m_ObjectRect = labelObj.GetComponent<RectTransform>();
        }

        public void SetAutoSize(bool flag)
        {
            m_LabelAutoSize = flag;
        }

        public void SetIcon(Image image)
        {
            m_IconImage = image;
            if (image != null)
            {
                m_IconRect = m_IconImage.GetComponent<RectTransform>();
            }
        }

        public void SetIconSprite(Sprite sprite)
        {
            if (m_IconImage != null) m_IconImage.sprite = sprite;
        }

        public void SetIconSize(float width, float height)
        {
            if (m_IconRect != null) m_IconRect.sizeDelta = new Vector3(width, height);
        }

        public void UpdateIcon(IconStyle iconStyle, Sprite sprite = null)
        {
            if (m_IconImage == null) return;
            if (iconStyle.show)
            {
                ChartHelper.SetActive(m_IconImage.gameObject, true);
                m_IconImage.sprite = sprite == null ? iconStyle.sprite : sprite;
                m_IconImage.color = iconStyle.color;
                m_IconRect.sizeDelta = new Vector2(iconStyle.width, iconStyle.height);
                m_IconOffest = iconStyle.offset;
                AdjustIconPos();
                if (iconStyle.layer == IconStyle.Layer.UnderLabel)
                    m_IconRect.SetSiblingIndex(0);
                else
                    m_IconRect.SetSiblingIndex(m_GameObject.transform.childCount - 1);
            }
            else
            {
                ChartHelper.SetActive(m_IconImage.gameObject, false);
            }
        }

        public float GetLabelWidth()
        {
            if (m_LabelRect) return m_LabelRect.sizeDelta.x;
            else return 0;
        }

        public float GetLabelHeight()
        {
            if (m_LabelRect) return m_LabelRect.sizeDelta.y;
            return 0;
        }

        public void SetLabelColor(Color color)
        {
            if (m_LabelText != null) m_LabelText.SetColor(color);
        }

        public void SetLabelRotate(float rotate)
        {
            if (m_LabelText != null) m_LabelText.SetLocalEulerAngles(new Vector3(0, 0, rotate));
        }

        public void SetPosition(Vector3 position)
        {
            if (m_GameObject != null)
            {
                m_GameObject.transform.localPosition = position;
            }
        }

        public void SetLabelPosition(Vector3 position)
        {
            if (m_LabelRect) m_LabelRect.localPosition = position;
        }

        public void SetActive(bool flag)
        {
            if (m_GameObject) ChartHelper.SetActive(m_GameObject, flag);
        }
        public void SetLabelActive(bool flag)
        {
            if (m_LabelText != null) m_LabelText.SetActive(flag);
        }
        public void SetIconActive(bool flag)
        {
            if (m_IconImage) ChartHelper.SetActive(m_IconImage, flag);
        }

        public bool SetText(string text)
        {
            if (m_LabelRect == null) return false;
            if (m_LabelText != null && !m_LabelText.GetText().Equals(text))
            {
                m_LabelText.SetText(text);
                if (m_LabelAutoSize)
                {
                    var newSize = string.IsNullOrEmpty(text) ? Vector2.zero :
                        new Vector2(m_LabelText.GetPreferredWidth() + m_LabelPaddingLeftRight * 2,
                                        m_LabelText.GetPreferredHeight() + m_LabelPaddingTopBottom * 2);
                    var sizeChange = newSize.x != m_LabelRect.sizeDelta.x || newSize.y != m_LabelRect.sizeDelta.y;
                    if (sizeChange)
                    {
                        m_LabelRect.sizeDelta = newSize;
                        AdjustIconPos();
                    }
                    return sizeChange;
                }
                AdjustIconPos();
            }
            return false;
        }

        private void AdjustIconPos()
        {
            if (m_IconImage && m_IconImage.sprite != null && m_IconRect)
            {
                var iconX = 0f;
                switch (m_LabelText.text.alignment)
                {
                    case TextAnchor.LowerLeft:
                    case TextAnchor.UpperLeft:
                    case TextAnchor.MiddleLeft:
                        iconX = -m_ObjectRect.sizeDelta.x / 2 - m_IconRect.sizeDelta.x / 2;
                        break;
                    case TextAnchor.LowerRight:
                    case TextAnchor.UpperRight:
                    case TextAnchor.MiddleRight:
                        iconX = m_ObjectRect.sizeDelta.x / 2 - m_LabelText.GetPreferredWidth() - m_IconRect.sizeDelta.x / 2;
                        break;
                    case TextAnchor.LowerCenter:
                    case TextAnchor.UpperCenter:
                    case TextAnchor.MiddleCenter:
                        iconX = -m_LabelText.GetPreferredWidth() / 2 - m_IconRect.sizeDelta.x / 2;
                        break;
                }
                m_IconRect.anchoredPosition = m_IconOffest + new Vector3(iconX, 0);
            }
        }
    }
}
