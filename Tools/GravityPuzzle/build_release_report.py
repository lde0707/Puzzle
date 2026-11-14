from pathlib import Path
import json, html, shutil

root=Path(__file__).resolve().parents[2]
out=root/'Docs/ReleaseAudit-2026-10-31';out.mkdir(parents=True,exist_ok=True)
logs=root/'Android Puzzle/Logs'
campaign=json.loads((logs/'campaign-audit.json').read_text(encoding='utf-8'))
unity=json.loads((logs/'release-audit-results.json').read_text(encoding='utf-8'))
network=json.loads((logs/'release-audit-public-assets.json').read_text(encoding='utf-8'))
issues=[
 dict(id='F01',severity='P1',title='첫 서버 연결 실패 후 기록 전송이 복구되지 않음',status='재현 완료 · shipped JS / 격리 VM',source='Sites/mallang-orbit/public/community.js:27,65,85',repro='기존 동의 계정으로 진입 → 첫 enroll 요청만 실패 → 서버 정상화 → 새 기록 생성 → 랭킹 열기.',actual='connected=false. 요청은 enroll 1회와 ranking 1회뿐이며 sync는 0회. 실패 안내도 없음. 새로고침 또는 online 이벤트 전까지 기록이 서버에 반영되지 않음. 로컬 기록 자체가 지워진 것은 아님.',fix='연결 재시도·타임아웃, 랭킹 진입 시 재연결, 성공 후 Unity 최신 스냅샷 재요청. 미연결 상태를 동기화 성공으로 취급하지 않기.',accept='최초 503/타임아웃 후 회복하면 새로고침 없이 최신 별·이동·초기화가 올라가고 상태 안내가 정확해야 함.'),
 dict(id='F02',severity='P1',title='동일 저장소의 오래된 인스턴스가 새 스킨을 덮어씀',status='재현 완료 · C# 저장 서비스 / 임시 폴더',source='Android Puzzle/Assets/Scripts/GravityPuzzle/ProgressSaveService.cs:81–115',repro='별 9개 저장 → A/B가 같은 세대 Load → A가 스킨 1개 뽑아 별 6개·소유 0,1 저장 → B가 예전 데이터로 효과음 설정 저장.',actual='두 Save 모두 true지만 최종 상태는 별 9개·소유 0. 새 스킨이 사라짐. 파일 서비스 충돌은 입증했으며, 브라우저 두 탭의 IndexedDB 동작을 실제 재현한 것은 아님.',fix='웹 단일 활성 게임 탭 잠금, 저장 세대 충돌 감지 및 명확한 복구 정책. 별 소비를 단순 max 병합하면 중복 소비/복구 문제가 생기므로 원장 또는 단일 작성자 정책 필요.',accept='두 탭·두 인스턴스 동시 실행에서 후발 설정 저장이 스킨/진행을 회귀시키지 않아야 함. 잠금 안내와 정상 인계도 검증.'),
 dict(id='F03',severity='P2',title='늦은 랭킹 응답이 이름 변경 화면에 섞임',status='재현 완료 · 격리 VM',source='Sites/mallang-orbit/public/community.js:73,83',repro='랭킹 GET 응답을 지연 → 이름 바꾸기 선택 → 이전 랭킹 응답 도착.',actual='제목은 나의 별친구 이름이고 입력칸 1개인데 랭킹 표 1개가 함께 삽입됨. balance도 화면 소유권 확인이 없음.',fix='화면 세대 번호 또는 요청 취소를 사용하고, 완료 시 요청을 시작한 화면인지 확인.',accept='느린 네트워크에서 랭킹/이름/통계를 연속 전환해도 이전 응답이 현재 화면을 변경하지 않아야 함.'),
 dict(id='F04',severity='P2',title='단계 목록에 상위 % 대신 이동·초기화가 남아 있음',status='재현 완료 · 390×844 실제 WebGL 화면 + 소스',source='Android Puzzle/Assets/Scripts/GravityPuzzle/GravityPuzzleApp.cs:500–507',repro='타이틀 → 단계 선택.',actual='16개 타일에 이동 0회·초기화 0회가 표시되고 상위 %는 없음. 상세 패널의 등급·누적 횟수는 정상. 사용자가 확정한 목록 표시 요구와 다름.',fix='목록의 두 카운터를 상위 구간 등급 표시로 교체. 상세 패널의 기존 누적 수치는 유지.',accept='64개 타일 모두 등급만 표시하고 상세 진입 시 초기화·누적 이동을 확인할 수 있어야 함.'),
]
gates=[
 ['G01','앱 출시 차단','Android 빌드·서명·아이콘','현재 Editor에는 AndroidPlayer 모듈이 없음. 릴리스 keystore/alias 비어 있고 useCustomKeystore=0. 앱 아이콘 슬롯도 비어 있음. AAB 제작·실기 설치·최종 manifest 검증을 수행할 수 없는 상태.','Android Build Support/SDK/NDK/JDK 설치 → 고유 패키지·업로드 키·아이콘 확정 → 서명 AAB 생성·내부 테스트.'],
 ['G02','앱 기능 결정 필요','웹과 네이티브 기능 차이','Android/iOS 랭킹은 웹에서 가능하다는 안내와 로컬 누적 기록만 표시. 서버 동의·동기화·실데이터 등급은 WebGL 전용. 네이티브 백업도 파일 경로 저장 수준.','같은 기능으로 출시할지 오프라인 앱으로 출시할지 확정. 같은 기능이면 HTTPS API, 동의/철회, 복구·공유 UI를 앱에 구현.'],
 ['G03','스토어 제출 전 필수','개인정보처리방침·지원·스토어 정보','검색한 게임 UI/웹 소스에 개인정보처리방침·지원 링크가 없음. 동의 문구만으로 스토어 정책 페이지를 대체할 수 없음. 계정 콘솔에 별도 제출된 문서는 확인하지 않음.','실제 수집 항목과 보존·삭제 정책에 맞는 공개 페이지, 앱 내 접근, Data safety/연령 등급/지원 연락처를 준비.'],
 ['G04','경쟁 랭킹 출시 전 필수','클라이언트 자체 보고 기록 신뢰','이동 0·기록 없음·누적 별 309 스냅샷과 B16을 1회 perfect로 주장한 스냅샷이 실제 validateSnapshot에서 허용됨. 현재 화면에는 테스트 랭킹임을 명시함.','베타용 표시를 유지하거나 서버 최단 이동·보상 검증, 이벤트 원장, 등록/전송 제한을 도입. 경쟁 보상과 연결하지 않기.'],
 ['G05','미검증 출시 관문','실제 기기와 스토어 빌드','이번 검증은 Windows Unity Editor 및 데스크톱 Chromium 계열 IAB. iPhone Safari·Android 기기·저사양 기기·스토어 설치 빌드는 직접 시험하지 않음.','Android 저/중/고사양, iPhone Safari/네이티브, 백그라운드·강제 종료·저장공간 부족·30분 연속 플레이를 실기 확인.'],
 ['G06','iOS 출시 차단','Mac/Xcode·서명·심사 패키지','iOS Support는 설치됨. Apple 팀 ID/프로비저닝은 비어 있음. 이 Windows에서 Xcode 최종 아카이브와 기기 서명은 검증하지 못함.','Mac 또는 macOS 빌드 환경에서 현재 SDK 요건을 만족하는 Xcode 아카이브, TestFlight 설치·검증.'],
]
coverage=[
 ['이동·충돌','통과','64개 초기 상태 최단 경로 BFS; C#와 독립 참조 전이 비교; 벽·친구·경계·2×2 통로 검사','모든 도달 가능 상태에서 탈출 가능한 게임이라는 뜻은 아님'],
 ['포탈','통과','입구 면·출구 점유·전송 후 멈춤·벽 포탈·시각적 순간이동과 효과 단절 검증','실제 iPhone 터치로 출구 발광 체감은 미검증'],
 ['크기 변환','통과','1×1/2×2 전환·커질 공간·도착 크기·저장/되돌리기·정답 검증','큰 블록 효과의 저사양 GPU 비용 미측정'],
 ['길바꿈 블록','통과','턴 parity·빈 이동 턴·친구 점유 시 닫힘 유예·포탈 출구·Undo/Reset/Save','1,920개 독립 전이 비교와 16개 C# BFS'],
 ['진행 중 조작','통과','이동 중 입력 거절·Undo/Reset·중첩 pause·큰 delta·30/60fps 결과 일치','OS 통화/잠금에 따른 네이티브 이벤트는 미검증'],
 ['힌트','통과','625개 정답 경로 상태, 다른 첫 방향 256개 중 228개 경로/28개 막힘/탐색 제한 0개','3회 한도·같은 상태 무료 재조회·저장 실패 롤백. 50,000노드 상한의 모든 상태 완전성은 미보장'],
 ['별 평가','통과','최단+3 이내와 되돌리기 미사용의 두 조건에 따라 3/2/1; 차액만 지급; 재클리어 중복 방지','힌트 사용은 일반 별 점수와 무관하고 perfect 여부에만 영향'],
 ['퍼펙트 등급','통과','최소 이동·Undo/Hint 미사용; 과거 정보가 없는 기록은 perfect 추정 안 함','0.1%는 보상용 고정 구간. 실제 통계 백분위와 동일하지 않음'],
 ['누적 획득 별','통과','소비 별 포함·구버전 마이그레이션·103개 사례·회귀 스냅샷·동시 API 병합·재전송 멱등성','로컬 다중 작성자 충돌 F02는 별도 실패'],
 ['1회·10회 뽑기','통과','3/30별 원자적 소비·미보유 균등 추첨·중복 없음·잔여 9/10 경계·저장 실패·완성 도감','균등 후보 선택 코드 검사이며 대규모 통계적 난수 검정은 아님'],
 ['뽑기 연출·착용','통과','빠른 진행에도 전체 연출; 10개 순차 등장; 멈춤·스킵·결과 선택·즉시 착용·저장 후 재진입','브라우저 수동 검증은 1회 뽑기; 10회는 Unity 자동 검사·렌더'],
 ['40종 스킨·이펙트','통과','40종×3색 데이터·80종 이동/도착 모티프·중앙 생성·크기 비례·입자/메시 제한·수명','모든 120색을 실제 모바일에서 개별 플레이한 것은 아님'],
 ['클리어 연출','통과','1/2/3별·순차 등장·별 비행·지갑 증가·마지막 강조·음 순서·중복 보상·빠른 모드','청취 품질은 실제 스피커/이어폰 추가 확인'],
 ['배경음악·효과음','통과','48초 루프·샘플 NaN/무한값 없음·조용한 RMS·피크 헤드룸·루프 경계·설정 저장','폰의 무음 모드/오디오 포커스/블루투스는 미검증'],
 ['낙서','통과','4색·부분 지우개·전체 삭제·제스처 단위 Undo·Ctrl/Cmd+Z·Shift 예외·히스토리 상한','실제 브라우저에서 낙서 중 방향 이동과 Ctrl+Z 확인'],
 ['기본 저장','통과','2슬롯 손상 복구·임시 파일 무시·버전/좌표 검증·Undo 히스토리 복원','WebGL 비동기 영구 저장 완료 직전 강제 종료는 실기 미검증'],
 ['백업·이전 기록','통과','체크섬·손상 거절·기존 스킨/진행·반복 가져오기 별 복제 방지·소비 반영','이전 도메인에 접근 못하는 사용자의 데이터 자동 회수는 보장 못함'],
 ['동의·철회','통과','기본 미동의·거절 시 전송 없음·등록 중 철회·철회 후 전송 중단·로컬 DB 삭제','실제 법적 정책 검토/운영 보존 기간은 별도'],
 ['랭킹 API','통과+결함','동일 출처·토큰·JSON/수치 검증·스냅샷 병합·정렬·삭제 통합 검사','연결 복구 F01, 화면 응답 경합 F03, 경쟁 검증 G04'],
 ['등급 API','통과','29명 숨김/30명 표시·동점/분위수·방별 분리·철회 후 표본 축소','개별 식별자 대신 집계만 출력. 실사용자 대표성은 보장 못함'],
 ['화면·레이아웃','부분 통과','390×844 실제 플레이; 320×568 옷장; 844×390 타이틀; 320×640/390×844/844×390 상세 Unity 렌더','작은 화면/가로에서 글씨·터치 영역이 작아짐; notch·OS 글꼴 확대 미검증'],
 ['공개 배포 파일','통과','v18 active; manifest/community.js/loader/data/wasm 5개 로컬 SHA 동일·모두 HTTP200','루트 HTML은 로컬 public 디렉터리와 비교 불가. 배포 변경 없음'],
 ['공개 운영 오류','관측 없음','조회 시점 직전 24시간 errors-only 로그 최대100개 결과 0건','전체 장애 0건/100% 가용성의 증명은 아님. 장기 부하 시험 없음'],
 ['스토어 출시','보류','설정·모듈·빌드 스크립트·아이콘·네이티브 분기 정적 점검','Android APK/AAB·Xcode IPA·Play Console/TestFlight 실검증 없음'],
]
notes=[
 ['난이도','각 16단계 묶음 안에서는 최단 이동 수가 비감소. 16→17은 12→3, 32→33과 48→49는 18→3으로 새 기능 도입 때만 낮아짐. 정상적인 학습 완화로 유지 권장.'],
 ['체감 난이도 후보','55단계의 정답 깊이까지 탐색 상태는 54→417(7.72배), 21단계 15→55, 41단계 28→93. 탐색 깊이도 함께 늘어나므로 이것만으로 불공정 난이도라고 단정할 수 없음. 플레이 시간·초기화·힌트·이탈을 우선 수집할 후보.'],
 ['맵 크기','묶음별 전체 격자 면적은 1–16:63–143칸, 17–32:156–306칸, 33–48:81–168칸, 49–64:64–132칸. 전체 64단계의 맵 크기가 계속 증가하는 구조는 아니며 새 규칙 학습 때 작아짐.'],
 ['수집 경제','64×3=192개 별이 캠페인 최대 기본 획득량. 기본 1종을 제외한 39스킨 비용은 117개. 모든 스킨 수집 뒤 75개가 남음. 중복 뽑기가 없어 소진 문제는 없지만 완성 후 별의 사용처/후속 목표는 없음.'],
 ['마지막 단계','64단계 클리어 시 다음 우주로 버튼은 비활성. 메인/재도전은 남아 있어 진행 불능은 아님. 전체 여정 완료 문구·도감 완료 안내를 추가하면 마무리 품질이 좋아짐.'],
 ['퍼센트 표현','표본 30명 미만은 기본 밸런스 구간, perfect는 0.1% 고정. 상세 화면에 실제 순위가 아닌 구간 등급이라고 안내함. 마케팅에서 실제 상위 0.1% 실력으로 표현하지 않는 것이 정확함.'],
 ['UI 가독성','이미지 원본 비율과 버튼 잘림은 대표 렌더에서 정상. 320×568에서는 안내·스킨 이름이 작고 844×390에서는 세로 UI가 중앙에 축소됨. 작은 글씨 최소 크기와 터치 영역 확대, 가로 회전 안내 검토.'],
 ['첫 로딩','data+wasm 압축 합계 29,656,654 bytes(28.28 MiB). 10 Mbps 순수 전송 하한 약23.7초, 5 Mbps 약47.5초이며 실행·TLS·서버 지연 제외. 느린 모바일에서는 첫 진입 지연이 여전히 가능.'],
 ['관측 성능','로컬 재접속에서 캐시 2개 적중, 파일 준비359ms·Unity 준비 총1424ms. 데스크톱 1회 관측이며 모바일 FPS나 첫 접속 성능의 대표값이 아님. 공개 파일 GET 시간은 별도 원시 JSON에 기록.'],
 ['접근성','색상으로 친구·목적지를 구분하는 구조. 색각 차이를 보완할 모양/문양 옵션과 큰 글씨의 일관 적용을 권장. Canvas 전체가 접근성 트리에서는 이미지 1개로 보여 스크린리더 게임 조작은 현재 검증/지원되지 않음.'],
 ['문서·빌드 재현성','README의 48단계 등 설명이 현재64단계와 다름. 버전0.1.0/code1·PuzzleStudio·com.puzzlestudio.androidpuzzle은 출시 식별자로 확정 필요. 유니티 경고 CS0219 1종은 미사용 변수로 런타임 실패는 아님.'],
]
sources=[
 ['Google Play target API','https://support.google.com/googleplay/android-developer/answer/11926878?hl=en','2026-10-31 확인: 일반 신규/업데이트 앱 API36 이상. 프로젝트는 Auto(0)이므로 최종 AAB manifest로 확인해야 함.'],
 ['Android 16KB 안내','https://android-developers.googleblog.com/2025/05/prepare-play-apps-for-devices-with-16kb-page-size.html','네이티브 라이브러리의 16KB 페이지 호환성 요구. 최종 arm64 라이브러리/설치 검증 필요; 엔진 버전만으로 통과 선언하지 않음.'],
 ['Play Data safety','https://support.google.com/googleplay/android-developer/answer/10787469?hl=en','수집하지 않는 앱도 공개 정책 URL과 정확한 Data safety 작성이 필요. 내부 테스트만은 예외.'],
 ['Google 개인정보처리방침 요건','https://support.google.com/googleplay/android-developer/answer/9859455?hl=en','앱 내와 스토어의 공개 HTTPS 정책 페이지 준비.'],
 ['신규 개인 계정 테스트','https://support.google.com/googleplay/android-developer/answer/14151465?hl=en-GB','해당 개인 계정은 최소12명·연속14일 비공개 테스트 후 production access 신청. 계정 적용 여부는 미조회.'],
 ['Apple SDK 최소 요구','https://developer.apple.com/news/?id=ueeok6yw','2026-04-28 이후 iOS26 SDK 이상 필요. Mac/Xcode에서 archive 및 TestFlight 확인.'],
]
manual=[
 ['M01','실기 설치·첫 실행','Android 저/중사양 및 iPhone에서 새 설치 → 실행 → 오프라인 진입','충돌·무한 로딩 없음, 기능 범위 명확'],
 ['M02','강제 종료 복구','이동 직후/보상 직후/뽑기 소비 직후 앱 종료·재시작 각각20회','별·스킨 손실/중복 없음, 안내된 시점까지 저장'],
 ['M03','30분 내구','후반64단계·큰 친구·입자 다수·힌트 반복·옷장 전환','메모리 지속 증가 없음, 발열/터치 지연 수용 범위'],
 ['M04','화면 안전 영역','노치/홈 인디케이터/탐색 막대/키보드/OS 글자 확대','상단 버튼과 하단 초기화/되돌리기 접근 가능'],
 ['M05','네트워크 장애','진입503·응답지연·중간단절·복구·두 탭','F01/F02/F03 재현 안 됨, 최신 기록 복구'],
 ['M06','서비스 운영','동의·이름 변경·철회·백업·장치 이전·문의','실사용자가 설명 없이 복구/삭제 수행 가능'],
 ['M07','체감 밸런스','신규 사용자에게1–6,17,33,49,55 플레이 관찰','가르치지 않아도 규칙 이해; 이탈·힌트/초기화 이유 기록'],
 ['M08','릴리스 패키지','서명 AAB/IPA 업로드·스토어 자동 검사·내부 배포','API/16KB/아이콘/개인정보/연령/서명 검사 통과'],
]
data=dict(date='2026-10-31',verdict='정식 출시 보류 · 수정 후 제한 베타 재검증',siteVersion=18,issues=issues,gates=gates,coverage=coverage,notes=notes,sources=sources,manual=manual,levels=campaign['levels'],unity=unity,network=network,
 boundaries='공개 배포·서버 사용자 데이터 수정 없음. 로컬 DB는 별도 경로 사용. 기존 게임 코드 수정 없이 검사 도구와 보고서만 추가. 자동 검사와 실제 기기 검증을 구분함.')
