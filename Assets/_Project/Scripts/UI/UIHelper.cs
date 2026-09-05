using UnityEngine;
using UnityEngine.UI;
public static class UIHelper{
static Sprite _spriteBlanco;
public static Sprite SpriteBlanco{get{
if(_spriteBlanco==null){
Texture2D tex=new Texture2D(4,4,TextureFormat.RGBA32,false);
Color[] px=new Color[16];for(int i=0;i<16;i++)px[i]=Color.white;
tex.SetPixels(px);tex.Apply();
_spriteBlanco=Sprite.Create(tex,new Rect(0,0,4,4),new Vector2(.5f,.5f),4f);}
return _spriteBlanco;}}
static Font _fuente;
public static Font Fuente{get{
if(_fuente==null)_fuente=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
return _fuente;}}
public static void AsegurarEventSystem(){
if(UnityEngine.EventSystems.EventSystem.current==null&&Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>()==null){
GameObject es=new GameObject("_EventSystem");
es.AddComponent<UnityEngine.EventSystems.EventSystem>();
es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
}}
public static RectTransform CrearRaiz(GameObject go){
RectTransform rt=go.AddComponent<RectTransform>();
rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;
rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;return rt;}
public static GameObject CrearPanel(Transform p,string n,
Vector2 am,Vector2 aM,Vector2 pv,Vector2 pos,Vector2 tam,Color c){
GameObject go=new GameObject(n,typeof(RectTransform));
go.transform.SetParent(p,false);
Image img=go.AddComponent<Image>();img.color=c;img.sprite=SpriteBlanco;
RectTransform rt=go.GetComponent<RectTransform>();
rt.anchorMin=am;rt.anchorMax=aM;rt.pivot=pv;
rt.anchoredPosition=pos;rt.sizeDelta=tam;return go;}
public static Image CrearRelleno(GameObject p,string n,Color c,float f){
GameObject go=new GameObject(n,typeof(RectTransform));
go.transform.SetParent(p.transform,false);
Image img=go.AddComponent<Image>();img.sprite=SpriteBlanco;
img.color=c;img.type=Image.Type.Filled;
img.fillMethod=Image.FillMethod.Horizontal;img.fillAmount=f;
RectTransform rt=img.GetComponent<RectTransform>();
rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;
rt.offsetMin=new Vector2(2f,2f);rt.offsetMax=new Vector2(-2f,-2f);return img;}
public static Text CrearTexto(Transform p,string n,string t,int sz,
FontStyle est,Color c,TextAnchor al,Vector2 am,Vector2 aM,Vector2 pv,Vector2 pos,Vector2 tam){
GameObject go=new GameObject(n,typeof(RectTransform));go.transform.SetParent(p,false);
Text tx=go.AddComponent<Text>();tx.text=t;tx.fontSize=sz;
tx.fontStyle=est;tx.font=Fuente;tx.color=c;tx.alignment=al;
tx.horizontalOverflow=HorizontalWrapMode.Overflow;
tx.verticalOverflow=VerticalWrapMode.Overflow;
RectTransform rt=tx.GetComponent<RectTransform>();
rt.anchorMin=am;rt.anchorMax=aM;rt.pivot=pv;
rt.anchoredPosition=pos;rt.sizeDelta=tam;return tx;}
public static Text CrearTexto(GameObject p,string n,string t,int sz,
FontStyle est,Color c,TextAnchor al,Vector2 am,Vector2 aM,Vector2 pv,Vector2 pos,Vector2 tam){return CrearTexto(p!=null?p.transform:null,n,t,sz,est,c,al,am,aM,pv,pos,tam);}
public static Text CrearTexto(GameObject p,string n,string t,int sz,FontStyle est,Color c,
TextAnchor al,Vector2 am,Vector2 aM){return CrearTexto(p!=null?p.transform:null,n,t,sz,est,c,al,am,aM,Vector2.one*.5f,Vector2.zero,Vector2.zero);}
public static Text CrearTexto(GameObject p,string n,string t,int sz,FontStyle est,Color c,
TextAnchor al,Vector2 am,Vector2 aM,Vector2 pos,Vector2 tam){return CrearTexto(p!=null?p.transform:null,n,t,sz,est,c,al,am,aM,Vector2.one*.5f,pos,tam);}
public static Text CrearTexto(Transform p,string n,string t,int sz,FontStyle est,Color c,
TextAnchor al,Vector2 am,Vector2 aM){return CrearTexto(p,n,t,sz,est,c,al,am,aM,Vector2.one*.5f,Vector2.zero,Vector2.zero);}
public static Text CrearTexto(Transform p,string n,string t,int sz,FontStyle est,Color c,
TextAnchor al,Vector2 am,Vector2 aM,Vector2 pos,Vector2 tam){return CrearTexto(p,n,t,sz,est,c,al,am,aM,Vector2.one*.5f,pos,tam);}
public static void AgregarBorde(GameObject go,Color c){
Outline o=go.GetComponent<Outline>();
if(o==null)o=go.AddComponent<Outline>();
o.effectColor=c;o.effectDistance=new Vector2(1f,1f);}
public static void CrearBoton(GameObject p,string n,string tx,
Vector2 am,Vector2 aM,Vector2 pv,Vector2 pos,Vector2 tam,
Color cF,Color cB,UnityEngine.Events.UnityAction acc){
AsegurarEventSystem();
GameObject btn=new GameObject(n,typeof(RectTransform));
btn.transform.SetParent(p.transform,false);
Image img=btn.AddComponent<Image>();img.color=cF;img.sprite=SpriteBlanco;
Button b=btn.AddComponent<Button>();
if(acc!=null)b.onClick.AddListener(acc);
RectTransform rt=b.GetComponent<RectTransform>();
rt.anchorMin=am;rt.anchorMax=aM;rt.pivot=pv;rt.anchoredPosition=pos;rt.sizeDelta=tam;
Outline ob=btn.AddComponent<Outline>();ob.effectColor=cB;ob.effectDistance=new Vector2(1f,1f);
GameObject txGO=new GameObject("Texto",typeof(RectTransform));
txGO.transform.SetParent(btn.transform,false);
Text txt=txGO.AddComponent<Text>();txt.text=tx;txt.fontSize=15;
txt.fontStyle=FontStyle.Bold;txt.font=Fuente;txt.color=Color.white;
txt.alignment=TextAnchor.MiddleCenter;
RectTransform rtTxt=txt.GetComponent<RectTransform>();
rtTxt.anchorMin=Vector2.zero;rtTxt.anchorMax=Vector2.one;
rtTxt.offsetMin=Vector2.zero;rtTxt.offsetMax=Vector2.zero;}}
