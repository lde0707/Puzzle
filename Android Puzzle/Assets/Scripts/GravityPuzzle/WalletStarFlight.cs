using System;
using UnityEngine;

namespace Puzzle.Gravity
{
    // Cosmetic copies leave the rating visible. The saved award is never delayed.
    public sealed class WalletStarFlight
    {
        private readonly RectTransform root, target;
        private readonly RectTransform[] copies, sources;
        private readonly PuzzleGlyph[,] trail;
        private readonly bool[] arrived;
        private readonly Action receive;
        private readonly int firstStar;
        public const float RevealHold=.12f,TravelDuration=.28f;
        public static float ArrivalAt(int index)=>CompletionStars.RevealAt(index)+RevealHold+TravelDuration;
        private readonly Vector2[] launchPoints;
        private readonly bool[] launched;
        private float elapsed;
        public WalletStarFlight(RectTransform parent,CompletionStars stars,RectTransform target,int rating,int count,Action receive)
        {
            this.target=target;this.receive=receive;firstStar=rating-count;
            root=PuzzleUI.Rect("Stars to wallet",parent);PuzzleUI.Fill(root);
            copies=new RectTransform[count];sources=new RectTransform[count];arrived=new bool[count];trail=new PuzzleGlyph[count,4];
            launchPoints=new Vector2[count];launched=new bool[count];
            for(int i=0;i<count;i++)
            {
                sources[i]=stars.Star(rating-count+i);
                for(int j=0;j<4;j++)
                {
                    trail[i,j]=PuzzleUI.Glyph(root,GlyphKind.Star,new Color(1,.80f,.37f));trail[i,j].gameObject.SetActive(false);
                }
                copies[i]=PuzzleUI.Rect("Collect star "+(i+1),root);
                var art=copies[i].gameObject.AddComponent<PuffyStar>();art.painted=true;art.raycastTarget=false;
                copies[i].gameObject.SetActive(false);
            }
        }
        public void Tick(float delta,bool instant)
        {
            elapsed+=delta;
            for(int i=0;i<copies.Length;i++)
            {
                if(arrived[i])continue;
                float t=instant?1:(elapsed-CompletionStars.RevealAt(firstStar+i)-RevealHold)/TravelDuration;
                if(t<0)continue;
                if(t>=1)
                {
                    arrived[i]=true;copies[i].gameObject.SetActive(false);
                    for(int j=0;j<4;j++)trail[i,j].gameObject.SetActive(false);
                    receive?.Invoke();continue;
                }
                if(!launched[i]){launched[i]=true;launchPoints[i]=root.InverseTransformPoint(sources[i].TransformPoint(sources[i].rect.center));}
                Vector2 start=launchPoints[i];
                Vector2 end=root.InverseTransformPoint(target.TransformPoint(target.rect.center));
                Vector2 bend=(start+end)/2+new Vector2(55+i*13,65);
                Vector2 Point(float u)=>(1-u)*(1-u)*start+2*(1-u)*u*bend+u*u*end;
                float eased=t*t*(3-2*t);
                float size=Mathf.Lerp(76,24,eased);
                void Place(RectTransform rect,Vector2 p,float s)
                {
                    rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=new Vector2(.5f,.5f);
                    rect.anchoredPosition=p-root.rect.center;rect.sizeDelta=new Vector2(s,s);
                }
                copies[i].gameObject.SetActive(true);Place(copies[i],Point(eased),size);
                float stretch=Mathf.Sin(t*Mathf.PI)*.12f;
                copies[i].localScale=new Vector3(1-stretch,1+stretch,1);
                copies[i].localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI)*18);
                for(int j=0;j<4;j++)
                {
                    float u=Mathf.Max(0,eased-(j+1)*.045f);
                    var spark=trail[i,j];spark.gameObject.SetActive(true);
                    Place(spark.rectTransform,Point(u),9-j*1.4f);
                    spark.color=new Color(1,.77f,.30f,(1-j*.2f)*Mathf.Min(1,t*10));
                }
            }
        }
    }
}
