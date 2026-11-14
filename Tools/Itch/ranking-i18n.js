(() => {
const rows={
  "랭킹과 기록": {
    "en": "Rankings and records",
    "zh": "排行榜与记录",
    "ja": "ランキングと記録"
  },
  "게임으로 돌아가기": {
    "en": "Back to game",
    "zh": "返回游戏",
    "ja": "ゲームに戻る"
  },
  "연결하지 못했어요. 게임 기록은 기기에 계속 저장돼요. 나중에 다시 참여해 주세요.": {
    "en": "Could not connect. Progress is still saved on this device. Please try again later.",
    "zh": "连接失败，游戏进度仍保存在此设备上，请稍后重试。",
    "ja": "接続できません。進行状況は端末に保存されています。後で再試行してください。"
  },
  "이름은 2~12자의 한글·영문·숫자·공백·밑줄·하이픈으로 입력해 주세요.": {
    "en": "Use 2–12 letters, numbers, spaces, underscores or hyphens.",
    "zh": "请输入2至12个文字、数字、空格、下划线或连字符。",
    "ja": "2〜12文字の文字・数字・空白・アンダースコア・ハイフンを使ってください。"
  },
  "닉네임": {
    "en": "Nickname",
    "zh": "昵称",
    "ja": "ニックネーム"
  },
  "예: 말랑탐험가": {
    "en": "Example: StarFriend",
    "zh": "例如：星星朋友",
    "ja": "例：星のともだち"
  },
  "기기 저장을 허용해야 참여할 수 있어요.": {
    "en": "Allow browser storage to join the ranking.",
    "zh": "请允许浏览器存储后参与排名。",
    "ja": "ランキング参加にはブラウザの保存を許可してください。"
  },
  "참여는 됐지만 이름을 저장하지 못했어요. 다시 시도해 주세요.": {
    "en": "Joined, but could not save your name. Please try again.",
    "zh": "已加入，但昵称保存失败。请重试。",
    "ja": "参加できましたが名前を保存できません。再試行してください。"
  },
  "별친구 이름을 정해 주세요": {
    "en": "Choose your star-friend name",
    "zh": "设置星友昵称",
    "ja": "星のともだちの名前を決めよう"
  },
  "이제 랭킹에 보일 이름을 직접 정할 수 있어요. 지금까지 모은 별·이동 기록은 그대로 유지돼요.": {
    "en": "Choose a display name. Your earned stars and movement records are kept.",
    "zh": "设置显示昵称。已获得的星星和移动记录会保留。",
    "ja": "表示名を設定できます。獲得した星と移動記録は維持されます。"
  },
  "실명·연락처는 넣지 말아 주세요. 한글·영문·숫자·공백·밑줄·하이픈으로 2~12자, 같은 이름도 쓸 수 있어요.": {
    "en": "Do not use your real name or contact details. Use 2–12 letters, numbers, spaces, underscores or hyphens. Names do not need to be unique.",
    "zh": "请勿填写真实姓名或联系方式。使用2至12个文字、数字、空格、下划线或连字符，昵称可以重复。",
    "ja": "本名や連絡先は入力しないでください。2〜12文字の文字・数字・空白・アンダースコア・ハイフンを使えます。同じ名前も使えます。"
  },
  "이 이름으로 시작": {
    "en": "Start with this name",
    "zh": "使用此昵称开始",
    "ja": "この名前で始める"
  },
  "이름을 저장하지 못했어요. 연결을 확인하고 다시 시도해 주세요.": {
    "en": "Could not save the name. Check your connection and retry.",
    "zh": "昵称保存失败，请检查连接并重试。",
    "ja": "名前を保存できません。接続を確認して再試行してください。"
  },
  "다음에 정하기": {
    "en": "Choose later",
    "zh": "以后再设置",
    "ja": "後で決める"
  },
  "반가워요! 이름을 정하고 시작해요": {
    "en": "Welcome! Choose a name",
    "zh": "欢迎！设置昵称",
    "ja": "ようこそ！名前を決めよう"
  },
  "우리의 기록을 함께 모을까요?": {
    "en": "Share your puzzle records?",
    "zh": "要分享游戏记录吗？",
    "ja": "パズルの記録を共有しますか？"
  },
  "랭킹에 보일 닉네임을 정해 주세요. 실명·연락처는 넣지 말고, 한글·영문·숫자·공백·밑줄·하이픈으로 2~12자까지 쓸 수 있어요.": {
    "en": "Choose a public nickname, not your real name or contact details. Use 2–12 letters, numbers, spaces, underscores or hyphens.",
    "zh": "请输入公开昵称，不要使用真实姓名或联系方式。支持2至12个文字、数字、空格、下划线或连字符。",
    "ja": "公開ニックネームを決めてください。本名や連絡先は使わず、2〜12文字の文字・数字・空白・アンダースコア・ハイフンを使ってください。"
  },
  "참여하면 이 기기에 저장된 별 기록과 단계별 이동·초기화·되돌리기·힌트 사용·방문·클리어 횟수, 이후 플레이 기록을 서버에 저장해 난이도 조정과 랭킹에 활용해요. 실명이나 이메일은 필요하지 않아요.": {
    "en": "If you join, this device's saved star records and per-stage moves, restarts, undos, hints, visits and clears, plus future play records, are sent to our server for rankings and balance analysis. No real name or email is needed.",
    "zh": "参与后，此设备已保存的星星记录、各关卡移动、重置、撤销、提示、访问与通关次数及今后的记录将发送至服务器，用于排名和难度分析。无需真实姓名或邮箱。",
    "ja": "参加すると、この端末の星の記録とステージごとの移動・リセット・戻す・ヒント・訪問・クリア回数、および今後の記録をランキングと難易度分析のためサーバーに送信します。本名やメールは不要です。"
  },
  "랭킹에는 정한 닉네임, 누적 획득 별, 총 이동, 초기화 횟수가 공개돼요. 기기·브라우저마다 별도 기록이에요. 이름은 랭킹 메뉴에서 언제든 바꿀 수 있어요.": {
    "en": "Your nickname, lifetime earned stars, total moves and restart count are public. Each browser/device has a separate record. You can rename yourself in the ranking menu.",
    "zh": "昵称、累计获得的星星、总移动次数和重置次数会公开。不同浏览器或设备有独立记录，可在排行榜中更改昵称。",
    "ja": "ニックネーム・累計獲得星・総移動・リセット回数を公開します。ブラウザと端末ごとに別の記録です。ランキングから名前を変更できます。"
  },
  "참여하지 않아도 모든 게임 기능을 사용할 수 있어요. 랭킹 메뉴에서 참여를 철회하면 서버 기록이 삭제됩니다. 브라우저 데이터를 지우면 기존 기록에 접근할 수 없어요.": {
    "en": "Participation is optional. Withdrawing in the ranking menu deletes your server record. Clearing browser data can prevent access to your existing record.",
    "zh": "参与完全自愿，不参与也能使用游戏功能。退出参与会删除服务器记录。清除浏览器数据可能导致无法访问原有记录。",
    "ja": "参加は任意です。不参加でもゲームを遊べます。参加を取り消すとサーバーの記録を削除します。ブラウザのデータを消すと既存の記録にアクセスできなくなる場合があります。"
  },
  "이 이름으로 참여하고 시작": {
    "en": "Join and start",
    "zh": "参与并开始",
    "ja": "参加して始める"
  },
  "이 이름으로 참여": {
    "en": "Join with this name",
    "zh": "使用此昵称参与",
    "ja": "この名前で参加"
  },
  "랭킹 참여 없이 시작": {
    "en": "Play without joining",
    "zh": "不参与，开始游戏",
    "ja": "参加せずに遊ぶ"
  },
  "참여하지 않고 플레이": {
    "en": "Play without sharing",
    "zh": "不分享记录，继续游戏",
    "ja": "記録を共有せずに遊ぶ"
  },
  "랭킹만 보기": {
    "en": "View rankings only",
    "zh": "仅查看排行榜",
    "ja": "ランキングだけ見る"
  },
  "참여를 철회했어요": {
    "en": "Participation withdrawn",
    "zh": "已退出参与",
    "ja": "参加を取り消しました"
  },
  "서버의 익명 기록을 삭제했어요. 기기의 게임 진행과 스킨은 그대로예요.": {
    "en": "Your server record was deleted. Local progress and skins are kept.",
    "zh": "服务器记录已删除，本地进度与皮肤保留。",
    "ja": "サーバーの記録を削除しました。端末の進行状況とスキンは維持されます。"
  },
  "전송은 중단했어요": {
    "en": "Sharing stopped",
    "zh": "已停止分享",
    "ja": "送信を停止しました"
  },
  "연결이 끊겨 서버 기록 삭제가 아직 끝나지 않았어요. 다시 연결되면 아래 버튼으로 삭제해 주세요.": {
    "en": "Offline: server deletion is still pending. Reconnect and use the button below to retry.",
    "zh": "离线状态，服务器记录尚未删除。重新连接后请点击下方按钮重试。",
    "ja": "オフラインのためサーバーの削除は未完了です。再接続して下のボタンから再試行してください。"
  },
  "서버 기록 삭제 재시도": {
    "en": "Retry server deletion",
    "zh": "重试删除服务器记录",
    "ja": "サーバー削除を再試行"
  },
  "별친구 랭킹": {
    "en": "Star-friend rankings",
    "zh": "星友排行榜",
    "ja": "星のともだちランキング"
  },
  "itch.io와 공개 웹버전의 통합 랭킹이에요.": {
    "en": "Shared rankings for itch.io and the public web edition.",
    "zh": "itch.io与公开网页版共享排行榜。",
    "ja": "itch.ioと公開Web版の共通ランキングです。"
  },
  "누적 획득 별 ↓ · 동점이면 총 이동 ↑ · 초기화 ↑": {
    "en": "Earned stars: highest first. Ties: fewer total moves, then fewer restarts.",
    "zh": "累计星星从高到低。同分时，移动次数少者优先，其次比较重置次数。",
    "ja": "獲得星の多い順。同点は総移動、次にリセットの少ない順です。"
  },
  "뽑기에 사용한 별도 누적 획득량에 포함돼요. 같은 단계에서 새로 추가된 별만 집계해요.": {
    "en": "Stars spent on skins still count. Only newly earned stage-star rewards are added.",
    "zh": "用于抽取皮肤的星星也计入累计数量。同一关卡仅计算新获得的星星奖励。",
    "ja": "スキンに使った星も含みます。同じステージでは新たに獲得した星の報酬だけを加算します。"
  },
  "익명 기기별 테스트 랭킹 · 브라우저 기록에 기반하며 경쟁용 부정행위 검증은 포함하지 않아요.": {
    "en": "Casual device-based rankings use browser saves, without competitive anti-cheat verification.",
    "zh": "休闲排行榜基于浏览器存档，未包含竞技级反作弊验证。",
    "ja": "ブラウザの記録に基づくカジュアルランキングです。競技用の不正対策検証は含みません。"
  },
  "나의 이름: ": {
    "en": "My name: ",
    "zh": "我的昵称：",
    "ja": "自分の名前："
  },
  "별친구-": {
    "en": "StarFriend-",
    "zh": "星友-",
    "ja": "星友-"
  },
  "이름 바꾸기": {
    "en": "Change name",
    "zh": "更改昵称",
    "ja": "名前を変更"
  },
  "참여 철회 · 서버 기록 삭제": {
    "en": "Withdraw and delete server record",
    "zh": "退出并删除服务器记录",
    "ja": "参加取消・サーバー記録削除"
  },
  "이름 정하고 랭킹 참여하기": {
    "en": "Choose a name and join",
    "zh": "设置昵称并参与",
    "ja": "名前を決めて参加"
  },
  "단계별 밸런스 통계": {
    "en": "Stage balance statistics",
    "zh": "关卡平衡统计",
    "ja": "ステージ別バランス統計"
  },
  "최신 기록을 전송하지 못해 마지막으로 저장된 랭킹을 표시해요.": {
    "en": "Could not upload the latest record. Showing the last saved ranking.",
    "zh": "最新记录上传失败，显示上次保存的排行榜。",
    "ja": "最新記録を送信できません。最後に保存されたランキングを表示します。"
  },
  "아직 등록된 별 기록이 없어요. 참여 후 단계를 플레이하면 기록이 올라가요.": {
    "en": "No star records yet. Join and play a stage to appear here.",
    "zh": "暂无星星记录。参与并游玩关卡后即可显示。",
    "ja": "星の記録はまだありません。参加してステージを遊ぶと表示されます。"
  },
  "별친구": {
    "en": "Player",
    "zh": "玩家",
    "ja": "プレイヤー"
  },
  "누적 획득 별": {
    "en": "Lifetime stars",
    "zh": "累计获得星星",
    "ja": "累計獲得星"
  },
  "총 이동": {
    "en": "Total moves",
    "zh": "总移动次数",
    "ja": "総移動"
  },
  "초기화": {
    "en": "Restarts",
    "zh": "重置次数",
    "ja": "リセット"
  },
  "랭킹을 불러오지 못했어요. 잠시 후 다시 시도해 주세요.": {
    "en": "Could not load rankings. Please retry later.",
    "zh": "无法加载排行榜，请稍后重试。",
    "ja": "ランキングを読み込めません。後で再試行してください。"
  },
  "나의 별친구 이름": {
    "en": "My star-friend name",
    "zh": "我的星友昵称",
    "ja": "自分の星友の名前"
  },
  "랭킹에 공개되는 닉네임이에요. 실명·연락처는 넣지 말아 주세요. 한글·영문·숫자·공백·밑줄·하이픈으로 2~12자까지 쓸 수 있어요. 같은 이름도 사용할 수 있습니다.": {
    "en": "This name is public. Do not enter real names or contact details. Use 2–12 letters, numbers, spaces, underscores or hyphens. Duplicate names are allowed.",
    "zh": "此昵称会公开，请勿填写真实姓名或联系方式。支持2至12个文字、数字、空格、下划线或连字符，可以重名。",
    "ja": "公開される名前です。本名や連絡先は使わないでください。2〜12文字の文字・数字・空白・アンダースコア・ハイフンを使えます。同名も可能です。"
  },
  "이 이름으로 저장": {
    "en": "Save this name",
    "zh": "保存昵称",
    "ja": "この名前で保存"
  },
  "먼저 익명 기록 참여를 연결해 주세요.": {
    "en": "Join the ranking first.",
    "zh": "请先参与排行榜。",
    "ja": "先にランキングに参加してください。"
  },
  "이름은 2~12자의 한글·영문·숫자·공백·밑줄·하이픈으로 입력해 주세요. 연결 상태도 확인해 주세요.": {
    "en": "Use 2–12 letters, numbers, spaces, underscores or hyphens. Also check your connection.",
    "zh": "请使用2至12个文字、数字、空格、下划线或连字符，并检查网络连接。",
    "ja": "2〜12文字の文字・数字・空白・アンダースコア・ハイフンを使い、接続も確認してください。"
  },
  "취소": {
    "en": "Cancel",
    "zh": "取消",
    "ja": "キャンセル"
  },
  "어느 길에서 머물렀을까요?": {
    "en": "Where do we get stuck?",
    "zh": "我们在哪些关卡受阻？",
    "ja": "どこで立ち止まる？"
  },
  "단계별 참여자가 5명 이상일 때 집계를 표시해요. 재시도·힌트가 많은 구간부터 난이도를 검토할 수 있어요.": {
    "en": "Statistics appear after at least five participants per stage. Restarts and hints help us review difficulty.",
    "zh": "每关至少5人参与后显示统计，重置和提示次数用于评估难度。",
    "ja": "ステージごとに5人以上で統計を表示します。リセットとヒントを参考に難易度を見直します。"
  },
  "랭킹으로": {
    "en": "Back to rankings",
    "zh": "返回排行榜",
    "ja": "ランキングに戻る"
  },
  "아직 분석에 필요한 표본이 부족해요. 실제 기록이 모이면 여기에 표시됩니다.": {
    "en": "Not enough records yet. Statistics will appear as people play.",
    "zh": "记录不足，随着玩家游玩将显示统计。",
    "ja": "記録がまだ不足しています。プレイ記録が集まると表示します。"
  },
  "단계": {
    "en": "Stage",
    "zh": "关卡",
    "ja": "ステージ"
  },
  "참여": {
    "en": "Players",
    "zh": "参与人数",
    "ja": "参加人数"
  },
  "이동/인": {
    "en": "Moves/player",
    "zh": "人均移动",
    "ja": "移動/人"
  },
  "초기화/인": {
    "en": "Restarts/player",
    "zh": "人均重置",
    "ja": "リセット/人"
  },
  "힌트/인": {
    "en": "Hints/player",
    "zh": "人均提示",
    "ja": "ヒント/人"
  },
  "클리어": {
    "en": "Clears",
    "zh": "通关次数",
    "ja": "クリア"
  },
  "통계를 불러오지 못했어요.": {
    "en": "Could not load statistics.",
    "zh": "无法加载统计。",
    "ja": "統計を読み込めません。"
  },
  "한글·영문 등 글자를 2개 이상 포함해 총 12자 이내로 입력해 주세요. 숫자·공백·밑줄·하이픈도 함께 쓸 수 있어요.": {
    "en": "Use at least 2 letters and no more than 12 characters in total. Numbers, spaces, underscores and hyphens are also allowed.",
    "zh": "总长度不超过12个字符，且至少包含2个文字（如汉字或英文字母）。也可搭配数字、空格、下划线和连字符。",
    "ja": "文字を2文字以上含む、合計12文字以内で入力してください。数字・空白・アンダースコア・ハイフンも使えます。"
  },
  "랭킹에 보일 닉네임을 정해 주세요. 실명·연락처는 넣지 말아 주세요.": {
    "en": "Choose a nickname for the rankings. Please do not include your real name or contact details.",
    "zh": "请设置排行榜昵称，不要填写真实姓名或联系方式。",
    "ja": "ランキング用のニックネームを決めてください。本名や連絡先は入力しないでください。"
  },
  "참여하지 않아도 모든 게임 기능을 사용할 수 있어요. 랭킹 메뉴에서 기록 전송을 중단할 수 있어요. 이미 저장된 서버 기록은 유지되며 게임 안에서 직접 삭제할 수 없어요. 브라우저 데이터를 지우면 기존 기록에 접근할 수 없어요.": {
    "en": "Joining is optional. You can stop sending records from the rankings menu. Existing server records are retained and cannot be deleted in the game. Clearing browser data removes access to your existing identity.",
    "zh": "不参与也可使用全部游戏功能。可在排行榜菜单停止上传记录。已保存的服务器记录会保留，不能在游戏内直接删除。清除浏览器数据后将无法访问原有身份。",
    "ja": "参加は任意です。ランキングから記録の送信を停止できます。保存済みのサーバー記録は保持され、ゲーム内では削除できません。ブラウザのデータを消すと以前のIDにアクセスできなくなります。"
  },
  "기록 전송 중단": {
    "en": "Stop sending records",
    "zh": "停止上传记录",
    "ja": "記録の送信を停止"
  },
  "기록 전송을 중단했어요": {
    "en": "Record sharing paused",
    "zh": "已停止上传记录",
    "ja": "記録の送信を停止しました"
  },
  "새 플레이 기록은 전송하지 않아요. 이미 저장된 서버 기록과 기기의 진행·스킨·별은 그대로 유지돼요.": {
    "en": "New play records will not be sent. Existing server records and this device’s progress, skins and stars are kept.",
    "zh": "不再上传新的游玩记录。已有服务器记录及此设备上的进度、皮肤和星星均保留。",
    "ja": "新しいプレイ記録は送信しません。保存済みのサーバー記録と端末の進行状況・スキン・星は保持されます。"
  }
};
window.degulRankingT=value=>{const lang=window.degulLanguage||'en';if(lang==='ko'||typeof value!=='string')return value;if(rows[value])return rows[value][lang]||rows[value].en;for(const key of ['나의 이름: ','별친구-'])if(value.startsWith(key))return (rows[key][lang]||rows[key].en)+value.slice(key.length);return value;};
})();
