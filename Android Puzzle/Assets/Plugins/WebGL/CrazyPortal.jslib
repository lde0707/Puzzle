mergeInto(LibraryManager.library, {
  DegulCGEnabled: function() { return window.degulCG ? 1 : 0; },
  DegulCGGet: function(key) {
    var text=window.degulCG.read(UTF8ToString(key)) || '';
    var size=lengthBytesUTF8(text)+1, ptr=_malloc(size);stringToUTF8(text,ptr,size);return ptr;
  },
  DegulCGSet: function(key,value) { return window.degulCG.write(UTF8ToString(key),UTF8ToString(value)) ? 1 : 0; },
  DegulCGReady: function(target) { window.degulCG.ready(UTF8ToString(target)); },
  DegulCGPlaying: function(value) { window.degulCG.playing(!!value); },
  DegulCGAds: function() { return window.degulCG.canReward() ? 1 : 0; },
  DegulCGMute: function() { return window.degulCG.muted() ? 1 : 0; },
  DegulCGReward: function(target,token) { window.degulCG.reward(UTF8ToString(target),UTF8ToString(token)); }
});
