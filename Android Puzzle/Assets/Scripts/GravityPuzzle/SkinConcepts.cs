using UnityEngine;

namespace Puzzle.Gravity
{
    public sealed partial class SkinDecoration
    {
        private void Royal()
        {
            Box(.12f,.10f,.76f,.70f,.13f,body);
            m.Round(.19f,.14f,.62f,.10f,.035f,dark);m.Line(.20f,.18f,.80f,.18f,.045f,cream);
            m.Line(.23f,.26f,.28f,.69f,.027f,highlight);m.Line(.77f,.26f,.72f,.69f,.027f,highlight);
            Box(.25f,.40f,.50f,.34f,.11f,highlight);Face(.5f,.54f,.98f);
            m.Poly(gold,new Vector2(.24f,.77f),new Vector2(.22f,.95f),new Vector2(.39f,.86f),new Vector2(.50f,.98f),new Vector2(.61f,.86f),new Vector2(.78f,.95f),new Vector2(.76f,.77f));
            m.Line(.27f,.78f,.73f,.78f,.045f,cream);m.Oval(.5f,.84f,.04f,.043f,body);m.Oval(.5f,.30f,.045f,.045f,gold);
        }
        private void Candy()
        {
            // Folded wrapper ends are secondary to the broad square bonbon.
            m.Poly(dark,new Vector2(.04f,.30f),new Vector2(.04f,.68f),new Vector2(.22f,.56f),new Vector2(.22f,.40f));
            m.Poly(dark,new Vector2(.96f,.30f),new Vector2(.96f,.68f),new Vector2(.78f,.56f),new Vector2(.78f,.40f));
            m.Line(.07f,.39f,.18f,.46f,.022f,highlight);m.Line(.93f,.59f,.82f,.52f,.022f,highlight);
            Box(.13f,.10f,.74f,.76f,.14f,body);
            m.Round(.21f,.19f,.58f,.60f,.12f,Color.Lerp(body,Color.white,.12f));
            m.Star(.5f,.75f,.105f,gold,90+wave*5,5,.53f);
            m.Line(.25f,.66f,.30f,.75f,.035f,cream);m.Star(.72f,.29f,.045f,cream,45+wave*15,4,.25f);
            Face(.5f,.47f,1.08f);m.Oval(.29f,.25f,.055f,.028f,highlight);
        }
        private void Astronaut()
        {
            Box(.06f,.38f,.16f,.30f,.05f,dark);Box(.78f,.38f,.16f,.30f,.05f,dark);
            Box(.13f,.10f,.74f,.72f,.14f,body);
            Box(.19f,.39f,.62f,.36f,.11f,cream);Box(.23f,.42f,.54f,.30f,.085f,new Color(.19f,.24f,.39f));
            m.Line(.30f,.67f,.48f,.67f,.022f,new Color(.67f,.85f,.96f,.7f));Face(.5f,.53f,1.04f,false,true);
            Box(.30f,.19f,.40f,.14f,.04f,highlight);m.Oval(.39f,.26f,.025f,.025f,gold);m.Oval(.60f,.26f,.024f,.024f,Color.Lerp(body,Color.white,.5f+wave*.25f));
            m.Line(.75f,.79f,.81f,.92f,.025f,dark);m.Oval(.82f,.93f,.034f,.034f,gold);
            m.Line(.23f,.13f,.39f,.13f,.03f,dark);m.Line(.61f,.13f,.77f,.13f,.03f,dark);
        }
        private void Cloud()
        {
            m.Arc(.5f,.65f,.25f,.27f,20,160,.045f,body);m.Arc(.5f,.65f,.19f,.22f,20,160,.035f,gold);
            Box(.10f,.12f,.80f,.68f,.16f,body);
            Bubble(.23f,.76f,.13f,.10f,body);Bubble(.50f,.79f,.18f,.12f,body);Bubble(.77f,.75f,.13f,.10f,body);
            m.Round(.18f,.52f,.64f,.23f,.08f,body);Face(.5f,.49f,1.12f);
            for(int i=0;i<3;i++){float y=.23f+(still?0:.015f*Mathf.Sin(phase*2+i));m.Oval(.28f+i*.22f,y,.025f,.044f,highlight);}
        }
        private void Clover()
        {
            Box(.12f,.10f,.76f,.75f,.14f,body);
            // Four rounded leaf panels fit into one square rather than making a round clover silhouette.
            for(int y=0;y<2;y++)for(int x=0;x<2;x++)
            {
                float xx=.20f+x*.30f,yy=.18f+y*.31f;
                m.Round(xx,yy,.30f,.32f,.13f,Color.Lerp(body,x==y?highlight:dark,.23f));
                m.Line(.5f,.49f,xx+.15f,yy+.16f,.016f,highlight);
            }
            m.Oval(.5f,.49f,.21f,.17f,body);Face(.5f,.49f,1.04f);
            m.Oval(.28f+sway*.2f,.73f,.028f,.038f,new Color(1,1,1,.7f));
            m.Line(.50f,.26f,.55f+sway,.17f,.025f,dark);
        }
        private void Gift()
        {
            Box(.13f,.09f,.74f,.65f,.10f,body);
            for(int y=0;y<3;y++)for(int x=0;x<4;x++)m.Oval(.23f+x*.18f,.19f+y*.17f,.023f,.023f,highlight);
            m.Round(.44f,.11f,.12f,.60f,.01f,cream);m.Round(.16f,.27f,.68f,.085f,.01f,cream);
            Box(.08f,.66f,.84f,.15f,.04f,body);m.Round(.44f,.69f,.12f,.11f,.01f,cream);
            m.Oval(.34f,.86f+sway,.17f,.095f,dark);m.Oval(.66f,.86f-sway,.17f,.095f,dark);
            m.Oval(.34f,.87f+sway,.10f,.042f,cream);m.Oval(.66f,.87f-sway,.10f,.042f,cream);m.Oval(.5f,.84f,.075f,.08f,gold);
            m.Round(.28f,.38f,.44f,.21f,.07f,highlight);Face(.5f,.46f,.85f);
        }
        private void Robot()
        {
            m.Line(.5f,.80f,.5f+sway,.93f,.035f,dark);m.Oval(.5f+sway,.94f,.04f,.04f,gold);
            Box(.05f,.41f,.15f,.28f,.06f,dark);Box(.80f,.41f,.15f,.28f,.06f,dark);Box(.15f,.11f,.70f,.70f,.13f,body);
            Box(.21f,.38f,.58f,.34f,.09f,new Color(.19f,.24f,.34f));
            Color led=new Color(.76f,1,.83f);
            for(int i=0;i<2;i++){float x=.31f+i*.29f;m.Round(x,.51f,.07f,.09f,.013f,led);m.Round(x+.015f,.56f,.025f,.027f,.005f,Color.white);}
            m.Line(.45f,.46f,.55f,.46f,.018f,led);
            for(int i=0;i<5;i++){float height=.04f+(still?.03f:(Mathf.Sin(phase*3+i)+1)*.035f);m.Round(.29f+i*.085f,.23f,.045f,height,.01f,i%2==0?gold:cream);}
            m.Line(.21f,.13f,.33f,.13f,.03f,dark);m.Line(.67f,.13f,.79f,.13f,.03f,dark);
        }
        private void Boat()
        {
            // The wooden hull itself is the block; the little sail is an accessory on its deck.
            Box(.12f,.10f,.76f,.66f,.12f,body);
            m.Round(.17f,.19f,.66f,.13f,.04f,dark);m.Line(.19f,.22f,.81f,.22f,.025f,highlight);
            m.Line(.18f,.65f,.82f,.65f,.045f,cream);m.Line(.18f,.72f,.82f,.72f,.035f,dark);
            m.Line(.48f,.68f,.48f,.96f,.026f,dark);
            m.Poly(cream,new Vector2(.26f,.75f),new Vector2(.44f,.94f),new Vector2(.44f,.75f));
            m.Poly(highlight,new Vector2(.53f,.94f),new Vector2(.74f,.75f),new Vector2(.53f,.75f));
            m.Line(.55f,.80f,.65f,.80f,.018f,body);Face(.5f,.46f,1.04f);
            m.Oval(.23f,.47f,.046f,.046f,gold);m.Oval(.77f,.47f,.046f,.046f,gold);
            m.Oval(.23f,.47f,.027f,.027f,dark);m.Oval(.77f,.47f,.027f,.027f,dark);
            for(int i=0;i<4;i++)m.Arc(.23f+i*.18f+sway*.3f,.13f,.085f,.023f,190,350,.018f,highlight);
        }
        private void Wizard()
        {
            Box(.12f,.10f,.74f,.70f,.12f,body);
            m.Line(.23f,.18f,.28f,.65f,.025f,highlight);m.Line(.75f,.18f,.70f,.65f,.025f,dark);
            m.Star(.31f,.25f,.053f,gold);m.Star(.69f,.22f,.035f,cream);m.Line(.5f,.17f,.5f,.35f,.022f,highlight);
            Box(.25f,.40f,.48f,.32f,.12f,highlight);Face(.49f,.53f,.96f);
            m.Poly(dark,new Vector2(.19f,.76f),new Vector2(.53f,.98f),new Vector2(.74f+sway,.89f),new Vector2(.65f,.76f));
            m.Poly(body,new Vector2(.26f,.78f),new Vector2(.53f,.94f),new Vector2(.68f+sway,.88f),new Vector2(.61f,.79f));
            m.Line(.12f,.76f,.84f,.76f,.07f,dark);m.Line(.22f,.78f,.76f,.78f,.020f,gold);m.Star(.49f,.87f,.035f,gold);
            m.Line(.90f,.19f,.90f,.44f,.023f,dark);m.Star(.90f,.49f,.065f,gold,90+wave*8);
        }
        private void Flower()
        {
            Box(.12f,.10f,.76f,.75f,.14f,body);
            Color leaf=new Color(.36f,.62f,.43f);
            m.Oval(.26f,.16f,.10f,.035f,leaf);m.Oval(.74f,.16f,.10f,.035f,leaf);
            // Petals frame the square cushion, leaving its four straight sides visible.
            for(int i=0;i<5;i++)
            {
                float a=(i*180f/4+wave*3)*Mathf.Deg2Rad;
                Bubble(.5f+.28f*Mathf.Cos(a),.58f+.18f*Mathf.Sin(a),.105f,.12f,highlight);
            }
            Box(.23f,.29f,.54f,.37f,.13f,cream);Face(.5f,.46f,.98f);
            m.Oval(.44f,.61f,.035f,.014f,Color.white);m.Star(.50f,.76f,.035f,gold);
        }
        private void Love()
        {
            Box(.12f,.10f,.76f,.76f,.16f,body);
            float pulse=1+wave*.035f;
            m.Heart(.5f,.51f,.31f*pulse,Color.Lerp(body,dark,.18f));
            m.Heart(.5f,.53f,.285f*pulse,Color.Lerp(body,highlight,.30f));
            m.Heart(.76f,.72f,.045f,cream);Face(.5f,.48f,1.08f);
            m.Line(.24f,.75f,.31f,.78f,.033f,highlight);m.Oval(.74f,.23f,.047f,.024f,highlight);
        }
        private void Bee()
        {
            float flutter=still?0:Mathf.Sin(phase*5)*.025f;
            m.Oval(.13f,.64f,.10f,.15f+flutter,highlight);m.Oval(.87f,.64f,.10f,.15f-flutter,highlight);
            m.Arc(.13f,.64f,.066f,.10f,0,360,.014f,Color.white);m.Arc(.87f,.64f,.066f,.10f,0,360,.014f,Color.white);
            Box(.15f,.10f,.70f,.70f,.13f,body);
            m.Round(.20f,.26f,.60f,.08f,.02f,dark);m.Round(.23f,.15f,.54f,.06f,.02f,dark);
            m.Line(.38f,.78f,.31f+sway,.91f,.026f,dark);m.Line(.62f,.78f,.69f-sway,.91f,.026f,dark);
            m.Oval(.31f+sway,.92f,.036f,.036f,dark);m.Oval(.69f-sway,.92f,.036f,.036f,dark);Face(.5f,.53f,1.04f);
        }
        private void Crystal()
        {
            m.Poly(dark,new Vector2(.25f,.87f),new Vector2(.75f,.87f),new Vector2(.89f,.73f),new Vector2(.89f,.23f),new Vector2(.75f,.09f),new Vector2(.25f,.09f),new Vector2(.11f,.23f),new Vector2(.11f,.73f));
            m.Round(.15f,.13f,.70f,.70f,.10f,body);
            m.Poly(highlight,new Vector2(.26f,.83f),new Vector2(.74f,.83f),new Vector2(.65f,.71f),new Vector2(.35f,.71f));
            m.Poly(Color.Lerp(body,Color.white,.35f),new Vector2(.85f,.71f),new Vector2(.85f,.25f),new Vector2(.72f,.34f),new Vector2(.72f,.63f));
            m.Poly(highlight,new Vector2(.26f,.13f),new Vector2(.74f,.13f),new Vector2(.65f,.25f),new Vector2(.35f,.25f));
            m.Poly(Color.Lerp(body,dark,.25f),new Vector2(.15f,.25f),new Vector2(.15f,.71f),new Vector2(.28f,.63f),new Vector2(.28f,.34f));
            for(int i=0;i<6;i++){float a=i*Mathf.PI/3;m.Line(.5f,.73f,.5f+.065f*Mathf.Cos(a),.73f+.065f*Mathf.Sin(a),.014f,cream);}
            Face(.5f,.48f,1.08f);m.Star(.73f,.66f,.04f*(1+wave*.15f),Color.white,45,4,.22f);
        }
        private void Planet()
        {
            m.Arc(.5f,.46f,.45f,.14f,0,180,.055f,dark);
            Box(.15f,.10f,.70f,.74f,.14f,body);
            m.Arc(.5f,.72f,.28f,.05f,185,355,.042f,highlight);m.Arc(.5f,.26f,.28f,.045f,190,350,.045f,dark);
            m.Oval(.66f,.70f,.04f,.020f,dark);Face(.5f,.53f,1.04f);
            m.Arc(.5f,.46f,.45f,.14f,180,360,.06f,cream);m.Arc(.5f,.46f,.45f,.14f,185,355,.018f,body);
            float a=still?.6f:phase*.65f;float x=.5f+.40f*Mathf.Cos(a),y=.55f+.36f*Mathf.Sin(a);
            m.Oval(x,y,.041f,.041f,gold);m.Oval(x-.01f,y+.01f,.012f,.012f,cream);
        }
        private void Moon()
        {
            Box(.12f,.10f,.76f,.73f,.15f,body);
            m.Round(.20f,.17f,.60f,.57f,.13f,Color.Lerp(body,dark,.10f));
            // The crescent is stitched into the night cushion instead of cutting away the body.
            for(int i=0;i<24;i++)
            {
                float t=i/24f,u=(i+1)/24f,a=(65+t*230)*Mathf.Deg2Rad,b=(65+u*230)*Mathf.Deg2Rad;
                var p=new Vector2(.71f+.105f*Mathf.Cos(a),.66f+.115f*Mathf.Sin(a));var q=new Vector2(.71f+.105f*Mathf.Cos(b),.66f+.115f*Mathf.Sin(b));
                var r=new Vector2(.754f-.085f*Mathf.Sin(u*Mathf.PI),.764f-.208f*u);var v=new Vector2(.754f-.085f*Mathf.Sin(t*Mathf.PI),.764f-.208f*t);
                m.Poly(cream,p,q,r,v);
            }
            m.Poly(dark,new Vector2(.17f,.80f),new Vector2(.40f,.97f),new Vector2(.69f+sway,.86f),new Vector2(.47f,.80f));
            m.Poly(body,new Vector2(.23f,.82f),new Vector2(.40f,.93f),new Vector2(.63f+sway,.86f),new Vector2(.46f,.83f));
            m.Line(.18f,.80f,.50f,.80f,.045f,cream);m.Oval(.69f+sway,.85f,.05f,.05f,cream);
            Face(.48f,.46f,1.04f,true);m.Star(.72f,.27f,.047f,gold,90+wave*6);m.Star(.27f,.68f,.031f,highlight,45,4,.3f);
        }
    }
}
