using System;
using HutongGames.PlayMaker;

namespace CarefreeGrimm
{
    public static class FsmExtensions
    {

        class RunAction : FsmStateAction
        {
            Action<Fsm> method;

            public RunAction(Action<Fsm> method)
            {
                this.method = method;
            }

            public override void OnEnter()
            {
                method.Invoke(Fsm);
                base.OnEnter();
            }
        }
 
        public static FsmTransition AddTransition(this FsmState state, string eventName, string toState)
        {
            FsmTransition transition = new FsmTransition();
            transition.FsmEvent = FsmEvent.GetFsmEvent(eventName);
            transition.ToState = toState;
            transition.ToFsmState = state.Fsm.GetState(toState);
            FsmTransition[] transitions = new FsmTransition[state.Transitions.Length + 1];
            Array.Copy(state.Transitions, transitions, state.Transitions.Length);
            transitions[transitions.Length - 1] = transition;
            state.Transitions = transitions;
            return transition;
        }

        public static FsmStateAction AddAction(this FsmState state, Action<Fsm> method)
        {
            FsmStateAction[] actions = new FsmStateAction[state.Actions.Length + 1];
            Array.Copy(state.Actions, actions, state.Actions.Length);
            FsmStateAction action = new RunAction(method);
            actions[actions.Length - 1] = action;
            state.Actions = actions;
            action.Init(state);
            return action;
        }

        public static FsmStateAction ReplaceAction(this FsmState state, Action<Fsm> method, int index)
        {
            FsmStateAction action = new RunAction(method);
            
            state.Actions[index] = action;
            action.Init(state);
            return action;
        }
    }
}