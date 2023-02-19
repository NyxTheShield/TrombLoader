using UnityEngine;

namespace TrombLoader.Data
{
    public class TromboneEventInvoker : MonoBehaviour
    {
        // i literally do not know why these two following variables need to be serialized
        // what
        [SerializeField]
        private GameController _controller;
        
        [SerializeField]
        private TromboneEventManager[] _eventManagers;

        [SerializeField]
        private BackgroundEvent[] _backgroundEvents;

        private bool previousNoteActiveValue = false;
        private bool waitingForNextTimeSigJump = true;
        private int barCount = 0;
        private int previousCombo = 0;
        private int previousBGDataIndex = 0;
        private bool currentInputState = false;

        public void InitializeInvoker(GameController controller, TromboneEventManager[] eventManagers)
        {
            _controller = controller;
            _eventManagers = eventManagers;
        }

        public void LateUpdate()
        {
            if (ReferenceEquals(_controller, null)) return;

            if (_controller.bgindex != previousBGDataIndex)
            {
                var eventID = (int)_controller.bgdata[previousBGDataIndex][1];

                previousBGDataIndex = _controller.bgindex;

                foreach (var manager in _eventManagers)
                {
                    foreach (var bgEvent in manager.Events)
                    {
                        if (bgEvent.BackgroundEventID == eventID) bgEvent.UnityEvent.Invoke();
                    }
                }
            }

            // beat / bar events
            if (_controller.timesigcount == 1)
            {
                if (waitingForNextTimeSigJump)
                {
                    waitingForNextTimeSigJump = false;

                    foreach (var manager in _eventManagers)
                    {
                        manager.OnBeat?.Invoke();

                        if (barCount % _controller.beatspermeasure == 0)
                        {
                            manager.OnBar?.Invoke();
                        }
                    }

                    barCount++;
                }
            }
            else waitingForNextTimeSigJump = true;

            // note start/end events
            if (previousNoteActiveValue != _controller.noteactive)
            {
                previousNoteActiveValue = _controller.noteactive;
                foreach (var manager in _eventManagers)
                {
                    if (previousNoteActiveValue) manager.NoteStart?.Invoke();
                    else manager.NoteEnd?.Invoke();
                }
            }

            if (_controller.highestcombocounter != previousCombo)
            {
                previousCombo = _controller.highestcombocounter;
                foreach (var manager in _eventManagers)
                {
                    manager.ComboUpdated?.Invoke(previousCombo);
                }
            }

            // input start/end events
            int keysActive = 0;
            foreach(var toot_key in _controller.toot_keys)
            {
                if (Input.GetKey(toot_key)) keysActive++;
            }

            if(keysActive == 1 || Input.GetMouseButton(0))
            {
                if(currentInputState == false)
                {
                    currentInputState = true;
                    foreach (var manager in _eventManagers) manager.PlayerTootInputStart?.Invoke();
                }
            }
            else
            {
                if(currentInputState == true)
                {
                    currentInputState = false;
                    foreach (var manager in _eventManagers) manager.PlayerTootInputEnd?.Invoke();
                }
            }
        }
    }
}
