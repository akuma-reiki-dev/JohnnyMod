using RoR2;
using EntityStates;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using JohnnyMod.Survivors.Johnny.Components;

namespace JohnnyMod.Survivors.Johnny.SkillStates
{
    public class RomanIdle : Idle
    {
        private JohnnyTensionController tensionCTRL;
        public override void OnEnter()
        {
            base.OnEnter();
            tensionCTRL = GetComponent<JohnnyTensionController>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool canRC = tensionCTRL.tension >= 50;

            if (canRC && isAuthority &&
                (inputBank.skill1.down) &&
                (inputBank.skill2.down) &&
                (inputBank.skill4.down))
            {
                inputBank.skill1.PushState(false);
                inputBank.skill2.PushState(false);
                inputBank.skill4.PushState(false);
                tensionCTRL.AddTension(-50);
                outer.SetState(new RomanCancel());
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