(out/'audit-data.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
for name in ['campaign-audit.json','release-audit-results.json','release-audit-client-probes.json','release-audit-public-assets.json','release-audit-save-conflict.txt','gravity-prototype-validation.txt','release-audit-2026-10-31.log','release-audit-save-conflict.log']:
 shutil.copy2(logs/name,out/name)
for name in ['release-stage-list-390.png','release-clear-390.png','release-wardrobe-320.png','stage-detail-matte-320x640.png','stage-detail-matte-844x390.png','candy-draw-ten-results.png','skin-effects-1-20.png','skin-effects-21-40.png']:
 shutil.copy2(logs/name,out/name)
e=html.escape
def table(headers,rows):
 return '<div class="scroll"><table><thead><tr>'+''.join('<th>'+e(str(x))+'</th>' for x in headers)+'</tr></thead><tbody>'+''.join('<tr>'+''.join('<td>'+e(str(x))+'</td>' for x in row)+'</tr>' for row in rows)+'</tbody></table></div>'
parts=['''<!doctype html><html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>데굴데굴 출시 전 검증 · 2026-10-31</title><style>
*{box-sizing:border-box}body{margin:0;background:#f5f3f8;color:#2e2938;font:16px/1.7 system-ui,sans-serif}main{overflow-wrap:anywhere;max-width:1180px;margin:auto;padding:36px 24px}h1{font-size:30px;line-height:1.3}h2{font-size:23px;margin-top:42px}h3{font-size:19px}p{max-width:100ch}a{color:#624090}nav{display:flex;flex-wrap:wrap;gap:16px;border-block:1px solid #d9d3e1;padding:14px 0}.verdict{border-left:5px solid #aa5436;background:#fff4ed;padding:20px}.metrics{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:12px;margin:22px 0}.metric{padding:16px;background:white;border:1px solid #e0dbe6}.metric strong{display:block;font-size:27px}.muted,small{color:#68616f}.scroll{overflow:auto}table{border-collapse:collapse;width:100%;font-size:14px;background:white}td,th{padding:12px;border-bottom:1px solid #ded9e5;vertical-align:top;text-align:left}th{background:#eee9f3;white-space:nowrap}details{border-top:1px solid #d9d3e1;padding:15px 0}summary{cursor:pointer;font-weight:700}.tag{display:inline-block;padding:2px 9px;background:#eee4dd;margin-right:10px}.gallery{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:20px}.gallery img{max-width:100%;max-height:550px;object-fit:contain}figure{margin:0;background:white;padding:12px}figcaption{font-size:13px}button{padding:9px 16px;border:1px solid #bfb2ce;background:white;border-radius:6px;cursor:pointer}input{padding:9px;font:inherit;max-width:100%}pre{white-space:pre-wrap;background:#eee9f3;padding:16px;font-size:13px}footer{padding:30px 0;border-top:1px solid #d9d3e1;margin-top:36px}@media(max-width:600px){main{padding:24px 16px}h1{font-size:25px}td,th{padding:9px;min-width:100px}}
</style><main><p class="muted">출시 전 품질 감사 · Unity 6000.3.10f1 / 공개 웹 v18 · 2026-10-31</p><h1>데굴데굴, 출시 준비 상태</h1><div class="verdict"><strong>판정: 정식 출시 보류. 핵심 퍼즐은 안정적이지만, 저장 충돌·서버 복구와 앱 출시 준비가 남았습니다.</strong><p>확인된 기능 결함 4건(P1 2건, P2 2건)과 출시 관문 6개를 분리했습니다. 모든 테스트 통과를 완성도 100%로 해석하지 않습니다. 현재 상태에서는 수정 후 제한 베타를 권장합니다.</p></div>
<div class="metrics"><div class="metric"><strong>64 / 64</strong>초기 상태 클리어 가능</div><div class="metric"><strong>16 / 16</strong>Unity 검사 묶음 통과</div><div class="metric"><strong>8 / 8</strong>Node·HTTP 검사 통과</div><div class="metric"><strong>4건</strong>별도 재현한 결함</div></div>
<nav><a href="#issues">확정 결함</a><a href="#gates">출시 관문</a><a href="#coverage">세부 검증</a><a href="#balance">난이도·경제·성능</a><a href="#levels">64단계</a><a href="#screens">화면 증거</a><a href="#next">남은 실기 검수</a><a href="#sources">근거·로그</a></nav>
<p>검증 대상: 현재 Unity 소스, 64단계·40스킨, 공개 사이트 v18과 동일한 웹 파일. Windows Editor와 IAB 브라우저에서 수행했습니다. 공개 사용자 데이터를 생성·수정·삭제하거나 새 버전을 배포하지 않았습니다.</p>
<p>기존 회귀 검사 외에 실패 상황을 새로 주입해 결함을 찾았습니다. 핵심 규칙 검사 로그의 8,497,737 assertions에는 반복 프레임·좌표 비교가 포함되므로 서로 다른 기능 849만 개를 검사했다는 의미는 아닙니다.</p><h2 id="issues">확정 결함과 수정 완료 조건</h2>''']
for issue in issues:
 parts.append(f'<details open><summary><span class="tag">{issue["severity"]} · {issue["id"]}</span>{e(issue["title"])}</summary><p class="muted">{e(issue["status"])} · {e(issue["source"])}</p>'+''.join(f'<p><b>{title}</b> {e(issue[key])}</p>' for title,key in [('재현','repro'),('결과·영향','actual'),('수정 방향','fix'),('완료 조건','accept')])+'</details>')
parts+=['<h2 id="gates">앱 출시 관문 — 기능 버그와 별개</h2>',table(['ID','우선순위','항목','현재 확인','완료 조건'],gates),'<h2 id="coverage">기능별 세부 검증과 한계</h2>',table(['영역','판정','검증 내용','한계·주의'],coverage),'<h3>실행한 Unity 검사 16종</h3>',table(['검사','결과','실행시간(초)'],[[r['name'],'PASS' if r['passed'] else 'FAIL',round(r['seconds'],3)] for r in unity['results']]),'<p>Node 검사: test-game-loading, test-lifetime-ranking, test-ranking-refresh, test-community-consent, test-backup, test-stage-tiers 및 격리 로컬 DB를 사용한 test-community, test-stage-tiers-api. 모두 통과했습니다. 이후 작성한 client-probes와 save-conflict는 별도 결함 재현이며, 기존 회귀 검사 결과와 구분합니다.</p>','<h2 id="balance">난이도·경제·성능·사용성 판단</h2>']
for title,body in notes:parts.append(f'<h3>{e(title)}</h3><p>{e(body)}</p>')
parts+=['<h2 id="levels">64개 단계 상세</h2><p>초기 상태부터 최단 경로 BFS. 탐색 상태 수는 정답 깊이까지의 상태이며, 사람의 난이도 점수가 아닙니다. 모든 정답 길이는 등록된 route와 일치합니다.</p><label>단계·ID 검색 <input id="filter" placeholder="예: 55 또는 B07"></label>',table(['단계','ID','최단 이동','맵 크기','친구','최단 해 개수','탐색 상태'],[[x['stage'],x['room'],x['shortest'],f"{x['width']}×{x['height']}",x['friends'],x['shortestSolutionCount'],x['statesThroughAuthoredDepth']] for x in campaign['levels']]),'<h2 id="screens">실제 화면과 렌더 증거</h2><p>첫 세 장은 실제 WebGL 브라우저. 나머지는 현재 소스의 Unity 오프스크린 렌더입니다. 작은 화면 확대 보기를 위해 이미지를 누르면 원본을 엽니다.</p><div class="gallery">']
for name,caption in [('release-stage-list-390.png','F04: 목록에 이동·초기화가 남아 있음 (390×844)'),('release-clear-390.png','3회 클리어·기존1별 대비 +2 지급 확인'),('release-wardrobe-320.png','320×568: 기능은 배치되지만 작은 글씨 개선 필요'),('stage-detail-matte-320x640.png','Unity: 긴 누적 숫자·0.1%·원본 비율 유지'),('stage-detail-matte-844x390.png','Unity: 가로 화면 상세 패널'),('candy-draw-ten-results.png','Unity: 10개 결과와 즉시 착용'),('skin-effects-1-20.png','Unity: 스킨1–20 이동/도착 효과'),('skin-effects-21-40.png','Unity: 스킨21–40 이동/도착 효과')]:
 parts.append(f'<figure><a href="{name}"><img src="{name}" alt="{e(caption)}" loading="lazy"></a><figcaption>{e(caption)}</figcaption></figure>')
parts+=['</div><h2 id="next">수정 순서와 실기 검수</h2><ol><li>F01 서버 연결 복구, F02 저장 충돌부터 수정. 실패 주입 검사를 회귀 테스트로 편입.</li><li>F03 응답 경합, F04 목록 표시 수정. 작은 화면 글자·터치 크기 개선.</li><li>웹/앱 기능 범위 확정, 개인정보·지원·아이콘·서명·빌드 환경 준비.</li><li>아래 실기 체크를 통과한 빌드로 비공개 테스트. 기록 수집으로55단계 등 후보 구간 재조정.</li><li>실제 출시 AAB/IPA 자체를 재검증한 뒤 정식 출시 판단.</li></ol>',table(['ID','영역','실행 절차','합격 기준'],manual),'<h2 id="sources">정책 근거와 재현 자료</h2><p>정책은 2026-10-31 공식 문서를 조회했습니다. 심사 결과나 계정 자격을 보장하는 것은 아니며 실제 콘솔·최종 빌드 확인이 필요합니다.</p><ul>']
for title,url,body in sources:parts.append(f'<li><a href="{e(url)}">{e(title)}</a> — {e(body)}</li>')
parts+=['</ul><p>Jua 및 NanumGothic 폰트의 OFL 라이선스 파일은 프로젝트에 존재합니다. 모든 스토어 홍보 이미지와 기타 소스 자산의 권리 목록을 별도로 확정하는 것이 좋습니다.</p><h3>증거 파일</h3><ul>']
for name in ['audit-data.json','campaign-audit.json','release-audit-results.json','release-audit-client-probes.json','release-audit-save-conflict.txt','release-audit-public-assets.json','gravity-prototype-validation.txt','release-audit-2026-10-31.log']:
 parts.append(f'<li><a href="{name}">{name}</a></li>')
parts+=['</ul><footer>검수 완료 범위: 자동 기능·실제 데스크톱 WebGL 조작·렌더·소스·공개 파일 읽기 확인. 미완료 범위: 네이티브 설치 패키지와 실제 모바일 하드웨어, 실제 청취·발열·장기 성능·스토어 심사. 검증용 도구 및 보고서만 추가했고 게임 수정·배포는 하지 않았습니다.</footer></main><script>document.getElementById("filter").addEventListener("input",e=>{const q=e.target.value.trim().toLowerCase();document.querySelectorAll("#filter")[0].parentElement.nextElementSibling.querySelectorAll("tbody tr").forEach(r=>r.hidden=!r.textContent.toLowerCase().includes(q))})</script></html>']
(out/'index.html').write_text('\n'.join(parts),encoding='utf-8')
print(out)
