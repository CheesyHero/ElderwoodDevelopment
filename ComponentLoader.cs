using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComponentLoader : MonoBehaviour
{
    public bool autoFind = true;
    public Image image;
    public TextMeshProUGUI textMesh;

    private void Awake()
    {
        if (autoFind == false) return;

        if(!image) image = GetComponent<Image>();
        if(!textMesh) textMesh = GetComponent<TextMeshProUGUI>();
    }
    public virtual void PassObject(object obj)
    {
        if(obj == null) return;
    }

    public virtual void SetText(string input)
    {
        if(textMesh)
            textMesh.text = input;
    }
    public virtual void SetImageSprite(Sprite input)
    {
        if(image)
            image.sprite = input;
    }
    public virtual void SetImageColor(Color input)
    {
        if(image)
            image.color = input;
    }
    public virtual void SetImageFill(float setting)
    {
        if(image)
            image.fillAmount = setting;
    }
    public virtual void AutoPopulate()
    {
        // Child components do cool things here
    }
    public void SetActive(bool setting)
    {
        gameObject.SetActive(setting);
    }
}
