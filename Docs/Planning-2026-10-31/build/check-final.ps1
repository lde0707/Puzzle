$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$app=New-Object -ComObject PowerPoint.Application
$result=@()
try {
 foreach($f in Get-ChildItem -LiteralPath (Join-Path $root 'output') -Filter '*.pptx') {
  $deck=$app.Presentations.Open($f.FullName,-1,0,0)
  try {
   $media=@($deck.Slides.Item($deck.Slides.Count).Shapes | Where-Object {$_.Type -eq 16})
   $durations=@($media | ForEach-Object {$_.MediaFormat.Length})
   $result+=@{file=$f.Name;slides=$deck.Slides.Count;audio=$media.Count;durationMilliseconds=$durations}
   if($media.Count -ne 3 -or @($durations | Where-Object {$_ -le 0}).Count){throw 'Embedded audio check failed'}
  }finally{$deck.Close()}
 }
}finally{$app.Quit();[System.Runtime.InteropServices.Marshal]::ReleaseComObject($app)|Out-Null}
$result | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8 (Join-Path $PSScriptRoot 'final-native-check.json')
$result | ConvertTo-Json -Depth 5
