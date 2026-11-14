from pathlib import Path
import numpy as np, wave, json
rate=22050;beat=.75;bars=16;duration=bars*4*beat;n=int(rate*duration)
a=np.zeros(n,dtype=np.float64)
def note(midi,start,length,gain,voice='bell'):
 t=np.arange(int(length*rate))/rate;f=440*2**((midi-69)/12)
 attack=1-np.exp(-t/(.018 if voice=='bell' else .065))
 env=attack*np.exp(-t/(.45 if voice=='bell' else .85))*np.minimum(1,(length-t)/.15)
 tone=np.sin(2*np.pi*f*t)+(.16 if voice=='bell' else .08)*np.sin(2*np.pi*f*2*t)*np.exp(-t*5)+.035*np.sin(2*np.pi*f*3*t)*np.exp(-t*8)
 samples=tone*env*gain
 indices=(np.arange(len(t))+int(start*rate))%n
 np.add.at(a,indices,samples)
 # Gentle, quiet room echoes wrap at the loop seam.
 for lag,g in [(0.16,.075),(.31,.035)]: np.add.at(a,(indices+int(lag*rate))%n,samples*g)
chords=[(48,[60,64,67,71]),(45,[57,60,64,67]),(41,[57,60,64,65]),(43,[55,59,62,64])]
melodies=[[(0,76),(1.5,79),(2.5,81)],[(.5,79),(2,76),(3,72)],[(0,74),(1.5,76),(3,79)],[(.5,74),(2,71)],[(0,76),(1,79),(2.5,84)],[(.5,81),(2,79),(3,76)],[(0,74),(2,76),(3,72)],[(.5,71),(2,74),(3,72)]]
for bar in range(bars):
 start=bar*4*beat;root,chord=chords[bar%4]
 note(root,start,2.6,.12,'warm')
 for j,pitch in enumerate(chord): note(pitch,start+(j*.5+.25)*beat,2.2,.075,'warm')
 for off,pitch in melodies[bar%8]: note(pitch,start+off*beat,1.7,.13 if bar<8 else .115)
# Keep headroom; playback gain is deliberately very low.
a*=.43/max(abs(a)); pcm=np.round(np.clip(a,-1,1)*32767).astype('<i2')
p=Path('Android Puzzle/Assets/Resources/GravityPuzzle/Audio/mallang-lullaby.wav');p.parent.mkdir(parents=True,exist_ok=True)
with wave.open(str(p),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(pcm.tobytes())
print(json.dumps({'seconds':duration,'peak':float(max(abs(a))),'rms':float(np.sqrt(np.mean(a*a))),'seamStep':float(abs(a[0]-a[-1]))}))
