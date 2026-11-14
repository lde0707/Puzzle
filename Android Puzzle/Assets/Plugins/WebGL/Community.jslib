mergeInto(LibraryManager.library, {
  DegulSetLanguage: function(code) {
    var value=UTF8ToString(code);document.documentElement.lang=value==='zh'?'zh-CN':value;
    window.degulLanguage=value;window.dispatchEvent(new CustomEvent('degul-language',{detail:value}));
  },
  MallangStageTiers: function(target) {
    if(window.mallangCommunity) window.mallangCommunity.tiers(UTF8ToString(target));
  },
  MallangBackupOpen: function(target,json,previous) {
    if(window.mallangBackup) window.mallangBackup.open(UTF8ToString(target),UTF8ToString(json),UTF8ToString(previous));
  },
  MallangBackupResult: function(text) {
    if(window.mallangBackup) window.mallangBackup.result(UTF8ToString(text));
  },
  MallangCommunitySync: function (json) {
    if(window.mallangCommunity && window.mallangCommunity.hasConsent()) window.mallangCommunity.snapshot(UTF8ToString(json));
  },
  MallangCommunityOpen: function () {
    if(window.mallangCommunity) window.mallangCommunity.open();
  },
  MallangCommunityReady: function (target) {
    if(window.mallangCommunity) window.mallangCommunity.ready(UTF8ToString(target));
  }
});
