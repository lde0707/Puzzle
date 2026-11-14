from pathlib import Path
import json
root=Path(__file__).resolve().parents[2]
folder=root/'Docs/ReleaseAudit-2026-10-31'
data=json.loads((folder/'audit-data.json').read_text(encoding='utf-8'))
code='''import {Stack,Row,Grid,H1,H2,H3,Text,Stat,Callout,Table,CollapsibleSection,LineChart,Link,Button,useCanvasState} from "cursor/canvas";
const data=DATA;
export default function ReleaseAudit(){
 const [chapter,setChapter]=useCanvasState("mallang-release-chapter",0);
 const levels=data.levels.slice(chapter*16,chapter*16+16);
 return <Stack gap={20} style={{padding:24,maxWidth:1200,margin:"0 auto"}}>
  <Text>2026-10-31 · Unity 6000.3.10f1 · 공개 웹 v18 · 출시 전 품질 검증</Text>
  <H1>데굴데굴: 핵심 퍼즐 통과, 정식 출시는 보류</H1>
  <Callout tone="warning" title="저장 충돌·연결 복구를 먼저 해결해야 합니다.">확정 결함 4건과 앱 출시 관문 6개를 분리했습니다. 실제 모바일 기기 및 서명 AAB/IPA 검증은 남아 있습니다. 자동 검사 통과를 완성도 100%로 해석하지 않습니다.</Callout>
  <Grid columns="repeat(auto-fit,minmax(160px,1fr))"><Stat value="64 / 64" label="초기 상태 클리어 가능"/><Stat value="16 / 16" label="Unity 검사 묶음 통과"/><Stat value="8 / 8" label="Node·HTTP 검사 통과"/><Stat value="4건" label="새 실패 주입으로 재현한 결함" tone="warning"/></Grid>
  <Text>{data.boundaries}</Text>
  <Link href="REPORT">화면 증거·재현 로그를 포함한 전체 HTML 보고서 열기</Link>
  <H2>확정 결함</H2>
  {data.issues.map(x=><CollapsibleSection key={x.id} title={x.severity+" · "+x.id+" · "+x.title} defaultOpen={x.severity==="P1"}><Stack gap={8}><Text>{x.status}</Text><Text>{x.source}</Text><Text>재현: {x.repro}</Text><Text>결과: {x.actual}</Text><Text>수정: {x.fix}</Text><Text>완료 기준: {x.accept}</Text></Stack></CollapsibleSection>)}
  <H2>앱 출시 관문</H2><Table headers={["ID","우선순위","항목","현재 확인","완료 조건"]} rows={data.gates}/>
  <H2>단계별 최단 이동 수</H2><Text>가로축: 단계 번호(단계) · 세로축: 최소 이동 명령 수(회). 출처: 2026-10-31 초기 상태 BFS. 사람의 체감 난이도를 나타내는 점수는 아닙니다.</Text>
  <Row wrap>{["1–16 기본","17–32 포탈","33–48 크기","49–64 길바꿈"].map((title,i)=><Button key={title} onClick={()=>setChapter(i)}>{title}</Button>)}</Row>
  <LineChart categories={levels.map(x=>String(x.stage))} series={[{name:"최단 이동(회)",data:levels.map(x=>x.shortest),tone:"info"}]} height={230} valueSuffix="회" showValues/>
  <Table headers={["단계","ID","최단 이동(회)","맵(가로×세로)","친구 수","최단 해 수","정답 깊이까지 탐색 상태"]} rows={levels.map(x=>[x.stage,x.room,x.shortest,x.width+"×"+x.height,x.friends,x.shortestSolutionCount,x.statesThroughAuthoredDepth])}/>
  <Callout tone="info">각 묶음의 최단 이동 수는 비감소합니다. 17·33·49단계는 새 규칙을 배우는 완화 구간입니다. 55단계의 탐색 상태 증가(54→417)는 실사용자 밸런스 검토 후보이며, 불공정 난이도를 입증하지는 않습니다.</Callout>
  <H2>기능별 검증 범위</H2><Table headers={["영역","판정","검증 내용","한계·주의"]} rows={data.coverage}/>
  <CollapsibleSection title="Unity 검사 16종 상세"><Table headers={["검사","결과","시간(초)"]} rows={data.unity.results.map(x=>[x.name,x.passed?"PASS":"FAIL",x.seconds.toFixed(3)])}/><Text>코어 검사 8,497,737 assertions에는 반복 프레임/좌표 비교가 포함됩니다. 서로 다른 기능 849만 개를 검사했다는 의미는 아닙니다.</Text></CollapsibleSection>
  <H2>밸런스·경제·성능·사용성</H2>{data.notes.map(x=><CollapsibleSection key={x[0]} title={x[0]}><Text>{x[1]}</Text></CollapsibleSection>)}
  <H2>수정 후 실기 검수</H2><Text>F01/F02 → F03/F04 → 네이티브 기능 범위·출시 설정 → 실제 기기와 비공개 테스트 순서로 진행합니다.</Text><Table headers={["ID","영역","절차","합격 기준"]} rows={data.manual}/>
  <H2>공식 정책 근거</H2>{data.sources.map(x=><Stack key={x[0]} gap={4}><Link href={x[1]}>{x[0]}</Link><Text>{x[2]}</Text></Stack>)}
  <Text>검증 범위: Windows Editor, 실제 데스크톱 WebGL 조작, Unity 렌더, 격리 API, 공개 파일 읽기. 미검증: 실제 모바일 하드웨어, 장기 발열·메모리·청취, 최종 설치 패키지, 스토어 심사. 개인정보·서버 사용자 기록은 변경하지 않았습니다.</Text>
 </Stack>;
}
'''.replace('DATA',json.dumps(data,ensure_ascii=False)).replace('REPORT',(folder/'index.html').as_posix())
(folder/'mallang-release-audit.canvas.tsx').write_text(code,encoding='utf-8')
print('Canvas source generated with inline audit data.')
