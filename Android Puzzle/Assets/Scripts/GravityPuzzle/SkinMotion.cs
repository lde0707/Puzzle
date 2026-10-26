using UnityEngine;

namespace Puzzle.Gravity
{
    public sealed partial class SkinDecoration
    {
        public const float ArrivalDuration=.70f;
        private bool travelling;
        private float arrivalAge=10;
        private Vector2 travelDirection;
        public bool IsTravelling=>travelling&&!still;
        public float ArrivalPulse=>still||arrivalAge>=ArrivalDuration?0:Mathf.Sin(Mathf.Clamp01(arrivalAge/ArrivalDuration)*Mathf.PI);
        // BoardView supplies settled events and its pause-aware clock; portraits remain idle.
        public void Motion(bool moving,float arrivalSeconds,Vector2 direction)
        {
            float age=Mathf.Clamp(arrivalSeconds,0,ArrivalDuration);
            if(travelling==moving&&arrivalAge==age&&travelDirection==direction)return;
            travelling=moving;arrivalAge=age;travelDirection=direction;SetVerticesDirty();
        }
        private void Twinkle(float x,float y,float size,float strength)
        {m.Star(x,y,size,new Color(cream.r,cream.g,cream.b,strength),45+phase*35,4,.24f);}
        private void MotionLeaf(float x,float y,float strength)
        {
            var c=Color.Lerp(body,new Color(.33f,.71f,.43f),.55f);c.a=strength;
            m.Oval(x,y,.045f,.022f,c);m.Line(x-.028f,y,x+.028f,y,.008f,new Color(1,1,1,strength*.65f));
        }
        private void DrawMotionDetails()
        {
            if(still)return;
            float arrival=ArrivalPulse,move=travelling?1:0;
            if(arrival<=0&&move==0)return;
            float beat=.5f+.5f*Mathf.Sin(phase*10),strength=Mathf.Max(arrival,move*.65f);
            float drift=Mathf.Repeat(phase*1.8f,1);
            var light=new Color(highlight.r,highlight.g,highlight.b,strength);
            var white=new Color(1,1,1,strength*.8f);
            if(skin>=20){DrawExpansionMotion(arrival,move,beat,strength,drift,light,white);return;}
            switch(skin)
            {
                case 0: // Sugar bubbles skim the jelly, then rise in a little pop.
                    for(int i=0;i<3;i++)m.Oval(.24f+i*.24f,.22f+(arrival>0?arrival*.08f:drift*.025f),.012f+strength*.012f,.018f,white);
                    if(arrival>0)Twinkle(.75f,.75f,.055f*arrival,arrival);break;
                case 1:
                    MotionLeaf(.36f+sway,.80f,strength);MotionLeaf(.64f+sway,.87f,strength);
                    if(arrival>0){m.Line(.5f,.69f,.5f,.76f+arrival*.06f,.015f,light);Twinkle(.75f,.77f,.044f*arrival,arrival);}break;
                case 2: // Tiny paw steps; a heart when home.
                    for(int i=0;i<2;i++){float x=.29f+i*.36f;m.Oval(x,.18f+.018f*Mathf.Sin(phase*10+i*Mathf.PI),.035f,.021f,light);}
                    if(arrival>0)m.Heart(.73f,.65f,.075f*arrival,new Color(1,.62f,.71f,arrival));break;
                case 3:
                    m.Arc(.32f+sway,.85f,.055f,.065f,65,150,.014f,white);m.Arc(.68f-sway,.85f,.055f,.065f,30,115,.014f,white);
                    if(arrival>0){Twinkle(.23f,.69f,.045f*arrival,arrival);Twinkle(.77f,.69f,.045f*arrival,arrival);}break;
                case 4:
                    m.Heart(.5f,.29f,.055f*(.7f+strength*.35f+move*beat*.2f),new Color(dark.r,dark.g,dark.b,strength));
                    if(arrival>0){m.Oval(.25f,.17f,.044f,.024f,light);m.Oval(.75f,.17f,.044f,.024f,light);}break;
                case 5:
                    Twinkle(.5f,.84f,.027f+.018f*beat,strength);
                    if(arrival>0){Twinkle(.28f,.86f,.045f*arrival,arrival);Twinkle(.72f,.86f,.045f*arrival,arrival);}break;
                case 6:
                    Twinkle(.25f,.70f,.025f+.020f*beat,strength);Twinkle(.75f,.26f,.045f*(1-beat*.4f),strength);
                    if(arrival>0){Twinkle(.09f,.49f,.035f*arrival,arrival);Twinkle(.91f,.49f,.035f*arrival,arrival);}break;
                case 7:
                    if(move>0)for(int i=0;i<2;i++)m.Oval(.31f+i*.38f,.10f,.027f,.025f+beat*.020f,new Color(.69f,.88f,1,.7f));
                    if(arrival>0){m.Arc(.82f,.90f,.045f*arrival,.045f*arrival,0,360,.012f,white);Twinkle(.75f,.29f,.04f*arrival,arrival);}break;
                case 8:
                    if(move>0)for(int i=0;i<3;i++)m.Oval(.28f+i*.22f,.25f-.065f*Mathf.Repeat(drift+i*.25f,1),.012f,.028f,white);
                    if(arrival>0){m.Arc(.5f,.65f,.24f,.21f,25,155,.020f,new Color(gold.r,gold.g,gold.b,arrival));m.Arc(.5f,.65f,.19f,.16f,25,155,.016f,white);}break;
                case 9:
                    m.Oval(.28f,.72f+.015f*beat,.018f,.025f,white);
                    if(arrival>0){MotionLeaf(.31f,.32f,arrival);MotionLeaf(.69f,.32f,arrival);Twinkle(.73f,.72f,.04f*arrival,arrival);}break;
                case 10:
                    m.Line(.27f,.87f+sway,.39f,.87f+sway,.015f,white);m.Line(.61f,.87f-sway,.73f,.87f-sway,.015f,white);
                    if(arrival>0)for(int i=0;i<3;i++)Twinkle(.25f+i*.25f,.73f+arrival*.12f,.032f*arrival,arrival);break;
                case 11:
                    if(move>0)m.Line(.28f+drift*.31f,.66f,.34f+drift*.31f,.66f,.018f,new Color(.70f,1,.8f,.8f));
                    if(arrival>0)for(int i=0;i<5;i++)m.Round(.29f+i*.085f,.23f,.045f,.055f+arrival*.04f,.01f,new Color(.75f,1,.83f,arrival));break;
                case 12:
                    for(int i=0;i<3;i++)m.Arc(.27f+i*.23f+travelDirection.x*.018f*drift,.16f,.07f,.018f,190,350,.013f,white);
                    if(arrival>0){Twinkle(.62f,.86f,.04f*arrival,arrival);m.Line(.5f,.94f,.60f,.94f+arrival*.02f,.018f,light);}break;
                case 13:
                    if(move>0)for(int i=0;i<3;i++)Twinkle(.89f,.24f+i*.075f,.012f+.009f*Mathf.Repeat(drift+i*.3f,1),strength);
                    if(arrival>0){Twinkle(.90f,.49f,.078f*arrival,arrival);Twinkle(.75f,.63f,.035f*arrival,arrival);}break;
                case 14:
                    if(move>0)m.Oval(.77f,.32f-drift*.10f,.018f,.029f,light);
                    if(arrival>0)for(int i=0;i<5;i++){float a=i*Mathf.PI*.4f;m.Oval(.50f+.055f*arrival*Mathf.Cos(a),.76f+.055f*arrival*Mathf.Sin(a),.018f,.018f,white);}break;
                case 15:
                    if(move>0)m.Heart(.76f,.72f,.04f+.012f*beat,light);
                    if(arrival>0){m.Heart(.24f,.72f,.055f*arrival,white);m.Heart(.76f,.70f,.065f*arrival,white);}break;
                case 16:
                    m.Oval(.12f,.65f,.025f,.065f*(.6f+beat*.4f),white);m.Oval(.88f,.65f,.025f,.065f*(1-beat*.4f),white);
                    if(arrival>0)for(int i=0;i<3;i++)m.Oval(.28f+i*.22f,.75f+arrival*.035f,.018f,.018f,new Color(1,.85f,.34f,arrival));break;
                case 17:
                    Twinkle(.24f,.69f,.026f+beat*.015f,strength);Twinkle(.77f,.30f,.024f+(1-beat)*.015f,strength);
                    if(arrival>0)for(int i=0;i<6;i++){float a=i*Mathf.PI/3+arrival*.3f;m.Line(.5f,.73f,.5f+Mathf.Cos(a)*.085f*arrival,.73f+Mathf.Sin(a)*.085f*arrival,.013f,white);}break;
                case 18:
                    m.Arc(.5f,.46f,.44f,.13f,200+drift*35,235+drift*35,.018f,white);
                    if(arrival>0){Twinkle(.25f,.75f,.045f*arrival,arrival);Twinkle(.76f,.71f,.035f*arrival,arrival);}break;
                default:
                    Twinkle(.27f,.68f,.023f+beat*.012f,strength);
                    if(arrival>0){Twinkle(.72f,.28f,.055f*arrival,arrival);m.Arc(.71f,.66f,.12f,.13f,95,220,.016f,white);}break;
            }
        }
    }
}
