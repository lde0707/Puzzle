$ErrorActionPreference='Stop'
$planningRoot=Split-Path $PSScriptRoot -Parent
$ppt=New-Object -ComObject PowerPoint.Application
$results=@()
try {
 foreach($kind in @('ui','system')) {
  $deck=$ppt.Presentations.Open((Join-Path $PSScriptRoot "$kind-candidate.pptx"),0,0,0)
  try {
   foreach($sl in $deck.Slides){foreach($sh in $sl.Shapes){if($sh.HasTable){for($r=1;$r -le $sh.Table.Rows.Count;$r++){for($c=1;$c -le $sh.Table.Columns.Count;$c++){$cell=$sh.Table.Cell($r,$c).Shape;$cell.Fill.Solid();$cell.Fill.Transparency=0;$cell.Fill.ForeColor.RGB=if($r -eq 1){0xA16077}elseif($c -eq 1){0xF6E7EE}else{0xFFFFFF}}}}}}
   $slide=$deck.Slides.Item($deck.Slides.Count)
   $sounds=@('mallang-lullaby.wav','clear-sequence.wav','draw-sequence.wav')
   for($i=0;$i -lt 3;$i++) {
    $shape=$slide.Shapes.AddMediaObject2((Join-Path $planningRoot ('media/'+$sounds[$i])),0,-1,(58+405*$i)*.75,609*.75,45*.75,38*.75)
    $shape.Name='Audio example '+$sounds[$i]
    $shape.AnimationSettings.PlaySettings.PlayOnEntry=0
    $shape.AnimationSettings.PlaySettings.HideWhileNotPlaying=0
   }
   $link=$deck.Slides.Item(2).Shapes | Where-Object { $_.HasTextFrame -and $_.TextFrame.HasText -and $_.TextFrame.TextRange.Text -eq '시청각 자료 열기' } | Select-Object -First 1
   if($link){$link.ActionSettings.Item(1).Action=7;$link.ActionSettings.Item(1).Hyperlink.Address='시청각 자료.html'}
   $out=Join-Path $PSScriptRoot "$kind-media.pptx"
   $deck.SaveAs($out,24)
   $renders=Join-Path $PSScriptRoot "$kind-powerpoint"
   New-Item -ItemType Directory -Force $renders | Out-Null
   $overflows=@()
   foreach($sl in $deck.Slides){
    $sl.Export((Join-Path $renders ($sl.SlideIndex.ToString('00')+'.png')),'PNG',1280,720)
    foreach($sh in $sl.Shapes){
     if($sh.HasTable){for($r=1;$r -le $sh.Table.Rows.Count;$r++){for($c=1;$c -le $sh.Table.Columns.Count;$c++){$cell=$sh.Table.Cell($r,$c).Shape;$tf=$cell.TextFrame2;if($tf.HasText -and $tf.TextRange.BoundHeight -gt ($cell.Height-$tf.MarginTop-$tf.MarginBottom+2)){$overflows+=@{slide=$sl.SlideIndex;row=$r;col=$c;text=$tf.TextRange.Text;actual=$tf.TextRange.BoundHeight;available=$cell.Height-$tf.MarginTop-$tf.MarginBottom}}}}}
     elseif($sh.HasTextFrame -and $sh.TextFrame.HasText){$tf=$sh.TextFrame2;if($tf.TextRange.BoundHeight -gt ($sh.Height-$tf.MarginTop-$tf.MarginBottom+3)){$overflows+=@{slide=$sl.SlideIndex;shape=$sh.Name;text=$tf.TextRange.Text;actual=$tf.TextRange.BoundHeight;available=$sh.Height-$tf.MarginTop-$tf.MarginBottom}}}
    }
   }
   $results+=@{kind=$kind;slides=$deck.Slides.Count;overflows=$overflows;nativeAudio=$slide.Shapes | Where-Object {$_.Type -eq 16} | Measure-Object | Select-Object -ExpandProperty Count}
  } finally {$deck.Close()}
 }
}finally{$ppt.Quit();[System.Runtime.InteropServices.Marshal]::ReleaseComObject($ppt)|Out-Null}
$results | ConvertTo-Json -Depth 7 | Set-Content -Encoding utf8 (Join-Path $PSScriptRoot 'powerpoint-validation.json')
$results | Select-Object kind,slides,nativeAudio,@{n='overflows';e={$_.overflows.Count}} | ConvertTo-Json
