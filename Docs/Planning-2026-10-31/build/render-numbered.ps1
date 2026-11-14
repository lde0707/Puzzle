$ErrorActionPreference='Stop'
$app=New-Object -ComObject PowerPoint.Application
$deck=$null
try {
 $deck=$app.Presentations.Open((Join-Path $PSScriptRoot 'ui-numbered-candidate.pptx'),-1,0,0)
 $folder=Join-Path $PSScriptRoot 'ui-numbered-renders'
 New-Item -ItemType Directory -Force -Path $folder | Out-Null
 $overflows=@();$counts=@()
 foreach($sl in $deck.Slides){
  $sl.Export((Join-Path $folder ($sl.SlideIndex.ToString('00')+'.png')),'PNG',1280,720)
  $counts+=@{slide=$sl.SlideIndex;badges=@($sl.Shapes | Where-Object {$_.Name -match '^UI_callout_\d+_badge_'}).Count}
  foreach($sh in $sl.Shapes){
   if($sh.HasTable){for($r=1;$r -le $sh.Table.Rows.Count;$r++){for($c=1;$c -le $sh.Table.Columns.Count;$c++){$cell=$sh.Table.Cell($r,$c).Shape;$tf=$cell.TextFrame2;if($tf.HasText -and $tf.TextRange.BoundHeight -gt ($cell.Height-$tf.MarginTop-$tf.MarginBottom+2)){$overflows+=@{slide=$sl.SlideIndex;row=$r;col=$c;text=$tf.TextRange.Text}}}}}
   elseif($sh.HasTextFrame -and $sh.TextFrame.HasText){$tf=$sh.TextFrame2;if($tf.TextRange.BoundHeight -gt ($sh.Height-$tf.MarginTop-$tf.MarginBottom+3)){$overflows+=@{slide=$sl.SlideIndex;shape=$sh.Name;text=$tf.TextRange.Text}}}
  }
 }
 $report=@{slides=$deck.Slides.Count;overflows=$overflows;counts=$counts;audio=@($deck.Slides.Item(31).Shapes|Where-Object {$_.Type -eq 16}).Count}
 $report|ConvertTo-Json -Depth 5|Set-Content -Encoding utf8 (Join-Path $PSScriptRoot 'ui-numbered-native-validation.json')
 $report|ConvertTo-Json -Depth 5
}finally{if($deck){$deck.Close()};$app.Quit();[System.Runtime.InteropServices.Marshal]::ReleaseComObject($app)|Out-Null}
