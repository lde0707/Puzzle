using UnityEngine;

namespace Puzzle.Gravity
{
    public sealed partial class SkinDecoration
    {
        private void Glaze(float x,float y,float radius)
        {
            m.Oval(x,y,radius,radius*.33f,new Color(1,1,1,.43f));
            m.Oval(x+radius*.91f,y-radius*.12f,radius*.18f,radius*.15f,new Color(1,1,1,.66f));
        }
        private void Stitch(float x,float y,float length,bool vertical=false)
        {
            var c=new Color(cream.r,cream.g,cream.b,.65f);
            for(int i=0;i<5;i++)
            {float p=i*length/5;m.Line(x+(vertical?0:p),y+(vertical?p:0),x+(vertical?.018f:p+.013f),y+(vertical?p+.012f:.018f),.009f,c);}
        }
        private void MaterialDetails()
        {
            // Concept-specific finishing stays clear of eyes and mouth, and within each body.
            var glint=new Color(1,1,1,.67f);var soft=new Color(highlight.r,highlight.g,highlight.b,.55f);
            switch(skin)
            {
                case 0:
                    m.Arc(.5f,.5f,.33f,.30f,55,128,.018f,glint);
                    for(int i=0;i<4;i++)m.Oval(.24f+i*.16f,.19f+(i%2)*.035f,.010f,.012f,soft);
                    m.Oval(.75f,.66f,.022f,.032f,new Color(1,1,1,.3f));break;
                case 1:
                    Stitch(.33f,.15f,.32f);m.Arc(.5f,.38f,.23f,.19f,195,270,.012f,soft);
                    for(int i=0;i<5;i++)m.Oval(.32f+i*.085f,.67f,.008f,.007f,new Color(.85f,.65f,.40f));
                    Glaze(.61f+sway,.89f,.06f);break;
                case 2:
                    for(int i=0;i<3;i++){m.Line(.22f+i*.04f,.25f,.23f+i*.04f,.21f,.012f,soft);m.Line(.58f+i*.04f,.25f,.59f+i*.04f,.21f,.012f,soft);}
                    m.Arc(.47f,.49f,.29f,.25f,60,130,.017f,soft);break;
                case 3:
                    m.Line(.30f+sway,.86f,.30f+sway,.93f,.016f,glint);m.Line(.66f-sway,.86f,.66f-sway,.93f,.016f,glint);
                    for(int i=0;i<3;i++){m.Oval(.25f+i*.035f,.12f,.008f,.018f,soft);m.Oval(.67f+i*.035f,.12f,.008f,.018f,soft);}break;
                case 4:
                    Stitch(.28f,.64f,.14f);Stitch(.70f,.32f,.21f,true);
                    m.Round(.54f,.22f,.085f,.08f,.018f,body);Stitch(.55f,.23f,.06f);
                    for(int i=0;i<4;i++)m.Line(.34f+i*.09f,.72f,.36f+i*.09f,.74f,.009f,soft);break;
                case 5:
                    for(int i=0;i<7;i++)m.Oval(.25f+i*.083f,.17f,.015f,.015f,gold);
                    m.Star(.49f,.84f,.025f,Color.white,45,4,.28f);
                    m.Line(.28f,.30f,.34f,.46f,.011f,soft);m.Line(.70f,.30f,.66f,.46f,.011f,soft);break;
                case 6:
                    m.Arc(.50f,.50f,.16f,.15f,48,145,.012f,glint);
                    for(int i=0;i<5;i++){float a=(i*72+90)*Mathf.Deg2Rad;m.Oval(.5f+.27f*Mathf.Cos(a),.49f+.27f*Mathf.Sin(a),.012f,.012f,glint);}break;
                case 7:
                    m.Line(.29f,.68f,.45f,.68f,.012f,new Color(.61f,.84f,1,.55f));
                    m.Oval(.68f,.66f,.025f,.012f,new Color(1,1,1,.4f));
                    m.Line(.24f,.14f,.38f,.14f,.012f,cream);m.Line(.62f,.14f,.76f,.14f,.012f,cream);
                    m.Star(.76f,.29f,.025f,gold);break;
                case 8:
                    m.Arc(.27f,.56f,.12f,.10f,60,155,.016f,glint);m.Arc(.74f,.54f,.11f,.09f,30,110,.013f,glint);
                    for(int i=0;i<3;i++)m.Oval(.271f+i*.22f,.24f+(still?0:.015f*Mathf.Sin(phase*2+i)),.009f,.020f,glint);break;
                case 9:
                    m.Arc(.31f,.69f,.09f,.08f,45,150,.012f,glint);m.Arc(.68f,.69f,.09f,.08f,30,135,.012f,soft);
                    m.Oval(.73f,.34f,.024f,.032f,glint);m.Oval(.721f,.35f,.007f,.009f,Color.white);break;
                case 10:
                    Stitch(.20f,.15f,.59f);m.Line(.47f,.13f,.47f,.30f,.012f,glint);
                    m.Arc(.34f,.86f+sway,.13f,.063f,20,150,.013f,glint);m.Arc(.66f,.86f-sway,.13f,.063f,30,150,.013f,glint);
                    m.Star(.50f,.85f,.032f,cream,45,4,.35f);break;
                case 11:
                    for(int i=0;i<2;i++){m.Oval(.22f+i*.56f,.75f,.018f,.018f,dark);m.Line(.21f+i*.56f,.75f,.23f+i*.56f,.75f,.007f,cream);}
                    m.Line(.29f,.68f,.48f,.68f,.011f,new Color(.76f,1,.94f,.55f));
                    for(int i=0;i<3;i++)m.Line(.085f,.47f+i*.045f,.13f,.47f+i*.045f,.010f,highlight);break;
                case 12:
                    m.Line(.22f,.34f,.78f,.34f,.010f,soft);m.Line(.22f,.60f,.78f,.60f,.012f,cream);
                    Stitch(.22f,.18f,.56f);m.Line(.56f,.79f,.56f,.89f,.010f,glint);
                    m.Oval(.22f,.49f,.010f,.010f,cream);m.Oval(.76f,.49f,.010f,.010f,cream);
                    m.Line(.50f,.95f,.59f,.95f,.014f,cream);break;
                case 13:
                    Stitch(.27f,.14f,.42f);m.Line(.30f,.81f,.48f,.92f,.014f,soft);
                    m.Star(.61f,.85f,.017f,cream);m.Oval(.41f,.83f,.009f,.009f,gold);
                    m.Oval(.90f,.49f,.025f,.025f,Color.white);m.Star(.79f,.53f,.018f,glint,45,4,.25f);break;
                case 14:
                    for(int i=0;i<5;i++){float a=(i*45+wave*3)*Mathf.Deg2Rad;m.Oval(.5f+.28f*Mathf.Cos(a),.60f+.18f*Mathf.Sin(a),.018f,.014f,soft);}
                    m.Line(.21f,.16f,.31f,.16f,.010f,new Color(.75f,.86f,.57f));m.Line(.69f,.16f,.79f,.16f,.010f,new Color(.75f,.86f,.57f));break;
                case 15:
                    Glaze(.29f,.75f,.058f);m.Line(.26f,.18f,.54f,.18f,.014f,soft);
                    m.Star(.71f,.60f,.028f,Color.white,45,4,.22f);break;
                case 16:
                    for(int i=0;i<3;i++){m.Line(.08f,.64f+i*.035f,.16f,.58f+i*.035f,.008f,new Color(1,1,1,.45f));m.Line(.84f,.58f+i*.035f,.92f,.64f+i*.035f,.008f,new Color(1,1,1,.45f));}
                    Glaze(.41f,.73f,.065f);break;
                case 17:
                    m.Line(.20f,.69f,.32f,.59f,.012f,glint);m.Line(.81f,.67f,.68f,.58f,.011f,glint);m.Line(.32f,.16f,.65f,.16f,.009f,glint);
                    m.Star(.25f,.67f,.035f,Color.white,45,4,.25f);break;
                case 18:
                    Glaze(.39f,.78f,.067f);m.Arc(.5f,.46f,.45f,.14f,215,315,.010f,new Color(1,1,1,.85f));
                    for(int i=0;i<3;i++)m.Oval(.35f+i*.04f,.33f,.006f,.006f,cream);break;
                case 19:
                    Stitch(.23f,.20f,.40f);Stitch(.18f,.30f,.28f,true);Stitch(.27f,.84f,.17f);
                    m.Oval(.23f,.32f,.022f,.028f,new Color(cream.r,cream.g,cream.b,.4f));
                    m.Oval(.28f,.27f,.014f,.017f,new Color(cream.r,cream.g,cream.b,.4f));break;
            }
        }
    }
}
