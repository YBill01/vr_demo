using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
   private TextMeshProUGUI _text;
   public static DebugText Instance;

   private string _lastMessage;

   public bool CanView;
   
   private void Start()
   {
      _text = GetComponent<TextMeshProUGUI>();
      Instance = this;
   }

   public void SetText(string text)
   {
      if(CanView)
         _text.text = text;
   }
   
   public void AddText(string text)
   {
      
   }
}
