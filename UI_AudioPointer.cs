using UnityEngine;
using UnityEngine.EventSystems;
 
public class UI_AudioPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{ 
    public AudioClip onEnterClip;  
    public AudioClip onClickClip;  
    public AudioClip onExitClip; 
     
    public void OnPointerEnter(PointerEventData eventData)
    {  
        AudioInstancer.PlayAudioInstance(onEnterClip);
    }
    public void OnPointerClick(PointerEventData eventData)
    {  
        AudioInstancer.PlayAudioInstance(onClickClip);
    }
    public void OnPointerExit(PointerEventData eventData)
    {  
        AudioInstancer.PlayAudioInstance(onExitClip);
    } 
}
