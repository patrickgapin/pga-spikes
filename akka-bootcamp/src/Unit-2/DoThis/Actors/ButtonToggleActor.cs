
using System;
using System.IO.IsolatedStorage;
using System.Windows.Forms;
using Akka.Actor;
using ChartApp.Messages;

namespace ChartApp.Actors
{
    public class ButtonToggleActor : UntypedActor
    {
        private readonly CounterType myCounterType;
        private bool isToggleOn;
        private readonly Button myButton;
        private readonly IActorRef coordinatorActor;

        public ButtonToggleActor(IActorRef coordinatorActor, Button myButton, CounterType myCounterType, bool isToggleOn = false)
        {
            this.coordinatorActor = coordinatorActor;
            this.myButton = myButton;
            this.myCounterType = myCounterType;
            this.isToggleOn = isToggleOn;
        }

        protected override void OnReceive(object message)
        {
            if (message is ToggleMessage)
            {
                if (isToggleOn) { coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.UnwatchMessage(myCounterType)); }
                else { coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.WatchMessage(myCounterType)); }

                FlipToggle();
                return;
            }

            Unhandled(message);
        }

        private void FlipToggle()
        {
            isToggleOn = !isToggleOn;

            myButton.Text = $"{myCounterType.ToString().ToUpperInvariant()} {(isToggleOn ? "(ON)" : "(OFF)")}";
        }

        #region Messages


        public class ToggleMessage
        {
        }



        #endregion
    }
}
