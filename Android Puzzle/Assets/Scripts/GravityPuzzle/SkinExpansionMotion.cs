using UnityEngine;

namespace Puzzle.Gravity
{
    public sealed partial class SkinDecoration
    {
        private void DrawExpansionMotion(float arrival,float move,float beat,float strength,float drift,Color light,Color white)
        {
            switch(skin)
            {
                case 20: // Fox: paw steps and a woodland leaf.
                    m.Oval(.27f,.13f+beat*.018f,.034f,.018f,light);m.Oval(.66f,.15f-beat*.018f,.034f,.018f,light);
                    if(arrival>0){MotionLeaf(.74f,.71f,arrival);Twinkle(.27f,.79f,.035f*arrival,arrival);}break;
                case 21:
                    m.Oval(.76f+sway,.35f,.035f+.01f*beat,.016f,light);
                    if(arrival>0){MotionLeaf(.73f,.43f,arrival);m.Heart(.27f,.70f,.05f*arrival,white);}break;
                case 22:
                    m.Arc(.5f,.13f,.29f,.04f,185,355,.013f,white);
                    if(arrival>0)for(int i=0;i<3;i++)m.Oval(.24f+i*.26f,.84f+.04f*arrival,.015f,.025f,white);break;
                case 23:
                    m.Line(.17f,.46f+sway,.13f,.52f+sway,.018f,white);m.Line(.83f,.46f-sway,.87f,.52f-sway,.018f,white);
                    if(arrival>0){Twinkle(.23f,.77f,.05f*arrival,arrival);Twinkle(.77f,.77f,.05f*arrival,arrival);}break;
                case 24:
                    for(int i=0;i<3;i++)m.Oval(.89f,.34f+i*.16f,.012f+.006f*beat,.018f,light);
                    if(arrival>0){m.Oval(.28f,.15f,.045f*arrival,.025f,white);m.Oval(.66f,.15f,.045f*arrival,.025f,white);}break;
                case 25:
                    m.Line(.09f,.50f+sway,.14f,.58f+sway,.018f,white);m.Line(.91f,.50f+sway,.86f,.58f+sway,.018f,white);
                    if(arrival>0)for(int i=0;i<3;i++)Twinkle(.36f+i*.14f,.76f+arrival*.06f,.033f*arrival,arrival);break;
                case 26:
                    m.Oval(.20f,.17f+beat*.02f,.045f,.024f,light);m.Oval(.80f,.19f-beat*.02f,.045f,.024f,light);
                    if(arrival>0)m.Arc(.5f,.44f,.13f+arrival*.045f,.08f+arrival*.025f,0,360,.014f,white);break;
                case 27:
                    for(int i=0;i<3;i++)m.Line(.23f+i*.24f,.80f,.25f+i*.24f,.85f+beat*.02f,.012f,light);
                    if(arrival>0){MotionLeaf(.28f,.29f,arrival);MotionLeaf(.72f,.29f,arrival);}break;
                case 28:
                    for(int i=0;i<2;i++)m.Arc(.10f+i*.80f,.73f+drift*.08f,.022f,.027f,0,360,.010f,white);
                    if(arrival>0){m.Heart(.30f,.72f,.045f*arrival,white);m.Heart(.70f,.72f,.045f*arrival,white);}break;
                case 29:
                    for(int i=0;i<4;i++)m.Oval(.23f+i*.18f,.13f+.012f*Mathf.Sin(phase*9+i),.022f,.013f,white);
                    if(arrival>0)m.Arc(.5f,.82f,.25f,.10f,20,160,.018f,white);break;
                case 30:
                    for(int i=0;i<3;i++)m.Oval(.26f+i*.24f,.20f-drift*.03f,.009f+.005f*beat,.011f,light);
                    if(arrival>0){Twinkle(.51f,.76f,.065f*arrival,arrival);m.Oval(.50f,.67f,.07f*arrival,.018f,white);}break;
                case 31:
                    m.Arc(.5f,.41f,.27f,.24f,45+beat*5,115+beat*5,.017f,white);
                    if(arrival>0){Twinkle(.51f,.89f,.04f*arrival,arrival);m.Arc(.5f,.12f,.35f,.035f,190,350,.016f,white);}break;
                case 32:
                    m.Oval(.73f,.70f-drift*.10f,.018f,.023f,new Color(1,.85f,.38f,strength));
                    if(arrival>0){Twinkle(.48f,.77f,.045f*arrival,arrival);Twinkle(.25f,.28f,.04f*arrival,arrival);}break;
                case 33:
                    m.Oval(.25f,.84f+beat*.014f,.025f,.013f,light);m.Oval(.75f,.85f-beat*.014f,.025f,.013f,light);
                    if(arrival>0){Twinkle(.5f,.86f,.045f*arrival,arrival);m.Heart(.75f,.60f,.04f*arrival,white);}break;
                case 34:
                    m.Oval(.77f,.39f-drift*.09f,.013f,.025f,light);
                    if(arrival>0){m.Oval(.26f,.72f,.025f,.035f,white);Twinkle(.74f,.70f,.035f*arrival,arrival);}break;
                case 35:
                    for(int i=0;i<3;i++){float x=.30f+i*.20f;m.Line(x,.73f+beat*.025f,x+.015f,.76f+beat*.025f,.015f,white);}
                    if(arrival>0)m.Arc(.5f,.46f,.13f+arrival*.03f,.11f+arrival*.03f,0,360,.014f,white);break;
                case 36:
                    for(int i=0;i<2;i++)m.Arc(.45f+i*.14f+sway,.86f+drift*.03f,.023f,.05f,-80,110,.012f,white);
                    if(arrival>0)m.Heart(.46f,.86f,.06f*arrival,white);break;
                case 37:
                    m.Arc(.49f,.45f,.17f,.17f,drift*120,drift*120+70,.014f,white);
                    if(arrival>0){m.Round(.65f,.64f,.15f,.075f,.014f,white);Twinkle(.72f,.68f,.12f*arrival,arrival);}break;
                case 38:
                    m.Arc(.81f,.35f,.08f,.08f,30+beat*30,110+beat*30,.015f,white);
                    if(arrival>0){Twinkle(.81f,.35f,.055f*arrival,arrival);m.Oval(.48f,.89f+arrival*.045f,.019f,.019f,white);}break;
                case 39:
                    m.Line(.55f,.23f+beat*.02f,.73f,.23f+beat*.02f,.015f,light);
                    if(arrival>0){m.Heart(.31f,.31f,.05f*arrival,light);Twinkle(.5f,.82f,.045f*arrival,arrival);}break;
            }
        }
    }
}
