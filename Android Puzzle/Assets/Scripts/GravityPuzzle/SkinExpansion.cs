using UnityEngine;

namespace Puzzle.Gravity
{
    public sealed partial class SkinDecoration
    {
        // Every concept owns its silhouette and surface; all accessories stay inside one cell.
        private Color LeafGreen=>new Color(.35f,.62f,.40f);
        private void Fox()
        {
            m.Arc(.77f,.32f,.15f,.19f,-80,100+wave*12,.12f,body);m.Oval(.89f,.41f+sway,.055f,.085f,cream);
            for(int i=0;i<2;i++){float x=.14f+i*.47f;m.Poly(dark,new Vector2(x,.70f),new Vector2(x+.04f,.96f),new Vector2(x+.22f,.76f));m.Poly(cream,new Vector2(x+.06f,.76f),new Vector2(x+.07f,.88f),new Vector2(x+.16f,.77f));}
            Box(.10f,.11f,.73f,.69f,.12f,body);
            m.Poly(cream,new Vector2(.16f,.58f),new Vector2(.47f,.38f),new Vector2(.78f,.58f),new Vector2(.70f,.24f),new Vector2(.24f,.24f));
            Face(.47f,.49f,1.04f);m.Oval(.47f,.46f,.034f,.022f,dark);m.Line(.26f,.70f,.35f,.68f,.018f,highlight);
            Stitch(.27f,.17f,.38f);m.Oval(.26f,.14f,.075f,.04f,dark);m.Oval(.66f,.14f,.075f,.04f,dark);
        }
        private void Panda()
        {
            Bubble(.25f,.79f,.13f,.13f,dark);Bubble(.75f,.79f,.13f,.13f,dark);
            Box(.12f,.12f,.76f,.70f,.14f,body);m.Round(.19f,.33f,.62f,.43f,.14f,cream);
            m.Oval(.35f,.57f,.085f,.10f,dark);m.Oval(.65f,.57f,.085f,.10f,dark);Face(.5f,.52f,1.25f,false,true);
            m.Oval(.5f,.45f,.032f,.024f,dark);m.Oval(.25f,.17f,.10f,.055f,dark);m.Oval(.75f,.17f,.10f,.055f,dark);
            m.Line(.61f,.20f,.74f+sway,.40f,.035f,LeafGreen);m.Oval(.76f+sway,.35f,.07f,.025f,LeafGreen);m.Line(.65f,.25f,.69f,.25f,.012f,cream);
            Stitch(.30f,.24f,.22f);
        }
        private void Frog()
        {
            Box(.11f,.12f,.78f,.65f,.13f,body);Bubble(.34f,.77f,.16f,.14f,body);Bubble(.66f,.77f,.16f,.14f,body);
            m.Round(.21f,.19f,.58f,.30f,.11f,highlight);Face(.5f,.65f,1.36f);
            m.Oval(.25f,.14f,.13f,.045f,dark);m.Oval(.75f,.14f,.13f,.045f,dark);
            for(int i=0;i<3;i++){m.Oval(.20f+i*.035f,.14f,.012f,.019f,highlight);m.Oval(.72f+i*.035f,.14f,.012f,.019f,highlight);}
            m.Oval(.70f,.27f,.09f,.038f,LeafGreen);m.Line(.66f,.27f,.72f,.27f,.009f,cream);Glaze(.32f,.83f,.05f);
        }
        private void Penguin()
        {
            m.Oval(.12f,.42f+sway,.065f,.19f,dark);m.Oval(.88f,.42f-sway,.065f,.19f,dark);
            Box(.14f,.13f,.72f,.73f,.15f,body);m.Round(.25f,.21f,.50f,.51f,.16f,cream);Face(.5f,.55f,1.04f);
            m.Poly(gold,new Vector2(.46f,.50f),new Vector2(.54f,.50f),new Vector2(.50f,.455f));
            m.Round(.20f,.31f,.60f,.085f,.025f,dark);m.Round(.67f,.20f,.075f,.16f,.023f,dark);Stitch(.24f,.335f,.48f);
            m.Oval(.30f,.12f,.11f,.04f,gold);m.Oval(.70f,.12f,.11f,.04f,gold);Glaze(.32f,.77f,.07f);
        }
        private void Dinosaur()
        {
            for(int i=0;i<4;i++){float y=.24f+i*.16f;m.Poly(highlight,new Vector2(.79f,y),new Vector2(.96f,y+.07f+sway),new Vector2(.80f,y+.14f));}
            Box(.10f,.12f,.75f,.74f,.13f,body);m.Round(.22f,.17f,.49f,.29f,.10f,highlight);
            for(int i=0;i<3;i++)m.Line(.31f,.23f+i*.07f,.61f,.23f+i*.07f,.012f,body);
            Face(.47f,.57f,1.12f);m.Oval(.27f,.74f,.023f,.017f,dark);m.Oval(.65f,.74f,.023f,.017f,dark);
            m.Round(.39f,.47f,.033f,.05f,.008f,cream);m.Round(.54f,.47f,.033f,.05f,.008f,cream);
            m.Oval(.27f,.13f,.09f,.045f,dark);m.Oval(.68f,.13f,.09f,.045f,dark);Glaze(.40f,.78f,.07f);
        }
        private void Dragon()
        {
            for(int i=0;i<2;i++){float x=i==0?.08f:.92f,inner=i==0?.26f:.74f;m.Poly(dark,new Vector2(inner,.29f),new Vector2(x,.40f),new Vector2(x,.73f+sway),new Vector2(inner,.62f));m.Line(x,.69f+sway,inner,.43f,.015f,highlight);}
            m.Poly(cream,new Vector2(.23f,.75f),new Vector2(.26f,.96f),new Vector2(.37f,.79f));m.Poly(cream,new Vector2(.63f,.79f),new Vector2(.74f,.96f),new Vector2(.77f,.75f));
            Box(.16f,.12f,.68f,.71f,.12f,body);m.Round(.32f,.19f,.36f,.25f,.08f,highlight);
            for(int i=0;i<3;i++)m.Arc(.5f,.24f+i*.06f,.13f,.028f,190,350,.012f,body);
            Face(.5f,.55f,1.05f);m.Oval(.32f,.71f,.024f,.017f,dark);m.Oval(.68f,.71f,.024f,.017f,dark);m.Star(.5f,.77f,.04f,gold,90,4,.5f);
        }
        private void Turtle()
        {
            for(int i=0;i<2;i++){m.Oval(.20f+i*.60f,.19f,.10f,.07f,highlight);m.Oval(.14f+i*.72f,.60f+sway,.07f,.10f,highlight);}
            Box(.13f,.15f,.74f,.66f,.13f,dark);m.Round(.20f,.22f,.60f,.52f,.12f,body);
            m.Poly(highlight,new Vector2(.36f,.28f),new Vector2(.64f,.28f),new Vector2(.73f,.44f),new Vector2(.64f,.59f),new Vector2(.36f,.59f),new Vector2(.27f,.44f));
            m.Line(.22f,.26f,.36f,.36f,.018f,dark);m.Line(.78f,.26f,.64f,.36f,.018f,dark);m.Line(.5f,.23f,.5f,.35f,.018f,dark);
            m.Round(.29f,.53f,.42f,.34f,.12f,highlight);Face(.5f,.66f,.82f);m.Arc(.5f,.44f,.13f,.08f,0,360,.014f,dark);Glaze(.39f,.81f,.05f);
        }
        private void Hedgehog()
        {
            for(int i=0;i<6;i++){float x=.13f+i*.12f;m.Poly(dark,new Vector2(x,.74f),new Vector2(x+.06f,.95f+sway*.4f),new Vector2(x+.12f,.74f));}
            Box(.10f,.12f,.80f,.69f,.11f,dark);Box(.20f,.17f,.60f,.55f,.13f,body);Face(.5f,.49f,1.05f);m.Oval(.5f,.455f,.033f,.025f,dark);
            for(int i=0;i<5;i++){m.Line(.14f,.25f+i*.09f,.19f,.29f+i*.09f,.012f,highlight);m.Line(.81f,.29f+i*.09f,.86f,.25f+i*.09f,.012f,highlight);}
            m.Oval(.5f,.27f,.07f,.075f,cream);m.Round(.42f,.30f,.16f,.045f,.02f,dark);m.Line(.5f,.34f,.52f,.38f,.014f,dark);Stitch(.28f,.20f,.16f);
        }
        private void Axolotl()
        {
            for(int side=0;side<2;side++)for(int i=0;i<3;i++){float x=side==0?.20f:.80f,end=side==0?.065f:.935f,y=.38f+i*.14f;m.Line(x,.51f,end,y+sway,.044f,dark);m.Oval(end,y+sway,.035f,.055f,highlight);}
            Box(.17f,.13f,.66f,.70f,.14f,body);m.Round(.27f,.22f,.46f,.25f,.10f,highlight);Face(.5f,.53f,1.1f);
            for(int i=0;i<3;i++){m.Oval(.27f+i*.04f,.44f,.009f,.009f,dark);m.Oval(.65f+i*.04f,.44f,.009f,.009f,dark);}
            m.Oval(.31f,.14f,.08f,.04f,highlight);m.Oval(.69f,.14f,.08f,.04f,highlight);Glaze(.37f,.76f,.075f);
        }
        private void Octopus()
        {
            for(int i=0;i<4;i++){float x=.23f+i*.18f,y=.15f+.012f*wave*(i%2==0?1:-1);m.Oval(x,y,.09f,.085f,body);m.Oval(x,y-.025f,.034f,.019f,cream);}
            Box(.12f,.19f,.76f,.61f,.13f,body);Face(.5f,.49f,1.15f);
            m.Round(.29f,.77f,.42f,.10f,.04f,cream);m.Round(.34f,.85f,.32f,.05f,.025f,cream);m.Line(.31f,.79f,.69f,.79f,.018f,dark);m.Star(.5f,.845f,.028f,dark);
            for(int i=0;i<3;i++)m.Oval(.24f+i*.24f,.27f,.025f,.025f,highlight);Glaze(.27f,.69f,.05f);
        }
        private void Toast()
        {
            Box(.10f,.10f,.80f,.77f,.13f,dark);Box(.16f,.16f,.68f,.65f,.12f,body);m.Round(.22f,.23f,.56f,.50f,.11f,cream);
            for(int i=0;i<7;i++)m.Oval(.27f+i*.073f,.29f+(i%2)*.025f,.013f,.012f,body);
            Box(.37f,.67f,.26f,.15f,.04f,gold);m.Line(.42f,.78f,.57f,.78f,.018f,cream);Face(.5f,.48f,1.14f);
            m.Line(.19f,.22f,.19f,.62f,.014f,highlight);m.Arc(.5f,.55f,.30f,.23f,50,130,.014f,highlight);
        }
        private void Pudding()
        {
            m.Oval(.5f,.12f,.43f,.055f,cream);Box(.16f,.14f,.68f,.65f,.12f,body);
            m.Round(.16f,.66f,.68f,.14f,.07f,dark);for(int i=0;i<3;i++)m.Round(.24f+i*.22f,.55f+(i%2)*.04f,.08f,.17f,.035f,dark);
            m.Oval(.5f,.82f,.13f,.04f,cream);m.Oval(.5f,.88f,.057f,.056f,new Color(.85f,.35f,.45f));m.Line(.51f,.92f,.55f+sway,.97f,.014f,LeafGreen);
            Face(.5f,.43f,1.10f);m.Line(.23f,.23f,.23f,.52f,.021f,highlight);Glaze(.68f,.70f,.06f);
        }
        private void Waffle()
        {
            Box(.10f,.12f,.80f,.75f,.11f,dark);Box(.16f,.18f,.68f,.63f,.06f,body);
            for(int y=0;y<4;y++)for(int x=0;x<4;x++){float xx=.21f+x*.15f,yy=.23f+y*.14f;m.Round(xx,yy,.115f,.105f,.017f,dark);m.Line(xx+.02f,yy+.086f,xx+.095f,yy+.086f,.010f,highlight);}
            m.Round(.25f,.37f,.50f,.25f,.075f,body);Face(.5f,.47f,1.05f);Box(.41f,.70f,.19f,.12f,.025f,cream);
            m.Line(.65f,.77f,.73f,.77f,.030f,gold);m.Line(.73f,.77f,.73f,.65f,.030f,gold);m.Oval(.73f,.64f,.022f,.030f,gold);
        }
        private void Cake()
        {
            Box(.12f,.11f,.76f,.65f,.10f,body);m.Round(.16f,.26f,.68f,.06f,.016f,cream);m.Round(.16f,.40f,.68f,.055f,.016f,cream);
            m.Round(.12f,.67f,.76f,.13f,.055f,cream);for(int i=0;i<5;i++)m.Oval(.20f+i*.15f,.67f,.07f,.055f,cream);
            m.Oval(.50f,.85f,.085f,.09f,new Color(.86f,.36f,.49f));m.Star(.50f,.93f,.06f,LeafGreen,90,5,.45f);
            for(int i=0;i<3;i++)m.Oval(.46f+i*.04f,.84f+(i%2)*.04f,.006f,.012f,cream);
            Face(.5f,.51f,1.1f);m.Oval(.25f,.84f,.06f,.035f,cream);m.Oval(.75f,.84f,.06f,.035f,cream);Stitch(.22f,.17f,.52f);
        }
        private void Watermelon()
        {
            Box(.10f,.11f,.80f,.76f,.11f,LeafGreen);m.Round(.15f,.23f,.70f,.59f,.08f,cream);m.Round(.20f,.28f,.60f,.50f,.065f,body);
            for(int i=0;i<7;i++)m.Line(.19f+i*.10f,.13f,.21f+i*.095f,.22f,.020f,new Color(.22f,.46f,.32f));
            for(int i=0;i<5;i++)m.Oval(.29f+i*.105f,.70f-.025f*(i%2),.014f,.025f,dark);
            for(int i=0;i<3;i++)m.Oval(.29f+i*.21f,.34f,.012f,.021f,dark);Face(.5f,.50f,1.12f);Glaze(.30f,.75f,.05f);
        }
        private void Donut()
        {
            var dough=new Color(.77f,.53f,.34f);Box(.10f,.11f,.80f,.76f,.16f,dough);m.Round(.14f,.26f,.72f,.57f,.16f,body);
            for(int i=0;i<5;i++)m.Oval(.22f+i*.14f,.28f+(i%2)*.025f,.07f,.06f,body);
            m.Oval(.5f,.46f,.12f,.10f,dough);m.Oval(.5f,.47f,.081f,.065f,cream);Face(.5f,.58f,1.4f);
            for(int i=0;i<10;i++){float a=i*Mathf.PI*.2f,x=.5f+.27f*Mathf.Cos(a),y=.54f+.22f*Mathf.Sin(a);m.Line(x-.012f,y-.016f,x+.012f,y+.016f,.018f,i%2==0?cream:gold);}
            Glaze(.27f,.73f,.065f);Stitch(.25f,.17f,.49f);
        }
        private void Teacup()
        {
            m.Arc(.80f,.49f,.15f,.18f,-95,95,.065f,dark);m.Arc(.80f,.49f,.15f,.18f,-85,85,.030f,highlight);
            m.Oval(.47f,.12f,.39f,.045f,highlight);Box(.12f,.15f,.69f,.62f,.10f,body);
            m.Oval(.465f,.76f,.32f,.055f,cream);m.Oval(.465f,.766f,.26f,.033f,new Color(.47f,.31f,.25f));
            Face(.465f,.49f,1.10f);m.Arc(.465f,.28f,.24f,.055f,190,350,.018f,cream);m.Heart(.465f,.27f,.055f,cream);Glaze(.25f,.64f,.05f);
            m.Line(.67f,.76f,.71f,.56f,.009f,cream);m.Round(.66f,.49f,.09f,.10f,.014f,cream);m.Oval(.705f,.54f,.017f,.017f,body);
            m.Arc(.37f+sway,.87f,.035f,.055f,-70,100,.015f,new Color(1,1,1,.6f));
        }
        private void Camera()
        {
            m.Arc(.5f,.65f,.39f,.29f,10,170,.027f,dark);m.Round(.22f,.77f,.14f,.09f,.02f,gold);m.Round(.58f,.76f,.21f,.12f,.03f,dark);
            Box(.10f,.13f,.80f,.64f,.10f,body);m.Round(.16f,.22f,.68f,.36f,.04f,dark);
            m.Oval(.49f,.45f,.23f,.23f,highlight);m.Oval(.49f,.45f,.19f,.19f,dark);m.Oval(.49f,.45f,.15f,.15f,body);Face(.49f,.44f,.78f,false,true);
            m.Arc(.49f,.45f,.13f,.13f,40,135,.017f,cream);m.Round(.65f,.64f,.15f,.075f,.015f,cream);m.Oval(.25f,.68f,.021f,.021f,gold);
            for(int i=0;i<4;i++)m.Line(.80f,.28f+i*.055f,.83f,.28f+i*.055f,.012f,highlight);Stitch(.19f,.18f,.52f);
        }
        private void Detective()
        {
            Box(.13f,.12f,.70f,.63f,.12f,body);m.Round(.18f,.73f,.62f,.15f,.06f,dark);m.Round(.28f,.86f,.39f,.07f,.03f,body);m.Line(.14f,.74f,.86f,.74f,.045f,dark);
            for(int i=0;i<5;i++)m.Line(.26f+i*.10f,.76f,.26f+i*.10f,.86f,.012f,highlight);m.Line(.22f,.81f,.75f,.81f,.012f,highlight);
            m.Poly(cream,new Vector2(.22f,.37f),new Vector2(.47f,.22f),new Vector2(.37f,.43f));m.Poly(cream,new Vector2(.72f,.37f),new Vector2(.47f,.22f),new Vector2(.57f,.43f));
            Face(.47f,.53f,1.04f);m.Oval(.48f,.18f,.020f,.020f,gold);
            m.Line(.81f,.25f,.91f,.13f,.035f,dark);m.Arc(.81f,.35f,.10f,.10f,0,360,.025f,gold);m.Oval(.81f,.35f,.085f,.085f,new Color(.7f,.88f,1,.35f));m.Line(.76f,.38f,.81f,.42f,.014f,cream);
        }
        private void Postman()
        {
            Box(.12f,.12f,.76f,.64f,.11f,body);m.Round(.18f,.74f,.64f,.13f,.06f,dark);m.Round(.24f,.85f,.52f,.07f,.03f,body);m.Line(.37f,.75f,.84f,.75f,.045f,dark);
            m.Round(.43f,.78f,.13f,.065f,.01f,cream);m.Line(.44f,.83f,.495f,.80f,.009f,gold);m.Line(.55f,.83f,.495f,.80f,.009f,gold);
            m.Line(.23f,.68f,.76f,.19f,.06f,dark);Face(.5f,.54f,1.1f);
            Box(.47f,.14f,.35f,.23f,.04f,dark);m.Round(.50f,.25f,.29f,.07f,.015f,body);m.Oval(.64f,.26f,.017f,.017f,gold);
            m.Round(.20f,.23f,.22f,.14f,.015f,cream);m.Line(.21f,.35f,.31f,.28f,.011f,body);m.Line(.41f,.35f,.31f,.28f,.011f,body);Stitch(.23f,.17f,.16f);
        }
    }
}
