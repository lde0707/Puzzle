import fs from 'node:fs/promises';
import {PresentationFile,FileBlob} from '@oai/artifact-tool';
const p=await PresentationFile.importPptx(await FileBlob.load('C:/Users/doun0/OneDrive/바탕 화면/File/GitHub/Roguelike-Defense/Rushitadel UI기획서.pptx'));
await fs.mkdir('reference-renders',{recursive:true});
for(let i=0;i<p.slides.items.length;i++){await fs.writeFile(`reference-renders/${i+1}.png`,new Uint8Array(await(await p.export({slide:p.slides.items[i],format:'png',scale:1})).arrayBuffer()));console.log('reference',i+1);}
