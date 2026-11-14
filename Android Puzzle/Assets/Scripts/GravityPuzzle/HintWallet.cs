using System;
namespace Puzzle.Gravity
{
    [Serializable] public sealed class HintWallet
    {
        public const int Capacity=3;
        public const long RechargeSeconds=3600;
        public int version=1, balance=Capacity;
        public long rechargeAt;
        public static long Now=>DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public static HintWallet Restore(HintWallet saved,long now)
        {
            if(saved==null||saved.version!=1||saved.rechargeAt<=0)return new HintWallet{rechargeAt=now};
            saved.balance=Math.Max(0,Math.Min(Capacity,saved.balance));saved.Recharge(now);return saved;
        }
        public bool Recharge(long now)
        {
            if(balance>=Capacity)return false;
            long steps=Math.Max(0,now-rechargeAt)/RechargeSeconds;
            if(steps==0)return false;
            balance=Math.Min(Capacity,balance+(int)Math.Min(Capacity,steps));
            rechargeAt=balance==Capacity?now:rechargeAt+steps*RechargeSeconds;
            return true;
        }
        public long SecondsUntilNext(long now)=>balance>=Capacity?0:Math.Max(0,RechargeSeconds-Math.Max(0,now-rechargeAt));
        public bool Spend(long now,Func<bool> persist)
        {
            Recharge(now);if(balance<=0)return false;
            int old=balance;long oldTime=rechargeAt;
            if(balance==Capacity)rechargeAt=now;
            balance--;bool ok=false;
            try{ok=persist();return ok;}finally{if(!ok){balance=old;rechargeAt=oldTime;}}
        }
    }
}
