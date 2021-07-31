using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frame
{
    public class FSM<TEvent, TState>
    {
        class InState
        {
            public TState State;
            public DAction Entry;
            public DAction Leave;
            public DAction Update;

        }
        public delegate void DAction();
        Dictionary<TEvent, Dictionary<TState, (TState ToState, DAction Action)?>> evenDict = new Dictionary<TEvent, Dictionary<TState, (TState ToState, DAction Action)?>>();
        public TState CurrentState { get { return _CurrentState.State; } }

        InState _CurrentState;

        Dictionary<TState, InState> stateDict = new Dictionary<TState, InState>();


        public bool AddState(TState state, DAction Entry, DAction Leave, DAction Update)
        {
            if (stateDict.GetValueOrDefault(state) != null)
                return false;
            stateDict.Add(state, new InState { State = state, Entry = Entry, Leave = Leave, Update = Update});
            return true;
        }
        public bool AddEvent(TEvent Event, TState FromState, TState ToState, DAction action )
        {
            if (stateDict.GetValueOrDefault(FromState) == null)
                return false;
            if (stateDict.GetValueOrDefault(ToState) == null)
                return false;
            var StateEvent = evenDict.GetValueOrDefault(Event);
            if (StateEvent == null)
            {
                StateEvent = new Dictionary<TState, (TState ToState, DAction Action)?>();
                evenDict.Add(Event, StateEvent);
                StateEvent.Add(FromState, (ToState, action));
                return true;
            }
            if (StateEvent.GetValueOrDefault(FromState) != null)
                return false;
            StateEvent.Add(FromState, (ToState, action));
            return true;
        }
        public bool PostEvent(TEvent Event)
        {
            var map = evenDict.GetValueOrDefault(Event);
            if (map == null)
                return false;
            var oldState = _CurrentState;

            var newStateNode = map.GetValueOrDefault(_CurrentState.State);
            if (newStateNode == null)
                return false;
            var newState = stateDict.GetValueOrDefault(_CurrentState.State);
            if (newState == null)
                return false;

            var newCallbacks = stateDict.GetValueOrDefault(newStateNode.Value.ToState);
            if (newCallbacks == null)
                return false;
             oldState.Leave?.Invoke();
            _CurrentState = newState;
             newCallbacks.Entry?.Invoke();
             newStateNode.Value.Action?.Invoke();
            return true;
        }

        public void Update()
        {
            _CurrentState.Update?.Invoke();
        }

        public bool Start(TState state)
        {
            var value = stateDict.GetValueOrDefault(state);
            if (value == null)
               return false;
            _CurrentState = value;
            value.Entry?.Invoke();
            return true;
        }

    }

}
