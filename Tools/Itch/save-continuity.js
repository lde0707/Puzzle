// Unity uses MD5(document URL directory) under /idbfs. itch upload IDs can change.
// Only our known published directory and this game's registered directories are read.
export function md5(text){
 const input=new TextEncoder().encode(text),n=((input.length+8>>>6)+1)*64,bytes=new Uint8Array(n);
 bytes.set(input);bytes[input.length]=128;const view=new DataView(bytes.buffer);
 view.setUint32(n-8,input.length*8,true);view.setUint32(n-4,Math.floor(input.length/0x20000000),true);
 let a0=0x67452301,b0=0xefcdab89,c0=0x98badcfe,d0=0x10325476;
 const shifts=[7,12,17,22,5,9,14,20,4,11,16,23,6,10,15,21];
 for(let offset=0;offset<n;offset+=64){let a=a0,b=b0,c=c0,d=d0;
  for(let i=0;i<64;i++){
   let f,g;if(i<16){f=b&c|~b&d;g=i;}else if(i<32){f=d&b|~d&c;g=(5*i+1)%16;}else if(i<48){f=b^c^d;g=(3*i+5)%16;}else{f=c^(b|~d);g=7*i%16;}
   const shift=shifts[(i>>>4)*4+i%4],x=(a+f+Math.floor(Math.abs(Math.sin(i+1))*4294967296)+view.getUint32(offset+g*4,true))|0;
   const next=(b+((x<<shift)|(x>>>(32-shift))))|0;a=d;d=c;c=b;b=next;
  }a0=a0+a|0;b0=b0+b|0;c0=c0+c|0;d0=d0+d|0;
 }
 return [a0,b0,c0,d0].map(v=>[0,8,16,24].map(s=>(v>>>s&255).toString(16).padStart(2,'0')).join('')).join('');
}
export function saveRoot(url){const clean=url.split('?')[0].split('#')[0];return '/idbfs/'+md5(clean.slice(0,clean.lastIndexOf('/')));}
export function envelopeSequence(record){
 try{
  if(!record?.contents||record.contents.byteLength>1048576)return -1;
  const e=JSON.parse(new TextDecoder().decode(record.contents));let checksum=2166136261;
  if(e.version!==1||!Number.isSafeInteger(e.sequence)||e.sequence<1||typeof e.payload!=='string')return -1;
  for(let i=0;i<e.payload.length;i++)checksum=Math.imul(checksum^e.payload.charCodeAt(i),16777619)>>>0;
  if(checksum.toString(16).padStart(8,'0')!==e.checksum)return -1;
  const p=JSON.parse(e.payload);
  return p&&typeof p.dataVersion==='string'&&Array.isArray(p.completed)&&p.collection&&p.current?e.sequence:-1;
 }catch{return -1;}
}
const REGISTRY='degul-itch-5285502-save-roots-v1';
const LEGACY='/idbfs/070851ede8382077bc86fa7b43d11215'; // verified live /html/20227853
const files=['/MellowGravity/gravity-progress-0.json','/MellowGravity/gravity-progress-1.json'];
export async function preserveItchSave({url=location.href,storage=localStorage,idb=indexedDB,legacy=[LEGACY]}={}){
 const target=saveRoot(url);let known=[];
 try{known=JSON.parse(storage.getItem(REGISTRY)||'[]');}catch{}
 if(!Array.isArray(known))known=[];
 const sources=[...new Set([...legacy,...known].filter(s=>typeof s==='string'&&/^\/idbfs\/[a-f0-9]{32}$/.test(s)))].filter(s=>s!==target).slice(-20);
 const register=()=>{try{storage.setItem(REGISTRY,JSON.stringify([...new Set([...sources,target])].slice(-20)));}catch{}};
 const db=await new Promise((resolve,reject)=>{
  const r=idb.open('/idbfs',21);let blocked=false;
  r.onupgradeneeded=()=>{if(!r.result.objectStoreNames.contains('FILE_DATA'))r.result.createObjectStore('FILE_DATA').createIndex('timestamp','timestamp',{unique:false});};
  r.onsuccess=()=>{if(blocked)r.result.close();else resolve(r.result);};r.onerror=()=>reject(Error('Could not open your saved progress. Please keep browser data and retry.'));
  r.onblocked=()=>{blocked=true;reject(Error('Close other game tabs and retry to keep your saved progress.'));};
 });
 try{
  const result=await new Promise((resolve,reject)=>{
   const tx=db.transaction('FILE_DATA','readwrite'),store=tx.objectStore('FILE_DATA'),values=new Map();
   const paths=[...new Set([target,...sources].flatMap(root=>[root,root+'/MellowGravity',...files.map(f=>root+f)]))];
   let remaining=paths.length,outcome='no-previous-save';
   tx.onerror=()=>reject(Error('Could not preserve saved progress. No original saves were removed. Please retry.'));
   tx.onabort=()=>reject(Error('Save transfer was interrupted. Your original saves are kept. Please retry.'));
   tx.oncomplete=()=>resolve({status:outcome,target});
   for(const path of paths){const r=store.get(path);r.onsuccess=()=>{values.set(path,r.result);if(--remaining===0)copy();};}
   function copy(){
    // Never overwrite an existing destination, even a damaged one. Unity keeps its two-slot recovery.
    if(files.some(f=>values.get(target+f))){outcome='existing-save-kept';return;}
    const ranked=sources.map(root=>({root,sequence:Math.max(...files.map(f=>envelopeSequence(values.get(root+f))))})).sort((a,b)=>b.sequence-a.sequence);
    if(!ranked.length||ranked[0].sequence<0){
     if(sources.some(root=>files.some(f=>values.get(root+f))))tx.abort();
     return;
    }
    const source=ranked[0].root;
    for(const suffix of ['', '/MellowGravity'])if(!values.get(target+suffix))store.put(values.get(source+suffix)||{timestamp:new Date(),mode:16895},target+suffix);
    for(const suffix of files){const record=values.get(source+suffix);if(envelopeSequence(record)>0)store.put(record,target+suffix);}
    outcome='previous-save-copied';
   }
  });register();return result;
 }finally{db.close();}
}
