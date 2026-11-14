using UnityEngine;

namespace Puzzle.Gravity
{
    public static class SkinEffectMotifs
    {
        public static readonly string[] Descriptions={
            "젤리 방울 → 설탕꽃", "새싹 잎 → 꽃핀 새싹", "고양이 발자국 → 하트 발바닥", "달 조각 → 달과 별", "솜실 → 봉제 하트",
            "금빛 보석 → 작은 왕관", "별사탕 → 포장 사탕", "로켓 불꽃 → 우주별", "구름 방울 → 무지개", "이슬잎 → 네잎클로버",
            "리본 조각 → 선물 리본", "음표 → 리듬 파동", "배 물결 → 돛과 물보라", "마법 불씨 → 별 마법진", "꽃잎 → 활짝 핀 꽃",
            "작은 하트 → 겹하트", "꿀방울 → 벌집", "얼음 결정 → 눈꽃", "별 궤적 → 고리 행성", "졸린 달 → 달빛 반짝임",
            "여우 발자국 → 단풍잎", "대나무잎 → 죽순", "연못 파문 → 수련", "빙하 조각 → 얼음별", "공룡 발자국 → 공룡 알",
            "용의 불꽃 → 날개 불꽃", "거북 발자국 → 등껍질", "밤송이 가시 → 도토리", "산호 방울 → 아가미 꽃", "빨판 흔적 → 잉크 방울",
            "빵 부스러기 → 버터 토스트", "카라멜 방울 → 푸딩", "꿀 조각 → 와플", "크림 꽃 → 딸기", "수박 씨 → 수박 조각",
            "스프링클 → 도넛", "차의 김 → 찻잔", "필름 조각 → 셔터 반짝임", "탐정 발자국 → 돋보기", "작은 편지 → 하트 편지"};
        public static void Draw(SkinMesh m,int id,bool arrival,Color c)
        {
            Color white=new Color(1,.98f,.90f,c.a),gold=new Color(1,.73f,.25f,c.a),green=new Color(.40f,.72f,.48f,c.a);
            void Star(float x=.5f,float y=.5f,float r=.35f)=>m.Star(x,y,r,gold,90,4,.22f);
            void Leaf(float x,float y,float rx=.23f){m.Oval(x,y,rx,.10f,green);m.Line(x-rx*.7f,y,x+rx*.7f,y,.025f,white);}
            void Paw(){m.Oval(.5f,.37f,.20f,.15f,c);for(int i=0;i<3;i++)m.Oval(.27f+i*.23f,.64f+(.12f*(i==1?1:0)),.085f,.11f,c);}
            void Bubble(){m.Arc(.48f,.48f,.27f,.29f,0,360,.05f,c);m.Arc(.48f,.48f,.19f,.21f,60,145,.04f,white);}
            void Heart(){m.Heart(.5f,.5f,.34f,c);m.Oval(.37f,.64f,.055f,.035f,white);}
            void Snow(){for(int j=0;j<6;j++){float a=j*Mathf.PI/3;Vector2 v=new Vector2(Mathf.Cos(a),Mathf.Sin(a));m.Line(.5f,.5f,.5f+v.x*.38f,.5f+v.y*.38f,.035f,c);m.Oval(.5f+v.x*.25f,.5f+v.y*.25f,.055f,.055f,white);}}
            void Wave(){for(int j=0;j<2;j++)m.Arc(.5f,.35f+j*.24f,.38f,.12f,190,350,.045f,c);}
            void Envelope(){m.Round(.13f,.25f,.74f,.5f,.06f,c);m.Line(.15f,.72f,.5f,.44f,.035f,white);m.Line(.5f,.44f,.85f,.72f,.035f,white);}
            switch(id)
            {
                case 0: if(!arrival)Bubble();else{for(int j=0;j<5;j++){float a=j*Mathf.PI*.4f;m.Oval(.5f+Mathf.Cos(a)*.23f,.5f+Mathf.Sin(a)*.23f,.12f,.12f,c);}m.Oval(.5f,.5f,.10f,.10f,gold);}break;
                case 1: Leaf(.5f,.5f);if(arrival){m.Line(.5f,.2f,.5f,.75f,.05f,green);Leaf(.33f,.57f,.18f);Star(.5f,.82f,.13f);}break;
                case 2: Paw();if(arrival)m.Heart(.5f,.37f,.14f,white);break;
                case 3: m.Arc(.53f,.52f,.3f,.34f,65,295,.13f,c);if(arrival)Star(.72f,.64f,.16f);break;
                case 4: if(arrival)Heart();else m.Arc(.5f,.5f,.29f,.15f,0,300,.06f,c);for(int j=0;j<3;j++)m.Line(.33f+j*.16f,.40f,.35f+j*.16f,.56f,.023f,white);break;
                case 5: if(!arrival)m.Poly(gold,new Vector2(.5f,.9f),new Vector2(.85f,.5f),new Vector2(.5f,.1f),new Vector2(.15f,.5f));else{m.Poly(gold,new Vector2(.15f,.25f),new Vector2(.10f,.75f),new Vector2(.35f,.52f),new Vector2(.5f,.88f),new Vector2(.65f,.52f),new Vector2(.90f,.75f),new Vector2(.85f,.25f));m.Line(.20f,.29f,.8f,.29f,.06f,white);}break;
                case 6: if(!arrival)Star();else{m.Poly(c,new Vector2(.05f,.25f),new Vector2(.05f,.75f),new Vector2(.4f,.5f));m.Poly(c,new Vector2(.95f,.25f),new Vector2(.95f,.75f),new Vector2(.6f,.5f));m.Round(.27f,.25f,.46f,.5f,.12f,gold);Star(.5f,.5f,.18f);}break;
                case 7: if(!arrival){m.Poly(c,new Vector2(.2f,.7f),new Vector2(.5f,.05f),new Vector2(.8f,.7f));m.Poly(white,new Vector2(.38f,.7f),new Vector2(.5f,.3f),new Vector2(.62f,.7f));}else{Star();m.Arc(.5f,.5f,.42f,.22f,0,360,.03f,c);}break;
                case 8: if(!arrival){m.Oval(.35f,.44f,.22f,.17f,c);m.Oval(.6f,.5f,.24f,.23f,c);}else for(int j=0;j<3;j++)m.Arc(.5f,.3f,.36f-j*.09f,.36f-j*.09f,0,180,.055f,j==0?c:j==1?gold:green);break;
                case 9: if(!arrival){Leaf(.5f,.4f);m.Oval(.5f,.72f,.07f,.1f,white);}else{for(int j=0;j<4;j++){float a=j*Mathf.PI/2;m.Heart(.5f+Mathf.Cos(a)*.19f,.5f+Mathf.Sin(a)*.19f,.19f,green);}m.Line(.5f,.4f,.4f,.1f,.035f,green);}break;
                case 10: m.Arc(.32f,.6f,.19f,.15f,0,360,.07f,c);m.Arc(.68f,.6f,.19f,.15f,0,360,.07f,c);if(arrival){m.Line(.48f,.5f,.3f,.15f,.09f,c);m.Line(.52f,.5f,.7f,.15f,.09f,c);}m.Oval(.5f,.55f,.09f,.09f,gold);break;
                case 11: if(!arrival){m.Oval(.3f,.25f,.15f,.09f,c);m.Line(.43f,.25f,.43f,.8f,.06f,c);m.Line(.43f,.8f,.7f,.68f,.08f,c);}else for(int j=0;j<5;j++)m.Round(.12f+j*.16f,.25f,.10f,.25f+.3f*Mathf.Sin(j*.8f),.035f,j%2==0?c:gold);break;
                case 12: Wave();if(arrival){m.Line(.5f,.38f,.5f,.9f,.03f,c);m.Poly(white,new Vector2(.54f,.9f),new Vector2(.54f,.5f),new Vector2(.85f,.5f));}break;
                case 13: m.Star(.5f,.5f,.35f,c,90,5,.45f);if(arrival){m.Arc(.5f,.5f,.45f,.45f,0,360,.025f,gold);Star(.5f,.5f,.12f);}break;
                case 14: if(!arrival)m.Oval(.5f,.5f,.15f,.32f,c);else{for(int j=0;j<6;j++){float a=j*Mathf.PI/3;m.Oval(.5f+Mathf.Cos(a)*.23f,.5f+Mathf.Sin(a)*.23f,.14f,.14f,c);}m.Oval(.5f,.5f,.13f,.13f,gold);}break;
                case 15: Heart();if(arrival)m.Heart(.5f,.48f,.18f,white);break;
                case 16: if(!arrival){m.Oval(.5f,.4f,.21f,.25f,gold);m.Poly(gold,new Vector2(.29f,.48f),new Vector2(.5f,.87f),new Vector2(.71f,.48f));}else{m.Star(.5f,.5f,.4f,gold,30,6,.86f);m.Star(.5f,.5f,.27f,c,30,6,.86f);}break;
                case 17: if(arrival)Snow();else m.Star(.5f,.5f,.34f,c,45,4,.55f);break;
                case 18: if(!arrival){Star(.65f,.55f,.22f);m.Line(.1f,.3f,.5f,.5f,.055f,c);}else{m.Oval(.5f,.5f,.26f,.26f,c);m.Arc(.5f,.5f,.45f,.13f,0,360,.05f,gold);}break;
                case 19: m.Arc(.48f,.5f,.25f,.32f,75,290,.13f,gold);if(arrival){Star(.79f,.7f,.12f);Star(.75f,.25f,.08f);}break;
                case 20: if(!arrival){Paw();m.Line(.3f,.82f,.3f,.92f,.025f,c);m.Line(.7f,.82f,.7f,.92f,.025f,c);}else{m.Star(.5f,.55f,.37f,c,90,5,.58f);m.Line(.5f,.15f,.5f,.7f,.035f,gold);}break;
                case 21: if(!arrival){Leaf(.4f,.4f,.27f);Leaf(.6f,.66f,.23f);}else{m.Poly(green,new Vector2(.23f,.15f),new Vector2(.5f,.9f),new Vector2(.77f,.15f));for(int j=0;j<3;j++)m.Line(.35f,.3f+j*.15f,.65f,.3f+j*.15f,.03f,white);}break;
                case 22: if(!arrival){Wave();m.Oval(.5f,.78f,.06f,.10f,c);}else{m.Arc(.5f,.45f,.37f,.25f,35,345,.12f,green);m.Heart(.5f,.62f,.20f,c);m.Oval(.5f,.55f,.06f,.06f,gold);}break;
                case 23: if(!arrival)m.Poly(c,new Vector2(.1f,.4f),new Vector2(.3f,.8f),new Vector2(.8f,.7f),new Vector2(.9f,.3f));else{m.Star(.5f,.5f,.4f,c,90,6,.45f);m.Star(.5f,.5f,.2f,white,90,6,.45f);}break;
                case 24: if(!arrival){m.Poly(c,new Vector2(.25f,.2f),new Vector2(.2f,.7f),new Vector2(.4f,.55f),new Vector2(.5f,.88f),new Vector2(.6f,.55f),new Vector2(.8f,.7f),new Vector2(.75f,.2f));}else{m.Oval(.5f,.48f,.27f,.37f,white);m.Oval(.4f,.6f,.07f,.08f,c);m.Oval(.62f,.35f,.08f,.09f,c);}break;
                case 25: m.Poly(c,new Vector2(.2f,.2f),new Vector2(.08f,.6f),new Vector2(.4f,.47f),new Vector2(.5f,.94f),new Vector2(.65f,.5f),new Vector2(.9f,.67f),new Vector2(.8f,.2f));if(arrival)m.Star(.5f,.42f,.22f,gold,90,3,.5f);break;
                case 26: if(!arrival){m.Oval(.3f,.3f,.11f,.15f,c);m.Oval(.7f,.65f,.11f,.15f,c);}else{m.Star(.5f,.5f,.4f,green,30,6,.87f);m.Star(.5f,.5f,.25f,c,30,6,.87f);for(int j=0;j<3;j++){float a=j*Mathf.PI*2/3;m.Line(.5f,.5f,.5f+Mathf.Cos(a)*.36f,.5f+Mathf.Sin(a)*.36f,.025f,white);}}break;
                case 27: if(!arrival)m.Star(.5f,.5f,.33f,c,90,8,.44f);else{m.Oval(.5f,.4f,.23f,.28f,gold);m.Round(.22f,.55f,.56f,.21f,.08f,c);m.Line(.5f,.75f,.56f,.88f,.06f,c);}break;
                case 28: if(!arrival){Bubble();m.Oval(.77f,.79f,.09f,.09f,c);}else{for(int j=0;j<5;j++){float a=j*Mathf.PI*.4f;m.Line(.5f,.5f,.5f+Mathf.Cos(a)*.35f,.5f+Mathf.Sin(a)*.35f,.09f,c);}m.Oval(.5f,.5f,.12f,.12f,white);}break;
                case 29: if(!arrival){m.Arc(.3f,.35f,.14f,.14f,0,360,.05f,c);m.Arc(.65f,.65f,.19f,.19f,0,360,.05f,c);}else{m.Oval(.5f,.5f,.27f,.3f,c);for(int j=0;j<4;j++){float a=j*Mathf.PI/2;m.Oval(.5f+Mathf.Cos(a)*.36f,.5f+Mathf.Sin(a)*.36f,.07f,.07f,c);}m.Oval(.4f,.63f,.07f,.04f,white);}break;
                case 30: if(!arrival){for(int j=0;j<3;j++)m.Round(.20f+j*.22f,.25f+(j%2)*.27f,.13f,.13f,.03f,gold);}else{m.Round(.18f,.15f,.64f,.7f,.15f,c);m.Round(.25f,.23f,.5f,.53f,.1f,white);m.Round(.37f,.4f,.25f,.2f,.03f,gold);}break;
                case 31: if(!arrival){m.Oval(.5f,.4f,.18f,.27f,gold);m.Line(.5f,.6f,.5f,.88f,.05f,gold);}else{m.Poly(gold,new Vector2(.18f,.2f),new Vector2(.3f,.73f),new Vector2(.7f,.73f),new Vector2(.82f,.2f));m.Round(.29f,.62f,.42f,.15f,.04f,c);m.Oval(.5f,.85f,.09f,.09f,c);}break;
                case 32: m.Round(.17f,.17f,.66f,.66f,.08f,gold);if(arrival){for(int j=0;j<3;j++){m.Line(.3f+j*.2f,.2f,.3f+j*.2f,.8f,.035f,c);m.Line(.2f,.3f+j*.2f,.8f,.3f+j*.2f,.035f,c);}}else m.Oval(.5f,.5f,.14f,.14f,white);break;
                case 33: if(!arrival){m.Star(.5f,.5f,.32f,white,90,6,.7f);m.Oval(.5f,.5f,.1f,.1f,c);}else{m.Heart(.5f,.45f,.34f,c);for(int j=0;j<3;j++)m.Oval(.33f+j*.17f,.45f,.025f,.04f,gold);Leaf(.5f,.8f,.2f);}break;
                case 34: if(!arrival){m.Oval(.5f,.5f,.09f,.25f,c);m.Oval(.48f,.63f,.02f,.06f,white);}else{m.Poly(c,new Vector2(.15f,.2f),new Vector2(.5f,.85f),new Vector2(.85f,.2f));m.Line(.15f,.2f,.85f,.2f,.09f,green);m.Oval(.5f,.46f,.035f,.06f,white);}break;
                case 35: if(!arrival){for(int j=0;j<3;j++)m.Line(.2f+j*.25f,.3f,.3f+j*.25f,.6f,.065f,j==1?gold:c);}else{m.Arc(.5f,.5f,.28f,.28f,0,360,.18f,c);for(int j=0;j<4;j++){float a=j*Mathf.PI/2;m.Oval(.5f+Mathf.Cos(a)*.28f,.5f+Mathf.Sin(a)*.28f,.04f,.04f,gold);}}break;
                case 36: if(!arrival){m.Arc(.4f,.4f,.16f,.25f,-80,90,.04f,c);m.Arc(.6f,.7f,.12f,.2f,95,270,.03f,white);}else{m.Round(.16f,.2f,.57f,.5f,.13f,c);m.Arc(.75f,.48f,.16f,.17f,-90,90,.07f,c);m.Arc(.45f,.82f,.10f,.12f,0,240,.03f,white);}break;
                case 37: if(!arrival){m.Round(.15f,.17f,.7f,.66f,.03f,c);m.Round(.25f,.29f,.5f,.4f,.01f,white);for(int j=0;j<3;j++)m.Oval(.2f+j*.3f,.22f,.025f,.025f,gold);}else{Star();m.Arc(.5f,.5f,.42f,.42f,0,360,.025f,c);m.Star(.5f,.5f,.17f,white,45,4,.2f);}break;
                case 38: if(!arrival){m.Oval(.4f,.62f,.11f,.22f,c);m.Oval(.68f,.22f,.11f,.14f,c);}else{m.Arc(.43f,.61f,.25f,.25f,0,360,.06f,c);m.Line(.6f,.42f,.83f,.16f,.10f,gold);Star(.4f,.65f,.11f);}break;
                case 39: Envelope();if(arrival)m.Heart(.5f,.5f,.18f,gold);break;
            }
        }
    }
}
