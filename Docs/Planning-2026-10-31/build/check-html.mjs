import {chromium} from 'playwright';
import path from 'node:path';
import {pathToFileURL} from 'node:url';
import fs from 'node:fs/promises';
const browser=await chromium.launch({channel:'msedge',headless:true});
try {
 const page=await browser.newPage({viewport:{width:1280,height:900}});
 await page.goto(pathToFileURL(path.resolve('../output/시청각 자료.html')).href);
 await page.locator('img').evaluateAll(imgs=>imgs.forEach(i=>i.loading='eager'));
 await page.waitForFunction(()=>[...document.images].every(i=>i.complete&&i.naturalWidth>0)&&[...document.querySelectorAll('audio')].every(a=>a.readyState>=1));
 const result=await page.evaluate(()=>({title:document.title,images:document.images.length,audio:[...document.querySelectorAll('audio')].map(a=>({duration:a.duration,error:a.error})),horizontalOverflow:document.documentElement.scrollWidth>innerWidth}));
 await page.screenshot({path:'html-desktop.png',fullPage:true});
 await page.setViewportSize({width:390,height:844});
 result.mobileOverflow=await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth);
 await page.screenshot({path:'html-mobile.png',fullPage:true});
 await fs.writeFile('html-validation.json',JSON.stringify(result,null,2));
 console.log(JSON.stringify(result));
}finally{await browser.close()}
