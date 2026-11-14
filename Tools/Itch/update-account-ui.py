from pathlib import Path
import json
root=Path(__file__).resolve().parents[2]
p=root/'Sites/mallang-orbit/public/community.js'
s=p.read_text(encoding='utf-8')
s=s.replace("button('참여 철회 · 서버 기록 삭제',withdraw);", "button('기록 전송 중단',pauseParticipation);")
s=s.replace("else if(prefs.withdrawPending)button('서버 기록 삭제 재시도',withdraw);", '')
p.write_text(s,encoding='utf-8')
p=root/'Sites/mallang-orbit/public/index.html'
s=p.read_text(encoding='utf-8').replace('community.js?v=5-theme-128','community.js?v=6-account-edit')
p.write_text(s,encoding='utf-8')
p=root/'Tools/Itch/ranking-i18n.js'
s=p.read_text(encoding='utf-8');start=s.index('{',s.index('const rows='));end=s.index('\n};',start)+2
rows=json.loads(s[start:end])
new={
'한글·영문 등 글자를 2개 이상 포함해 총 12자 이내로 입력해 주세요. 숫자·공백·밑줄·하이픈도 함께 쓸 수 있어요.':[
'Use at least 2 letters and no more than 12 characters in total. Numbers, spaces, underscores and hyphens are also allowed.',
'总长度不超过12个字符，且至少包含2个文字（如汉字或英文字母）。也可搭配数字、空格、下划线和连字符。',
'文字を2文字以上含む、合計12文字以内で入力してください。数字・空白・アンダースコア・ハイフンも使えます。'],
'랭킹에 보일 닉네임을 정해 주세요. 실명·연락처는 넣지 말아 주세요.':[
'Choose a nickname for the rankings. Please do not include your real name or contact details.',
'请设置排行榜昵称，不要填写真实姓名或联系方式。','ランキング用のニックネームを決めてください。本名や連絡先は入力しないでください。'],
'참여하지 않아도 모든 게임 기능을 사용할 수 있어요. 랭킹 메뉴에서 기록 전송을 중단할 수 있어요. 이미 저장된 서버 기록은 유지되며 게임 안에서 직접 삭제할 수 없어요. 브라우저 데이터를 지우면 기존 기록에 접근할 수 없어요.':[
'Joining is optional. You can stop sending records from the rankings menu. Existing server records are retained and cannot be deleted in the game. Clearing browser data removes access to your existing identity.',
'不参与也可使用全部游戏功能。可在排行榜菜单停止上传记录。已保存的服务器记录会保留，不能在游戏内直接删除。清除浏览器数据后将无法访问原有身份。',
'参加は任意です。ランキングから記録の送信を停止できます。保存済みのサーバー記録は保持され、ゲーム内では削除できません。ブラウザのデータを消すと以前のIDにアクセスできなくなります。'],
'기록 전송 중단':['Stop sending records','停止上传记录','記録の送信を停止'],
'기록 전송을 중단했어요':['Record sharing paused','已停止上传记录','記録の送信を停止しました'],
'새 플레이 기록은 전송하지 않아요. 이미 저장된 서버 기록과 기기의 진행·스킨·별은 그대로 유지돼요.':[
'New play records will not be sent. Existing server records and this device’s progress, skins and stars are kept.',
'不再上传新的游玩记录。已有服务器记录及此设备上的进度、皮肤和星星均保留。',
'新しいプレイ記録は送信しません。保存済みのサーバー記録と端末の進行状況・スキン・星は保持されます。']}
for key,values in new.items():rows[key]=dict(zip(['en','zh','ja'],values))
p.write_text(s[:start]+json.dumps(rows,ensure_ascii=False,indent=2)+s[end:],encoding='utf-8')
# Adjust regression expectations to the new non-destructive pause contract.
p=root/'Sites/mallang-orbit/scripts/test-community-consent.mjs'
s=p.read_text().replace('참여 철회 · 서버 기록 삭제','기록 전송 중단').replace("assert.equal(calls.filter(c=>c.url.endsWith('/withdraw')).length,1);","assert.equal(calls.filter(c=>c.url.endsWith('/withdraw')).length,0);\nassert.ok(JSON.parse(storage.get('mallang-community-v1')).id,'pause retains identity');")
p.write_text(s.replace('withdrawal waits for enrollment','pause retains identity through enrollment'),encoding='utf-8')
p=root/'Sites/mallang-orbit/scripts/test-itch-ranking.mjs'
s=p.read_text().replace("await call('withdraw',{method:'POST',body:ids[1]});", "assert.equal((await call('withdraw',{method:'POST',body:ids[1]})).status,403);")
s=s.replace("rows.map(r=>r.label),['web player']);","rows.map(r=>r.label),['web player','itch player']);")
p.write_text(s.replace('withdrawal isolation','player deletion blocked'),encoding='utf-8')
p=root/'Tools/Itch/test-ranking-edition.mjs'
s=p.read_text().replace('2026-11-04-v1.2.0-ranking','2026-11-04-v1.2.1').replace('Withdraw and delete server record','Stop sending records')
s=s.replace("assert.equal(api.hasConsent(),false);\nassert.equal(stored.get('unrelated-game-save')", "assert.equal(api.hasConsent(),false);\nassert.equal(JSON.parse(stored.get('degul-itch-5285502-community-v1')).id,id);\nassert.ok(!calls.some(r=>r.url.endsWith('/withdraw')));\nassert.equal(stored.get('unrelated-game-save')")
p.write_text(s,encoding='utf-8')
print('Updated account UI, translations and regression expectations')
