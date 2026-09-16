# Rebuilds assets/banners/*.webp. Usage: python tools/build-banners.py <workdir>
# <workdir>/src/{groot,uermend,qubit,vindicator}.png = original banner art (see git history),
# <workdir>/asimovian.ttf = qubit/fonts/asimovian.woff2 decompressed with fontTools.
import math, sys
from PIL import Image, ImageDraw, ImageFont, ImageFilter
S=sys.argv[1]; OUT='assets/banners'
import os; os.makedirs(OUT, exist_ok=True)
W,H=1600,400
IVORY=(242,232,204,255)

def crop(name, cy):
    im=Image.open(f'{S}/src/{name}.png').convert('RGBA')
    scale=W/im.width; im=im.resize((W,round(im.height*scale)),Image.LANCZOS)
    top=max(0,min(im.height-H,round(im.height*cy-H/2)))
    return im.crop((0,top,W,top+H))

def scrim(im, reach=.46, alpha=170, rgb=(12,14,16)):
    s=Image.new('RGBA',im.size,(0,0,0,0)); d=ImageDraw.Draw(s)
    for x in range(int(W*reach)):
        d.line([(x,0),(x,H)],fill=rgb+(int(alpha*(1-x/(W*reach))**1.5),))
    return Image.alpha_composite(im,s)

def text(im, xy, txt, font, fill=IVORY, spacing=0, shadow=True):
    layer=Image.new('RGBA',im.size,(0,0,0,0)); d=ImageDraw.Draw(layer)
    x,y=xy
    if shadow:
        sh=Image.new('RGBA',im.size,(0,0,0,0)); sd=ImageDraw.Draw(sh); cx=x
        for ch in txt:
            sd.text((cx+3,y+5),ch,font=font,fill=(0,0,0,190)); cx+=font.getlength(ch)+spacing
        im=Image.alpha_composite(im,sh.filter(ImageFilter.GaussianBlur(6)))
    for ch in txt:
        d.text((x,y),ch,font=font,fill=fill); x+=font.getlength(ch)+spacing
    return Image.alpha_composite(im,layer), x

def vcenter(font, txt):
    b=font.getbbox(txt); return H/2-(b[1]+b[3])/2

def save(im,name):
    im.convert('RGB').save(f'{OUT}/{name}.webp','WEBP',quality=80,method=6)

# Groot: Baskerville wordmark, amber dot (groot page brand)
im=scrim(crop('groot',.47),alpha=120)
f=ImageFont.truetype('C:/Windows/Fonts/BASKVILL.TTF',150)
im,x=text(im,(80,vcenter(f,'Groot')),'Groot',f,spacing=-6)
im,_=text(im,(x-2,vcenter(f,'Groot')),'.',f,fill=(214,150,48,255))
save(im,'groot')

# Uermend: Bungee + gold hexagon (UrmendUiTheme)
im=scrim(crop('uermend',.52))
f=ImageFont.truetype('P:/Uermend/assets/fonts/Bungee-Regular.ttf',118)
hs=44; hx,hy=80+hs,H/2
d=Image.new('RGBA',im.size,(0,0,0,0)); ImageDraw.Draw(d).polygon([(hx+hs*math.cos(math.radians(a)),hy+hs*math.sin(math.radians(a))) for a in range(0,360,60)],fill=(221,181,74,255))
im=Image.alpha_composite(im,d)
im,_=text(im,(80+2*hs+34,vcenter(f,'UERMEND')),'UERMEND',f)
save(im,'uermend')

# Qubit: app icon + Asimovian
im=scrim(crop('qubit',.46),alpha=200)
icon=Image.open('assets/qubit-icon.png').convert('RGBA').resize((150,150),Image.LANCZOS)
im.alpha_composite(icon,(80,H//2-75))
f=ImageFont.truetype(sys.argv[1]+'/asimovian.ttf',120)
im,_=text(im,(80+150+36,vcenter(f,'Qubit')),'Qubit',f,fill=(236,244,238,255))
save(im,'qubit')

# Vindicator: Asimovian, spaced caps, signal blue rule
im=scrim(crop('vindicator',.56),reach=.55,alpha=210)
f=ImageFont.truetype(sys.argv[1]+'/asimovian.ttf',92)
im,x=text(im,(80,vcenter(f,'VINDICATOR')),'VINDICATOR',f,spacing=10)
r=Image.new('RGBA',im.size,(0,0,0,0)); ImageDraw.Draw(r).rectangle([80,H/2+66,80+120,H/2+72],fill=(131,195,255,255))
im=Image.alpha_composite(im,r)
save(im,'vindicator')

# Boomhut journey plate
j=Image.open('boomhut/images/boomhut-journey.png').convert('RGB')
j.resize((1600,round(j.height*1600/j.width)),Image.LANCZOS).save('boomhut/images/boomhut-journey.webp','WEBP',quality=82,method=6)
print('banners ok')
