// QA driver observes rendered pixels and sends normal browser key events only.
(()=>{
let lethal=false,probeEntered=false,pauseProbe=false,pauseSent=false,probeLockUntil=0;let geometry,active=false,previous=null,lastSample=0,jumpUntil=0,lastJump=0,attackAt=0,start=0,killed=new Set(),lastDeaths=0,trace=[];
const W=520,H=600,X=180,Y=0,bytes=new Uint8Array(W*H*4),mask=new Uint8Array(W*H),queue=new Int32Array(W*H);
fetch('/qa/LevelGeometry.json').then(r=>r.json()).then(v=>geometry=v);
window.qaAdaptiveStop=()=>{active=false;window.qaInput?.('KeyD',false);window.qaInput?.('Space',false);window.qaInput?.('KeyJ',false);};
window.qaObserve=(gl,cam,t)=>{
 if(!gl||!cam||t-lastSample<45)return;lastSample=t;if(gl.drawingBufferWidth!==960||gl.drawingBufferHeight!==600)return;
 gl.readPixels(X,Y,W,H,gl.RGBA,gl.UNSIGNED_BYTE,bytes);mask.fill(0);
 for(let i=0;i<W*H;i++){let j=i*4,r=bytes[j],g=bytes[j+1],b=bytes[j+2];if(g>170&&b>170&&g-r>25&&Math.abs(g-b)<50)mask[i]=1;}
 let candidates=[];
 for(let i=0;i<mask.length;i++){if(mask[i]!==1)continue;let n=0,k=0;queue[n++]=i;mask[i]=2;let minX=W,maxX=0,minY=H,maxY=0;
 while(k<n){let q=queue[k++],x=q%W,y=(q/W)|0;minX=Math.min(x,minX);maxX=Math.max(x,maxX);minY=Math.min(y,minY);maxY=Math.max(y,maxY);for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){let nx=x+dx,ny=y+dy;if(nx<0||nx>=W||ny<0||ny>=H)continue;let ni=ny*W+nx;if(mask[ni]===1){mask[ni]=2;queue[n++]=ni;}}}
 let w=maxX-minX+1,h=maxY-minY+1;if(w>=25&&w<=62&&h>=12&&h<=48&&n>180){let px=X+(minX+maxX)/2,py=Y+(minY+maxY)/2;let x=cam.x+(px-480)/60,y=cam.y+(py-300)/60-.72;candidates.push({x,y,w,h,n,px,py});}
 }
 const prediction=previous?{x:previous.x+(active?7.6:0)*(t-previous.t)/1000,y:previous.y}:{x:-134,y:38.5};
 candidates.sort((a,b)=>(Math.abs(a.x-prediction.x)+Math.abs(a.y-prediction.y)*.5)-(Math.abs(b.x-prediction.x)+Math.abs(b.y-prediction.y)*.5));let p=candidates[0];if(active&&(window.qaAudioSources||[]).some(s=>s.created>start&&s.duration>.619&&s.duration<.621)){window.qaAdaptiveStop();fetch('/qa/results',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({kind:'metrics',summary:{adaptiveTrace:trace,completed:true},intervals:[],callbacks:[]})});return;}if(!p){window.qaAdaptiveState={active,cam,candidates};return;}
 let vy=previous?(p.y-previous.y)/((t-previous.t)/1000):0;previous={...p,t};
 let floor=geometry?.platforms.find(f=>p.x>=f.minX-.2&&p.x<=f.maxX+.2&&Math.abs(p.y-f.top-1)<.26);let grounded=!!floor&&Math.abs(vy)<3;
 window.qaAdaptiveState={active,x:p.x,y:p.y,vy,grounded,floor:floor?.name,candidates:candidates.slice(0,3),killed:[...killed],time:(t-start)/1000};
 if(!active||!geometry)return;
 trace.push({t,x:p.x,y:p.y,grounded,floor:floor?.name});
 const audio=window.qaAudioSources||[];let deaths=audio.filter(s=>s.duration>.219&&s.duration<.221).length;if(deaths>lastDeaths){let bot=geometry.objects.filter(o=>o.type==='PatrolEnemy'&&!killed.has(o.name)).sort((a,b)=>Math.abs(a.x-p.x)-Math.abs(b.x-p.x))[0];if(bot)killed.add(bot.name);lastDeaths=deaths;}
 if(audio.some(s=>s.created>start&&s.duration>.619&&s.duration<.621)){window.qaAdaptiveStop();fetch('/qa/results',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({kind:'metrics',summary:{adaptiveTrace:trace,completed:true},intervals:[],callbacks:[]})});return;}
 if(t-start>160000){window.qaAdaptiveStop();return;}
 let land=(window.qaAudioSources||[]).filter(s=>s.duration>.089&&s.duration<.091)[0];let elapsed=(t-(land?.created||start))/1000;let movingOffset=(base)=>{let v=(elapsed*2+3)%12;return base+(v<=6?v-3:9-v);};
 if(lethal&&pauseSent&&t<probeLockUntil)return;
 if(lethal&&p.x>-92&&p.x<-87){probeEntered=true;if(pauseProbe&&!pauseSent&&p.x>=-90.4){pauseSent=true;probeLockUntil=t+11000;window.qaInput('KeyD',false);setTimeout(()=>{window.qaInput('Escape',true);setTimeout(()=>window.qaInput('Escape',false),40);},250);setTimeout(()=>{window.qaInput('Escape',true);setTimeout(()=>window.qaInput('Escape',false),40);},8250);}window.qaInput('KeyD',p.x<-90.4);window.qaInput('Space',false);return;}if(lethal&&probeEntered&&p.x<-130){lethal=false;window.qaAdaptiveStop();fetch('/qa/results',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({kind:'metrics',summary:{adaptiveTrace:trace,stationaryProbeRespawn:true,pauseProbe},intervals:[],callbacks:[]})});return;}
 let bot=geometry.objects.filter(o=>o.type==='PatrolEnemy'&&!killed.has(o.name)).map(o=>({...o,now:movingOffset(o.x)})).find(o=>o.now-p.x>-.3&&o.now-p.x<3.2&&Math.abs(o.y-p.y)<1.6);
 window.qaInput('KeyD',!bot||bot.now-p.x>1.65);
 if(bot&&bot.now-p.x<2.5&&t-attackAt>365){attackAt=t;window.qaInput('KeyJ',true);setTimeout(()=>window.qaInput('KeyJ',false),40);}
 let needJump=false;if(grounded&&t-lastJump>750){if(floor.maxX-p.x<1.4)needJump=true;for(let o of geometry.objects){if(o.type!=='HazardDamage'||o.name.includes('PatrolBot')||(lethal&&o.name==='StaticSpikes'))continue;let moving=o.name.includes('Moving'), minX=moving?movingOffset(o.x)-.65:o.minX;let dx=minX-p.x;if(dx>-.1&&dx<(moving?2.7:1.8)&&Math.abs(o.minY-floor.top)<.3)needJump=true;}}
 if(needJump){lastJump=t;jumpUntil=t+740;window.qaInput('Space',true);}else if(t>jumpUntil)window.qaInput('Space',false);
};
document.addEventListener('DOMContentLoaded',()=>{document.getElementById('pause-probe').onclick=()=>{pauseProbe=true;pauseSent=false;document.getElementById('lethal').click();};document.getElementById('lethal').onclick=()=>{lethal=true;probeEntered=false;document.getElementById('adaptive').click();};document.getElementById('adaptive').onclick=()=>{window.qaAdaptiveStop();previous=null;killed.clear();trace=[];lastDeaths=(window.qaAudioSources||[]).filter(s=>s.duration>.219&&s.duration<.221).length;start=performance.now();active=true;document.querySelector('canvas').focus();};});
})();
