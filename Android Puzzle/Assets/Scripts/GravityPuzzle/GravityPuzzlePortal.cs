using System;
using UnityEngine;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
        private bool portalBusy,portalMute,portalAdAudio,portalRegistered;
        private string portalRequest;
        private Action<bool> portalRewardComplete;
        public void SetPortalMute(string value){portalMute=value=="1";ApplyPortalAudio();}
        public void SetPortalAdAudio(string value){portalAdAudio=value=="1";ApplyPortalAudio();session?.SetPause(PauseReason.Inactive,!Active);if(portalAdAudio)CancelTouches();}
        private void ApplyPortalAudio(){if(CrazyPortal.Enabled)AudioListener.pause=portalMute||portalAdAudio;}
        private void TickPortal()
        {
            if(!CrazyPortal.Enabled)return;
            // JS can only SendMessage after createUnityInstance has resolved.
            if(!portalRegistered){CrazyPortal.Ready(gameObject.name);portalRegistered=true;}
            portalMute=CrazyPortal.Muted;
            ApplyPortalAudio();
            CrazyPortal.Playing(modal==""&&!session.Solved&&!portalBusy&&!portalAdAudio);
        }
        private void BeginPortalReward(Action<bool> complete)
        {
            if(portalBusy||!CrazyPortal.AdsEnabled){complete(false);return;}
            portalBusy=true;portalRewardComplete=complete;portalRequest=Guid.NewGuid().ToString("N");
            CancelTouches();session.SetPause(PauseReason.Menu,true);CrazyPortal.Playing(false);
            var card=OpenDialog("portal-ad",230);
            DialogText(card,"광고 준비 중…",125,45,23);
            DialogText(card,"광고가 끝나면 힌트를 받아요.",70,45,14,PuzzleUI.Muted);LayoutDialog();
            CrazyPortal.Reward(gameObject.name,portalRequest);
        }
        public void PortalAdResult(string message)
        {
            if(!portalBusy||string.IsNullOrEmpty(portalRequest))return;
            var fields=message.Split('|');
            if(fields.Length!=2||fields[0]!=portalRequest)return;
            var complete=portalRewardComplete;portalRewardComplete=null;portalRequest=null;
            portalBusy=false;portalAdAudio=false;ApplyPortalAudio();
            CloseDialog();session.SetPause(PauseReason.Menu,false);
            session.SetPause(PauseReason.Inactive,!Active);
            complete?.Invoke(fields[1]=="finished");
        }
    }
}
