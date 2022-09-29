using UnityEngine;
using UnityEngine.UI;

namespace TrombLoader.Data
{
    [RequireComponent(typeof(Text))]
    public class TextEventHandler : MonoBehaviour
    {
        public string StringToReplaceWithEventData = "{int}";

        private Text _text;
        private string _originalText = null;
        private int _cachedInt = 0;

        public void ConvertIntToText(int inputInt)
        {
            _cachedInt = inputInt;

            UpdateText();
        }

        private void UpdateText()
        {
            if (_text != null)
            {
                if (_originalText == null) _originalText = _text.text;
                _text.text = _originalText.Replace(StringToReplaceWithEventData, _cachedInt.ToString());
            } 
        }

        public void Start()
        {
            _text = GetComponent<Text>();

            UpdateText();
        }
    }
}
