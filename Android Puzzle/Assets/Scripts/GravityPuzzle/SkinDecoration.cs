using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Shared rounded-block proportions, with concept-specific materials, accessories and motion.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed partial class SkinDecoration : MaskableGraphic
    {
        public int skin,variant;
        public bool happy;
        public bool silhouette;
        public Vector2 gaze;
        private float phase;
        private bool still;
        private SkinMesh m;
        private Color body,dark,highlight,ink,cream,gold;
        private float wave,sway;
        private static readonly string[] Palettes={
            "FCA17D 66D1D9 FFD15E", "DC855C 6CAFC0 A59AD7", "D89362 868EC9 D984A3", "EDA4BB 82C4CD B2A1E2",
            "B98258 91A7C9 CE91AC", "BD6688 6A90CB 82B69B", "F4BA53 AC87DD 63C5C3", "D78666 6BB7CE AD91D8",
            "89C5DE DCA6C2 AAA1DB", "72B284 8C9DD2 D89BB5", "E77985 70BCA8 A294D7", "6AAEBB C684AB D7AC58",
            "CE8871 81ACCD A39BD2", "8B73C5 539EAA BE799B", "ED9D76 B19BDC 78B8C9", "DE799B 8B9AD2 78BCA9",
            "E6B94C A48BCF 72BDB8", "71BBD7 BC95D6 79BEAA", "B890D5 E7A269 70B8BA", "DBAF69 8BA8D6 C18FBA",
            "E69A65 8BAED2 C991B5", "8FAF9C 9BA8D2 D49CB1", "83BC80 79BCCA BE9BCC", "7298B8 B494C4 D7A275",
            "93B971 75B3C4 C98EAE", "B497D7 74B9AE E6A37B", "7EAE93 9C9CCB C9A26A", "B78D72 90A6C0 BD91B0",
            "E5A1B9 82C2C4 B09CD5", "D48DAE 8CA7D2 C6AD6C", "DBA26C 94BFA4 C99CB9", "E7BE71 89C3B2 B69CCD",
            "C99B66 86B5BC BD95BC", "E59CAE 90BBC4 B1A0D2", "E88F9A 8ABCB3 C4A0D5", "D989AC 84BEBB C1A0D7",
            "78B7B5 C399BA DCB16E", "8E9FC4 C695A9 82B8A6", "B79C7B 8FAAC5 B795BF", "729EBD BD96C4 CAA76D"
        };
        private static readonly Color[] Colors=BuildColors();
        private static Color[] BuildColors()
        {
            var colors=new Color[Palettes.Length*3];
            for(int id=0;id<Palettes.Length;id++)for(int v=0;v<3;v++)ColorUtility.TryParseHtmlString("#"+Palettes[id].Substring(v*7,6),out colors[id*3+v]);
            return colors;
        }
        public static readonly string[] Concepts={
            "탱글한 젤리 · 반짝이는 설탕막", "토분과 흙 · 바람에 흔들리는 새싹", "네모 고양이 · 줄무늬 털과 꼬리", "달토끼 · 긴 귀와 폭신한 발", "봉제 곰 · 바느질과 보송한 배",
            "작은 임금님 · 벨벳 망토와 왕관", "별사탕 큐브 · 설탕막과 포장 날개", "우주 탐험가 · 유리 헬멧과 우주복", "몽실 구름 · 빗방울과 무지개", "클로버 블록 · 네 잎 무늬와 이슬",
            "선물 상자 · 포장 무늬와 큰 리본", "리듬 로봇 · 화면 얼굴과 이퀄라이저", "꼬마 배 블록 · 나무 선체와 돛", "꼬마 마법사 · 커다란 모자와 별 망토", "봄꽃 친구 · 꽃잎과 잎사귀 발",
            "하트 젤리 · 사각 몸과 두근대는 하트", "통통 꿀벌 · 줄무늬와 팔랑이는 날개", "얼음 큐브 · 투명한 면과 눈꽃", "네모 행성 · 고리와 공전하는 위성", "달빛 쿠션 · 초승달과 수면 모자",
            "숲속 여우 · 뾰족 귀와 크림색 꼬리", "대나무 판다 · 눈 무늬와 작은 죽순", "연못 개구리 · 볼록 눈과 수련 잎", "빙하 펭귄 · 포근한 배와 목도리", "꼬마 공룡 · 등 가시와 말랑한 배",
            "구름 용 · 작은 뿔과 비늘 날개", "느긋한 거북 · 조각 등껍질과 네 발", "밤송이 고슴 · 부드러운 가시와 도토리", "아홀로틀 · 산호빛 아가미와 물방울", "산책 문어 · 빨판 발과 선원 모자",
            "버터 토스트 · 구운 테두리와 버터", "카라멜 푸딩 · 시럽과 체리 한 알", "바삭 와플 · 격자 무늬와 꿀 한 방울", "딸기 케이크 · 크림 층과 생딸기", "수박 한입 · 줄무늬 껍질과 씨앗",
            "네모 도넛 · 폭신한 아이싱과 스프링클", "오후의 찻잔 · 도자기 손잡이와 김", "찰칵 카메라 · 커다란 렌즈와 스트랩", "체크 탐정 · 사냥 모자와 돋보기", "우편 배달부 · 작은 가방과 편지"
        };
        public static Color ColorFor(int id,int colorIndex)
        {
            return Colors[Mathf.Clamp(id,0,Palettes.Length-1)*3+Mathf.Clamp(colorIndex,0,2)];
        }
        public void Pose(float time,bool reduced)
        {
            if(canvasRenderer.cull)return; // Scrolled-out collection portraits do not rebuild animation meshes.
            float next=reduced?0:Mathf.Floor(time*30)/30;
            if(phase==next&&still==reduced)return;phase=next;still=reduced;SetVerticesDirty();
        }
        public void Mood(bool arrived,Vector2 direction)
        {if(happy==arrived&&gaze==direction)return;happy=arrived;gaze=direction;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)=>Draw(vh,rectTransform.rect);
        public void Draw(VertexHelper vh,Rect rect)
        {
            vh.Clear();m=new SkinMesh(vh,rect,true);body=ColorFor(skin,variant);ink=PuzzleUI.Ink;
            dark=Color.Lerp(body,ink,.47f);highlight=Color.Lerp(body,Color.white,.65f);
            cream=new Color(1,.95f,.84f);gold=new Color(1,.81f,.34f);
            wave=still?0:Mathf.Sin(phase*(travelling?7f:ArrivalPulse>0?9f:1.65f)+variant*.6f);sway=wave*.025f;
            if(!silhouette)m.Oval(.5f,.065f,.30f,.032f,new Color(.25f,.16f,.37f,.10f));
            switch(skin)
            {
                case 0:Jelly();break;case 1:Plant();break;case 2:Cat();break;case 3:Bunny();break;case 4:Bear();break;
                case 5:Royal();break;case 6:Candy();break;case 7:Astronaut();break;case 8:Cloud();break;case 9:Clover();break;
                case 10:Gift();break;case 11:Robot();break;case 12:Boat();break;case 13:Wizard();break;case 14:Flower();break;
                case 15:Love();break;case 16:Bee();break;case 17:Crystal();break;case 18:Planet();break;case 19:Moon();break;
                case 20:Fox();break;case 21:Panda();break;case 22:Frog();break;case 23:Penguin();break;case 24:Dinosaur();break;
                case 25:Dragon();break;case 26:Turtle();break;case 27:Hedgehog();break;case 28:Axolotl();break;case 29:Octopus();break;
                case 30:Toast();break;case 31:Pudding();break;case 32:Waffle();break;case 33:Cake();break;case 34:Watermelon();break;
                case 35:Donut();break;case 36:Teacup();break;case 37:Camera();break;case 38:Detective();break;case 39:Postman();break;default:Jelly();break;
            }
            if(silhouette)
            {
                // Flatten the complete outline: no face, material, palette or motion clues.
                var vertex=UIVertex.simpleVert;
                for(int i=0;i<vh.currentVertCount;i++)
                {vh.PopulateUIVertex(ref vertex,i);vertex.color=new Color(.48f,.42f,.58f,1);vh.SetUIVertex(vertex,i);}
            }
            else {MaterialDetails();DrawMotionDetails();}
        }
        private void Bubble(float x,float y,float rx,float ry,Color color)
        {m.Oval(x,y-.014f,rx,ry,Color.Lerp(color,ink,.27f));m.Oval(x,y+.007f,rx*.93f,ry*.93f,color);}
        private void Box(float x,float y,float w,float h,float radius,Color color)
        {m.Round(x,y,w,h,radius,Color.Lerp(color,ink,.27f));m.Round(x+.018f,y+.035f,w-.036f,h-.05f,radius*.85f,color);}
        private void Face(float x,float y,float span=1,bool sleepy=false,bool bright=false)
        {
            Color c=bright?cream:ink;float dx=gaze.x*.018f,dy=gaze.y*.018f;
            bool blink=!still&&Mathf.Repeat(phase+variant*.73f,4.4f)>4.24f;
            for(int i=0;i<2;i++)
            {
                float ex=x+(i==0?-.115f:.115f)*span+dx;
                if(sleepy||blink)m.Arc(ex,y+.04f*span+dy,.043f*span,.025f*span,190,350,.023f*span,c);
                else if(happy)m.Arc(ex,y+.025f*span+dy,.042f*span,.04f*span,15,165,.024f*span,c);
                else {m.Oval(ex,y+.055f*span+dy,.028f*span,.043f*span,c);m.Oval(ex-.008f*span,y+.071f*span+dy,.008f*span,.012f*span,Color.white);}
            }
            m.Arc(x,y-.005f*span,.054f*span,.040f*span,195,345,.018f*span,c);
            m.Oval(x-.19f*span,y-.018f*span,.046f*span,.022f*span,new Color(1,.45f,.57f,.38f));m.Oval(x+.19f*span,y-.018f*span,.046f*span,.022f*span,new Color(1,.45f,.57f,.38f));
        }
        private void Jelly()
        {
            Box(.10f,.10f,.80f,.78f,.16f,body);m.Oval(.28f,.73f,.11f,.045f,new Color(1,1,1,.6f));
            m.Oval(.69f,.25f,.075f,.04f,highlight);m.Oval(.27f,.27f,.025f,.025f,highlight);Face(.5f,.49f,1.2f);
        }
        private void Plant()
        {
            Color leaf=new Color(.31f,.64f,.40f);
            m.Line(.5f,.61f,.5f+sway,.84f,.045f,leaf);m.Oval(.36f+sway,.80f,.16f,.075f,leaf);m.Oval(.63f+sway,.87f,.15f,.08f,leaf);
            m.Line(.26f+sway,.81f,.45f+sway,.79f,.016f,new Color(.71f,.85f,.46f));
            Box(.15f,.10f,.70f,.60f,.11f,body);
            Box(.13f,.56f,.74f,.14f,.045f,body);m.Oval(.5f,.666f,.28f,.035f,new Color(.36f,.25f,.27f));
            m.Line(.30f,.19f,.32f,.43f,.027f,highlight);Face(.5f,.36f,.90f);
        }
        private void Cat()
        {
            m.Arc(.78f,.28f,.14f,.17f,-90,140+wave*12,.09f,dark);
            m.Poly(dark,new Vector2(.14f,.65f),new Vector2(.15f,.96f),new Vector2(.40f,.77f));m.Poly(dark,new Vector2(.60f,.77f),new Vector2(.85f,.96f),new Vector2(.86f,.65f));
            m.Poly(highlight,new Vector2(.19f,.72f),new Vector2(.20f,.88f),new Vector2(.34f,.77f));m.Poly(highlight,new Vector2(.66f,.77f),new Vector2(.80f,.88f),new Vector2(.81f,.72f));
            Box(.10f,.10f,.74f,.69f,.14f,body);m.Oval(.47f,.30f,.23f,.16f,highlight);
            for(int i=0;i<3;i++)m.Line(.37f+i*.10f,.76f,.39f+i*.08f,.65f,.035f,dark);
            Face(.47f,.46f,1.08f);m.Poly(dark,new Vector2(.435f,.44f),new Vector2(.505f,.44f),new Vector2(.47f,.40f));
            for(int i=0;i<2;i++){m.Line(.12f,.38f+i*.07f,.29f,.40f+i*.035f,.018f,dark);m.Line(.65f,.40f+i*.035f,.82f,.38f+i*.07f,.018f,dark);}
        }
        private void Bunny()
        {
            Bubble(.32f+sway,.78f,.12f,.20f,body);Bubble(.68f-sway,.78f,.12f,.20f,body);
            m.Oval(.32f+sway,.80f,.055f,.14f,cream);m.Oval(.68f-sway,.80f,.055f,.14f,cream);
            Box(.13f,.10f,.74f,.67f,.14f,body);m.Oval(.5f,.27f,.20f,.12f,highlight);
            m.Oval(.29f,.12f,.12f,.07f,highlight);m.Oval(.71f,.12f,.12f,.07f,highlight);Face(.5f,.43f,1.12f);
            m.Round(.477f,.315f,.046f,.066f,.012f,Color.white);
        }
        private void Bear()
        {
            Bubble(.23f,.77f,.16f,.16f,body);Bubble(.77f,.77f,.16f,.16f,body);m.Oval(.23f,.78f,.09f,.09f,cream);m.Oval(.77f,.78f,.09f,.09f,cream);
            Box(.12f,.10f,.76f,.70f,.14f,body);m.Oval(.5f,.29f,.23f,.18f,cream);m.Oval(.5f,.48f,.12f,.08f,cream);
            Face(.5f,.51f,1.08f);m.Oval(.5f,.48f,.042f,.029f,dark);m.Oval(.25f,.15f,.10f,.09f,dark);m.Oval(.75f,.15f,.10f,.09f,dark);
            for(int i=0;i<3;i++)m.Line(.48f,.17f+i*.038f,.52f,.19f+i*.038f,.012f,dark);
        }
        public static SkinDecoration Add(Transform parent,int id,int colorIndex)
        {
            var result=PuzzleUI.Rect("Concept skin",parent).gameObject.AddComponent<SkinDecoration>();
            PuzzleUI.Fill(result.rectTransform);result.skin=id;result.variant=colorIndex;result.raycastTarget=false;return result;
        }
        public static RectTransform Portrait(Transform parent,int id,int colorIndex,out SkinDecoration decoration)
        {var root=PuzzleUI.Rect(PuzzleCollection.Names[id],parent);decoration=Add(root,id,colorIndex);return root;}
    }
}
