import fs from 'node:fs/promises';import path from 'node:path';import {pathToFileURL} from 'node:url';
const W=path.resolve('..'),S='C:/Users/doun0/.codex/plugins/cache/openai-primary-runtime/presentations/26.1017.22136/skills/presentations';
const {finalizePresentation}=await import(pathToFileURL(path.join(S,'container_tools/artifact_tool_utils.mjs')).href);
await fs.mkdir(path.join(W,'output'),{recursive:true});
for(const type of ['ui','system']){
 const meta=JSON.parse(await fs.readFile(`${type}-meta.json`,'utf8'));
 const result=await finalizePresentation({workspaceDir:W,candidatePath:path.join(W,`build/${type}-ready.pptx`),finalPath:path.join(W,'output',type==='ui'?'말랑궤도 UI 기획서.pptx':'말랑궤도 System 기획서.pptx'),pythonExecutable:'C:/Users/doun0/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe',integrityValidatorPath:path.join(S,'container_tools/inspect_presentation_package_integrity.py'),layoutValidatorPath:path.join(S,'container_tools/inspect_presentation_layout_geometry.py'),layoutArgs:['--expected-slide-size-emu','12192000,6858000','--validate-bullet-geometry','--validate-heading-fit',...meta.owners.flatMap(n=>['--require-native-table-slide',String(n)])],requiredNativeTableOwnerSlides:meta.owners,fontPolicy:{basis:'design',families:['Malgun Gothic','맑은 고딕']},verifyArtifactToolImport:true,receiptPath:path.join(W,`build/${type}-final-validation.json`)});
 console.log('FINAL',type,JSON.stringify(result));
}


